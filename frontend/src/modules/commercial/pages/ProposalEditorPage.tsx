import { useEffect, useState } from 'react';
import { useNavigate, useParams, useSearchParams } from 'react-router-dom';
import { useForm, useWatch } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { ArrowLeft, FileDown, Save, Send, ShoppingCart } from 'lucide-react';

import { Button } from '@/components/ui/button';
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from '@/components/ui/form';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { useToast } from '@/hooks/use-toast';
import { useCreateProposal, useProposal, useUpdateProposal } from '../hooks/useProposals';
import { useLeads } from '../hooks/useLeads';
import { proposalService } from '../services/proposalService';
import { VehicleSelection } from '../components/proposal/VehicleSelection';
import { PaymentForm } from '../components/proposal/PaymentForm';
import { ProposalSummary } from '../components/proposal/ProposalSummary';
import { AccessoriesSection } from '../components/proposal/AccessoriesSection';
import { TradeInSection } from '../components/proposal/TradeInSection';

const proposalSchema = z.object({
  leadId: z.string().uuid({ message: 'Lead é obrigatório' }),
  
  // Vehicle fields
  vehicleModel: z.string().min(1, 'Modelo é obrigatório'),
  vehicleTrim: z.string().optional(),
  vehicleColor: z.string().optional(),
  vehicleYear: z.coerce.number().min(1900, 'Ano inválido'),
  vehiclePrice: z.coerce.number().min(0, 'Preço deve ser positivo'),
  isReadyDelivery: z.boolean().default(true),

  // Accessories
  items: z.array(z.object({
    description: z.string().min(1, 'Descrição obrigatória'),
    value: z.coerce.number().min(0)
  })).optional(),

  // Trade-in
  tradeIn: z.object({
    hasTradeIn: z.boolean().default(false),
    brand: z.string().optional(),
    model: z.string().optional(),
    year: z.coerce.number().optional(),
    mileage: z.coerce.number().optional(),
    plate: z.string().optional(),
    color: z.string().optional(),
    generalCondition: z.string().optional(),
    hasDealershipServiceHistory: z.boolean().optional(),
    value: z.coerce.number().optional(),
    evaluationId: z.string().optional(),
  }).optional(),

  // Payment fields
  paymentMethod: z.enum(['CASH', 'FINANCING', 'CONSORTIUM']),
  downPayment: z.coerce.number().min(0).optional(),
  installments: z.number().optional(),
  discount: z.coerce.number().min(0).optional(),
  discountReason: z.string().optional(),
  
  notes: z.string().optional(),
});

type ProposalFormValues = z.infer<typeof proposalSchema>;

export function ProposalEditorPage() {
  const navigate = useNavigate();
  const { id } = useParams();
  const [searchParams] = useSearchParams();
  const { toast } = useToast();
  const createProposal = useCreateProposal();
  const updateProposal = useUpdateProposal();

  const isEdit = !!id;
  const { data: proposal, isLoading: isLoadingProposal } = useProposal(id ?? '');
  const [isSaving, setIsSaving] = useState(false);

  const leadIdFromQuery = searchParams.get('leadId') ?? '';
  const { data: leadsData, isLoading: isLoadingLeads, isError: isLeadsError } = useLeads({
    page: 1,
    pageSize: 50,
  });

  const methods = useForm<ProposalFormValues>({
    resolver: zodResolver(proposalSchema) as any,
    defaultValues: {
      paymentMethod: 'CASH',
      downPayment: 0,
      isReadyDelivery: true,
      vehicleYear: new Date().getFullYear(),
      items: [],
      tradeIn: {
        hasTradeIn: false,
        hasDealershipServiceHistory: false,
      },
      discount: 0,
      discountReason: '',
      leadId: leadIdFromQuery,
    },
  });

  useEffect(() => {
    if (!isEdit) return;
    if (!proposal) return;

    methods.reset({
      leadId: proposal.leadId,
      vehicleModel: proposal.vehicleModel ?? '',
      vehicleTrim: proposal.vehicleTrim ?? '',
      vehicleColor: proposal.vehicleColor ?? '',
      vehicleYear: proposal.vehicleYear ?? new Date().getFullYear(),
      isReadyDelivery: proposal.isReadyDelivery,
      vehiclePrice: proposal.vehiclePrice ?? 0,
      paymentMethod: (proposal.paymentMethod as any) ?? 'CASH',
      downPayment: proposal.downPayment ?? 0,
      installments: proposal.installments,
      items: (proposal.items ?? []).map((item) => ({
        description: item.description,
        value: item.value,
      })),
      tradeIn: {
        hasTradeIn: (proposal.tradeInValue ?? 0) > 0 || !!proposal.usedVehicleEvaluationId,
        value: proposal.tradeInValue ?? 0,
        evaluationId: proposal.usedVehicleEvaluationId ?? undefined,
        hasDealershipServiceHistory: false,
      },
      discount: proposal.discountAmount ?? 0,
      discountReason: proposal.discountReason ?? '',
      notes: '',
    });
  }, [isEdit, proposal, methods]);

  const watchedVehiclePrice = useWatch({ control: methods.control, name: 'vehiclePrice' }) || 0;
  const watchedDiscount = useWatch({ control: methods.control, name: 'discount' }) || 0;
  const discountPercent = Number(watchedVehiclePrice) > 0 ? (Number(watchedDiscount) / Number(watchedVehiclePrice)) * 100 : 0;
  const isDiscountHigh = discountPercent > 5;

  const syncItems = async (proposalId: string) => {
    const formItems = methods.getValues('items') ?? [];
    const existingItems = proposal?.items ?? [];

    for (const item of existingItems) {
      await proposalService.removeItem(proposalId, item.id);
    }
    for (const item of formItems) {
      if (!item?.description) continue;
      await proposalService.addItem(proposalId, { description: item.description, value: Number(item.value) || 0 });
    }
  };

  const applyDiscountIfNeeded = async (proposalId: string) => {
    const discount = Number(methods.getValues('discount') || 0);
    if (discount <= 0) return;
    const reason = String(methods.getValues('discountReason') || '').trim();
    if (!reason) {
      toast({
        title: 'Motivo do desconto é obrigatório',
        description: 'Informe o motivo para aplicar desconto.',
        variant: 'destructive',
      });
      throw new Error('discountReason required');
    }

    await proposalService.applyDiscount(proposalId, { amount: discount, reason });
  };

  const persist = async (mode: 'save' | 'requestApproval' | 'close') => {
    const isValid = await methods.trigger();
    if (!isValid) return;

    const data = methods.getValues();

    setIsSaving(true);
    try {
      const basePayload = {
        leadId: data.leadId,
        vehicleModel: data.vehicleModel,
        vehicleTrim: data.vehicleTrim,
        vehicleColor: data.vehicleColor,
        vehicleYear: data.vehicleYear,
        isReadyDelivery: data.isReadyDelivery,
        vehiclePrice: data.vehiclePrice,
        paymentMethod: data.paymentMethod,
        downPayment: data.downPayment,
        installments: data.installments,
      };

      let proposalId = id;
      if (!proposalId) {
        const created = await createProposal.mutateAsync(basePayload as any);
        proposalId = created.id;
        navigate(`/commercial/proposals/${proposalId}/edit`, { replace: true });
      } else {
        await updateProposal.mutateAsync({ id: proposalId, data: basePayload as any });
      }

      await syncItems(proposalId);

      if (mode === 'requestApproval') {
        if (!isDiscountHigh) {
          toast({
            title: 'Sem necessidade de aprovação',
            description: 'Aprovação é exigida apenas para desconto acima de 5%.',
          });
          return;
        }
        await applyDiscountIfNeeded(proposalId);
        toast({
          title: 'Aprovação solicitada',
          description: 'A proposta ficou aguardando aprovação de desconto.',
        });
        return;
      }

      await applyDiscountIfNeeded(proposalId);

      if (mode === 'close') {
        const status = proposal?.status;
        if (isDiscountHigh || status === 'AwaitingDiscountApproval') {
          toast({
            title: 'Não é possível fechar venda',
            description: 'Desconto acima de 5% requer aprovação do gerente.',
            variant: 'destructive',
          });
          return;
        }

        await proposalService.close(proposalId);
        toast({ title: 'Venda fechada com sucesso' });
        navigate('/commercial/proposals');
        return;
      }

      toast({ title: 'Proposta salva' });
    } catch (e) {
      toast({
        title: 'Erro ao salvar proposta',
        description: 'Ocorreu um erro ao tentar salvar a proposta.',
        variant: 'destructive',
      });
    } finally {
      setIsSaving(false);
    }
  };

  const title = isEdit ? 'Editar Proposta' : 'Nova Proposta';

  return (
    <Form {...methods}>
      <form onSubmit={(e) => e.preventDefault()} className="space-y-6">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-4">
            <Button
              variant="ghost"
              size="icon"
              onClick={() => navigate('/commercial/proposals')}
              type="button"
            >
              <ArrowLeft className="h-4 w-4" />
            </Button>
            <div>
              <h1 className="text-2xl font-bold tracking-tight">{title}</h1>
              <p className="text-muted-foreground">
                Preencha os dados para criar uma nova proposta comercial.
              </p>
            </div>
          </div>
          <div className="flex gap-2">
            <Button variant="outline" type="button" onClick={() => navigate('/commercial/proposals')}>
              Cancelar
            </Button>
            <Button type="button" variant="outline" onClick={() => void persist('save')} disabled={isSaving || isLoadingProposal}>
              <Save className="mr-2 h-4 w-4" />
              Salvar Rascunho
            </Button>
            <Button type="button" onClick={() => void persist('requestApproval')} disabled={isSaving || !isDiscountHigh}>
              <Send className="mr-2 h-4 w-4" />
              Solicitar Aprovação
            </Button>
            <Button type="button" variant="outline" onClick={() => { toast({ title: 'Abrindo impressão…' }); window.print(); }}>
              <FileDown className="mr-2 h-4 w-4" />
              Gerar PDF
            </Button>
            <Button type="button" onClick={() => void persist('close')} disabled={isSaving || !isEdit || proposal?.status === 'AwaitingDiscountApproval'}>
              <ShoppingCart className="mr-2 h-4 w-4" />
              Fechar Venda
            </Button>
          </div>
        </div>

        <FormField
          control={methods.control}
          name="leadId"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Lead *</FormLabel>
              <Select
                onValueChange={field.onChange}
                value={field.value}
                disabled={isLoadingLeads || isLeadsError}
              >
                <FormControl>
                  <SelectTrigger>
                    <SelectValue
                      placeholder={
                        isLoadingLeads
                          ? 'Carregando leads...'
                          : isLeadsError
                            ? 'Erro ao carregar leads'
                            : 'Selecione um lead'
                      }
                    />
                  </SelectTrigger>
                </FormControl>
                <SelectContent>
                  {leadsData?.items?.map((lead) => (
                    <SelectItem key={lead.id} value={lead.id}>
                      {lead.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
              <FormMessage />
            </FormItem>
          )}
        />

        <div className="grid gap-6 lg:grid-cols-3">
          <div className="lg:col-span-2 space-y-6">
            <VehicleSelection />
            <AccessoriesSection />
            <TradeInSection proposalId={id} />
            <PaymentForm />
          </div>
          
          <div className="lg:col-span-1">
            <ProposalSummary />
          </div>
        </div>
      </form>
    </Form>
  );
}
