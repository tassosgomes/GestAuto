import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
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
import { Input } from '@/components/ui/input';
import { Checkbox } from '@/components/ui/checkbox';
import { useQualifyLead } from '../hooks/useLeads';
import { usePaymentMethods } from '../hooks/usePaymentMethods';
import { useToast } from '@/hooks/use-toast';
import type { Lead } from '../types';

// Schema de validação com regras condicionais
const qualificationSchema = z.object({
  paymentMethod: z.string().min(1, 'Forma de pagamento é obrigatória'),
  expectedPurchaseDate: z.string().optional(),
  interestedInTestDrive: z.boolean().default(false),
  hasTradeInVehicle: z.boolean().default(false),
  // Campos condicionais do veículo de troca
  tradeInBrand: z.string().optional(),
  tradeInModel: z.string().optional(),
  tradeInYear: z.coerce.number().optional(),
  tradeInMileage: z.coerce.number().optional(),
  tradeInColor: z.string().optional(),
  tradeInLicensePlate: z.string().optional(),
  tradeInGeneralCondition: z.string().optional(),
  tradeInHasDealershipServiceHistory: z.boolean().default(false),
}).refine(
  (data) => {
    // Se hasTradeInVehicle for true, os campos do veículo são obrigatórios
    if (data.hasTradeInVehicle) {
      return (
        data.tradeInBrand &&
        data.tradeInModel &&
        data.tradeInYear &&
        data.tradeInYear > 1900 &&
        data.tradeInMileage !== undefined &&
        data.tradeInMileage >= 0 &&
        data.tradeInGeneralCondition
      );
    }
    return true;
  },
  {
    message: 'Dados do veículo de troca são obrigatórios quando marcado',
    path: ['hasTradeInVehicle'],
  }
);

type QualificationFormValues = z.infer<typeof qualificationSchema>;

interface LeadQualificationFormProps {
  lead: Lead;
  onSuccess?: () => void;
}

export function LeadQualificationForm({
  lead,
  onSuccess,
}: LeadQualificationFormProps) {
  const { toast } = useToast();
  const qualifyLead = useQualifyLead();
  const { data: paymentMethods, isLoading: isLoadingPaymentMethods } = usePaymentMethods();

  const form = useForm<QualificationFormValues>({
    resolver: zodResolver(qualificationSchema) as any,
    defaultValues: {
      paymentMethod: lead.qualification?.paymentMethod || '',
      expectedPurchaseDate: lead.qualification?.expectedPurchaseDate || '',
      interestedInTestDrive: lead.qualification?.interestedInTestDrive || false,
      hasTradeInVehicle: lead.qualification?.hasTradeInVehicle || false,
      tradeInBrand: lead.qualification?.tradeInVehicle?.brand || '',
      tradeInModel: lead.qualification?.tradeInVehicle?.model || '',
      tradeInYear: lead.qualification?.tradeInVehicle?.year || undefined,
      tradeInMileage: lead.qualification?.tradeInVehicle?.mileage || undefined,
      tradeInColor: lead.qualification?.tradeInVehicle?.color || '',
      tradeInLicensePlate: lead.qualification?.tradeInVehicle?.licensePlate || '',
      tradeInGeneralCondition:
        lead.qualification?.tradeInVehicle?.generalCondition || '',
      tradeInHasDealershipServiceHistory:
        lead.qualification?.tradeInVehicle?.hasDealershipServiceHistory || false,
    },
  });

  const hasTradeIn = form.watch('hasTradeInVehicle');

  const onSubmit = (data: QualificationFormValues) => {
    // Converter expectedPurchaseDate de enum para DateTime ISO ou undefined
    let expectedPurchaseDate: string | undefined = undefined;
    
    if (data.expectedPurchaseDate) {
      const now = new Date();
      switch (data.expectedPurchaseDate) {
        case 'IMMEDIATE':
          expectedPurchaseDate = new Date(now.setDate(now.getDate() + 7)).toISOString();
          break;
        case '7_DAYS':
          expectedPurchaseDate = new Date(now.setDate(now.getDate() + 7)).toISOString();
          break;
        case '15_DAYS':
          expectedPurchaseDate = new Date(now.setDate(now.getDate() + 15)).toISOString();
          break;
        case '30_DAYS_PLUS':
          expectedPurchaseDate = new Date(now.setDate(now.getDate() + 30)).toISOString();
          break;
        default:
          // Se já for uma data ISO, usar diretamente
          if (data.expectedPurchaseDate.includes('T') || data.expectedPurchaseDate.includes('-')) {
            expectedPurchaseDate = data.expectedPurchaseDate;
          }
      }
    }

    qualifyLead.mutate(
      {
        id: lead.id,
        data: {
          paymentMethod: data.paymentMethod,
          expectedPurchaseDate,
          interestedInTestDrive: data.interestedInTestDrive,
          hasTradeInVehicle: data.hasTradeInVehicle,
          tradeInVehicle: data.hasTradeInVehicle
            ? {
                brand: data.tradeInBrand,
                model: data.tradeInModel,
                year: data.tradeInYear || 0,
                mileage: data.tradeInMileage || 0,
                color: data.tradeInColor,
                licensePlate: data.tradeInLicensePlate,
                generalCondition: data.tradeInGeneralCondition,
                hasDealershipServiceHistory:
                  data.tradeInHasDealershipServiceHistory,
              }
            : undefined,
        },
      },
      {
        onSuccess: () => {
          toast({
            title: 'Lead qualificado com sucesso! 🎉',
            description: 'A pontuação e prioridade foram atualizadas.',
          });
          onSuccess?.();
        },
        onError: (error: Error) => {
          toast({
            title: 'Erro ao atualizar',
            description:
              error.message || 'Não foi possível salvar a qualificação.',
            variant: 'destructive',
          });
        },
      }
    );
  };

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
        {/* Forma de Pagamento */}
        <FormField
          control={form.control}
          name="paymentMethod"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Forma de Pagamento *</FormLabel>
              <Select 
                onValueChange={field.onChange} 
                value={field.value}
                disabled={isLoadingPaymentMethods}
              >
                <FormControl>
                  <SelectTrigger>
                    <SelectValue placeholder={
                      isLoadingPaymentMethods 
                        ? "Carregando..." 
                        : "Selecione a forma de pagamento"
                    } />
                  </SelectTrigger>
                </FormControl>
                <SelectContent>
                  {paymentMethods?.map((method) => (
                    <SelectItem key={method.code} value={method.code}>
                      {method.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
              <FormMessage />
            </FormItem>
          )}
        />

        {/* Previsão de Compra */}
        <FormField
          control={form.control}
          name="expectedPurchaseDate"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Previsão de Compra</FormLabel>
              <Select onValueChange={field.onChange} value={field.value}>
                <FormControl>
                  <SelectTrigger>
                    <SelectValue placeholder="Selecione o prazo" />
                  </SelectTrigger>
                </FormControl>
                <SelectContent>
                  <SelectItem value="IMMEDIATE">Imediato</SelectItem>
                  <SelectItem value="7_DAYS">7 Dias</SelectItem>
                  <SelectItem value="15_DAYS">15 Dias</SelectItem>
                  <SelectItem value="30_DAYS_PLUS">30 Dias+</SelectItem>
                </SelectContent>
              </Select>
              <FormMessage />
            </FormItem>
          )}
        />

        {/* Interesse em Test Drive */}
        <FormField
          control={form.control}
          name="interestedInTestDrive"
          render={({ field }) => (
            <FormItem className="flex flex-row items-start space-x-3 space-y-0 rounded-md border p-4">
              <FormControl>
                <Checkbox
                  checked={field.value}
                  onCheckedChange={field.onChange}
                />
              </FormControl>
              <div className="space-y-1 leading-none">
                <FormLabel>Interessado em Test-Drive</FormLabel>
              </div>
            </FormItem>
          )}
        />

        {/* Veículo na Troca */}
        <FormField
          control={form.control}
          name="hasTradeInVehicle"
          render={({ field }) => (
            <FormItem className="flex flex-row items-start space-x-3 space-y-0 rounded-md border p-4">
              <FormControl>
                <Checkbox
                  checked={field.value}
                  onCheckedChange={field.onChange}
                />
              </FormControl>
              <div className="space-y-1 leading-none">
                <FormLabel>Possui Veículo na Troca?</FormLabel>
              </div>
            </FormItem>
          )}
        />

        {/* Campos condicionais do veículo de troca */}
        {hasTradeIn && (
          <div className="space-y-4 rounded-md border border-muted p-4">
            <h3 className="text-sm font-semibold">Dados do Veículo de Troca</h3>

            <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
              <FormField
                control={form.control}
                name="tradeInBrand"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Marca *</FormLabel>
                    <FormControl>
                      <Input placeholder="Ex: Volkswagen" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="tradeInModel"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Modelo *</FormLabel>
                    <FormControl>
                      <Input placeholder="Ex: Gol" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </div>

            <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
              <FormField
                control={form.control}
                name="tradeInYear"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Ano *</FormLabel>
                    <FormControl>
                      <Input
                        type="number"
                        placeholder="Ex: 2020"
                        {...field}
                        onChange={(e) =>
                          field.onChange(
                            e.target.value ? Number(e.target.value) : undefined
                          )
                        }
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="tradeInMileage"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Quilometragem (km) *</FormLabel>
                    <FormControl>
                      <Input
                        type="number"
                        placeholder="Ex: 50000"
                        {...field}
                        onChange={(e) =>
                          field.onChange(
                            e.target.value ? Number(e.target.value) : undefined
                          )
                        }
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </div>

            <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
              <FormField
                control={form.control}
                name="tradeInColor"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Cor</FormLabel>
                    <FormControl>
                      <Input placeholder="Ex: Prata" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="tradeInLicensePlate"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Placa</FormLabel>
                    <FormControl>
                      <Input placeholder="Ex: ABC-1234" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </div>

            <FormField
              control={form.control}
              name="tradeInGeneralCondition"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Condição Geral *</FormLabel>
                  <Select onValueChange={field.onChange} value={field.value}>
                    <FormControl>
                      <SelectTrigger>
                        <SelectValue placeholder="Selecione a condição" />
                      </SelectTrigger>
                    </FormControl>
                    <SelectContent>
                      <SelectItem value="Excellent">Excelente</SelectItem>
                      <SelectItem value="Good">Bom</SelectItem>
                      <SelectItem value="Fair">Regular</SelectItem>
                      <SelectItem value="Poor">Ruim</SelectItem>
                    </SelectContent>
                  </Select>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="tradeInHasDealershipServiceHistory"
              render={({ field }) => (
                <FormItem className="flex flex-row items-start space-x-3 space-y-0">
                  <FormControl>
                    <Checkbox
                      checked={field.value}
                      onCheckedChange={field.onChange}
                    />
                  </FormControl>
                  <div className="space-y-1 leading-none">
                    <FormLabel>Possui Histórico de Revisões na Concessionária</FormLabel>
                  </div>
                </FormItem>
              )}
            />
          </div>
        )}

        <Button type="submit" disabled={qualifyLead.isPending} className="w-full">
          {qualifyLead.isPending ? 'Salvando...' : 'Salvar Qualificação'}
        </Button>
      </form>
    </Form>
  );
}
