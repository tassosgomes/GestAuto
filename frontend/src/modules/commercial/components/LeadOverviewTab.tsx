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
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import type { Lead } from '../types';
import { useQualifyLead } from '../hooks/useLeads';
import { useToast } from '@/hooks/use-toast';

// I don't have Switch component installed yet. I'll use a checkbox or install switch.
// Let's use Checkbox for now or just a Select for Yes/No if I want to be quick, 
// but Switch is better. I'll try to install switch later or use a simple checkbox.
// Actually, I'll use a simple native checkbox or install switch.
// Let's install switch.

const qualifySchema = z.object({
  paymentMethod: z.string().optional(),
  expectedPurchaseDate: z.string().optional(),
  hasTradeInVehicle: z.boolean(),
  tradeInBrand: z.string().optional(),
  tradeInModel: z.string().optional(),
  tradeInYear: z.string().optional(),
  tradeInMileage: z.string().optional(),
});

type QualifyFormValues = z.infer<typeof qualifySchema>;

interface LeadOverviewTabProps {
  lead: Lead;
}

export function LeadOverviewTab({ lead }: LeadOverviewTabProps) {
  const { toast } = useToast();
  const qualifyLead = useQualifyLead();

  const form = useForm<QualifyFormValues>({
    resolver: zodResolver(qualifySchema),
    defaultValues: {
      paymentMethod: lead.qualification?.paymentMethod || '',
      expectedPurchaseDate: lead.qualification?.expectedPurchaseDate || '',
      hasTradeInVehicle: lead.qualification?.hasTradeInVehicle || false,
      tradeInBrand: lead.qualification?.tradeInVehicle?.brand || '',
      tradeInModel: lead.qualification?.tradeInVehicle?.model || '',
      tradeInYear: lead.qualification?.tradeInVehicle?.year?.toString() || '',
      tradeInMileage: lead.qualification?.tradeInVehicle?.mileage?.toString() || '',
    },
  });

  const hasTradeIn = form.watch('hasTradeInVehicle');

  const onSubmit = (data: QualifyFormValues) => {
    qualifyLead.mutate(
      {
        id: lead.id,
        data: {
          paymentMethod: data.paymentMethod,
          expectedPurchaseDate: data.expectedPurchaseDate,
          hasTradeInVehicle: data.hasTradeInVehicle,
          interestedInTestDrive: false, // Default for now
          tradeInVehicle: data.hasTradeInVehicle
            ? {
                brand: data.tradeInBrand,
                model: data.tradeInModel,
                year: data.tradeInYear ? parseInt(data.tradeInYear) : 0,
                mileage: data.tradeInMileage ? parseInt(data.tradeInMileage) : 0,
                hasDealershipServiceHistory: false,
              }
            : undefined,
        },
      },
      {
        onSuccess: () => {
          toast({
            title: 'Qualificação atualizada',
            description: 'Os dados de qualificação foram salvos com sucesso.',
          });
        },
        onError: () => {
          toast({
            title: 'Erro ao atualizar',
            description: 'Não foi possível salvar a qualificação.',
            variant: 'destructive',
          });
        },
      }
    );
  };

  return (
    <div className="grid gap-6 md:grid-cols-2">
      <Card>
        <CardHeader>
          <CardTitle>Qualificação do Lead</CardTitle>
        </CardHeader>
        <CardContent>
          <Form {...form}>
            <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
              <FormField
                control={form.control}
                name="paymentMethod"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Forma de Pagamento</FormLabel>
                    <Select
                      onValueChange={field.onChange}
                      defaultValue={field.value}
                    >
                      <FormControl>
                        <SelectTrigger>
                          <SelectValue placeholder="Selecione..." />
                        </SelectTrigger>
                      </FormControl>
                      <SelectContent>
                        <SelectItem value="CASH">À Vista</SelectItem>
                        <SelectItem value="FINANCING">Financiamento</SelectItem>
                        <SelectItem value="CONSORTIUM">Consórcio</SelectItem>
                      </SelectContent>
                    </Select>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="expectedPurchaseDate"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Prazo de Compra</FormLabel>
                    <Select
                      onValueChange={field.onChange}
                      defaultValue={field.value}
                    >
                      <FormControl>
                        <SelectTrigger>
                          <SelectValue placeholder="Selecione..." />
                        </SelectTrigger>
                      </FormControl>
                      <SelectContent>
                        <SelectItem value="IMMEDIATE">Imediato</SelectItem>
                        <SelectItem value="15_DAYS">15 Dias</SelectItem>
                        <SelectItem value="30_DAYS_PLUS">30 Dias+</SelectItem>
                      </SelectContent>
                    </Select>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="hasTradeInVehicle"
                render={({ field }) => (
                  <FormItem className="flex flex-row items-center justify-between rounded-lg border p-4">
                    <div className="space-y-0.5">
                      <FormLabel className="text-base">
                        Veículo na Troca?
                      </FormLabel>
                    </div>
                    <FormControl>
                      <input
                        type="checkbox"
                        checked={field.value}
                        onChange={field.onChange}
                        className="h-4 w-4"
                      />
                    </FormControl>
                  </FormItem>
                )}
              />

              {hasTradeIn && (
                <div className="space-y-4 border-l-2 pl-4">
                  <FormField
                    control={form.control}
                    name="tradeInBrand"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Marca</FormLabel>
                        <FormControl>
                          <Input {...field} />
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
                        <FormLabel>Modelo</FormLabel>
                        <FormControl>
                          <Input {...field} />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />
                  <div className="grid grid-cols-2 gap-4">
                    <FormField
                      control={form.control}
                      name="tradeInYear"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>Ano</FormLabel>
                          <FormControl>
                            <Input type="number" {...field} />
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
                          <FormLabel>KM</FormLabel>
                          <FormControl>
                            <Input type="number" {...field} />
                          </FormControl>
                          <FormMessage />
                        </FormItem>
                      )}
                    />
                  </div>
                </div>
              )}

              <Button type="submit" disabled={qualifyLead.isPending}>
                {qualifyLead.isPending ? 'Salvando...' : 'Salvar Qualificação'}
              </Button>
            </form>
          </Form>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Interesse</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="space-y-4">
            <div>
              <span className="font-semibold">Modelo:</span>{' '}
              {lead.interestedModel || 'Não informado'}
            </div>
            <div>
              <span className="font-semibold">Versão:</span>{' '}
              {lead.interestedTrim || 'Não informado'}
            </div>
            <div>
              <span className="font-semibold">Cor:</span>{' '}
              {lead.interestedColor || 'Não informado'}
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
