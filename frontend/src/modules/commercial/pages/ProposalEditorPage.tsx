import { useNavigate } from 'react-router-dom';
import { useForm, FormProvider } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { ArrowLeft, Save } from 'lucide-react';

import { Button } from '@/components/ui/button';
import { useToast } from '@/hooks/use-toast';
import { useCreateProposal } from '../hooks/useProposals';
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
    model: z.string().optional(),
    year: z.coerce.number().optional(),
    mileage: z.coerce.number().optional(),
    plate: z.string().optional(),
    value: z.coerce.number().optional(),
    evaluationId: z.string().optional(),
  }).optional(),

  // Payment fields
  paymentMethod: z.enum(['CASH', 'FINANCING', 'CONSORTIUM']),
  downPayment: z.coerce.number().min(0).optional(),
  installments: z.number().optional(),
  discount: z.coerce.number().min(0).optional(),
  
  notes: z.string().optional(),
});

type ProposalFormValues = z.infer<typeof proposalSchema>;

export function ProposalEditorPage() {
  const navigate = useNavigate();
  const { toast } = useToast();
  const createProposal = useCreateProposal();

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
      },
      discount: 0,
      // Temporary default lead ID until we have lead selection
      leadId: '00000000-0000-0000-0000-000000000000', 
    },
  });

  const onSubmit = (data: ProposalFormValues) => {
    const tradeIn = data.tradeIn?.hasTradeIn && data.tradeIn.value ? {
      model: data.tradeIn.model || '',
      year: data.tradeIn.year || 0,
      mileage: data.tradeIn.mileage || 0,
      value: data.tradeIn.value,
    } : undefined;

    createProposal.mutate(
      {
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
        items: data.items,
        tradeIn,
        discount: data.discount,
      },
      {
        onSuccess: () => {
          toast({
            title: 'Proposta criada com sucesso!',
            description: 'A proposta foi salva no sistema.',
          });
          navigate('/commercial/proposals');
        },
        onError: () => {
          toast({
            title: 'Erro ao criar proposta',
            description: 'Ocorreu um erro ao tentar salvar a proposta.',
            variant: 'destructive',
          });
        },
      }
    );
  };

  return (
    <FormProvider {...methods}>
      <form onSubmit={methods.handleSubmit(onSubmit)} className="space-y-6">
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
              <h1 className="text-2xl font-bold tracking-tight">Nova Proposta</h1>
              <p className="text-muted-foreground">
                Preencha os dados para criar uma nova proposta comercial.
              </p>
            </div>
          </div>
          <div className="flex gap-2">
            <Button variant="outline" type="button" onClick={() => navigate('/commercial/proposals')}>
              Cancelar
            </Button>
            <Button type="submit" disabled={createProposal.isPending}>
              {createProposal.isPending ? 'Salvando...' : (
                <>
                  <Save className="mr-2 h-4 w-4" />
                  Salvar Proposta
                </>
              )}
            </Button>
          </div>
        </div>

        <div className="grid gap-6 lg:grid-cols-3">
          <div className="lg:col-span-2 space-y-6">
            <VehicleSelection />
            <AccessoriesSection />
            <TradeInSection />
            <PaymentForm />
          </div>
          
          <div className="lg:col-span-1">
            <ProposalSummary />
          </div>
        </div>
      </form>
    </FormProvider>
  );
}
