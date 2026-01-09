import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from '@/components/ui/form';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Checkbox } from '@/components/ui/checkbox';
import { useToast } from '@/hooks/use-toast';
import { useMutation } from '@tanstack/react-query';
import { proposalService } from '../services/proposalService';
import { usePaymentMethods } from '../hooks/usePaymentMethods';
import type { Lead } from '../types';

const proposalSchema = z.object({
  vehicleModel: z.string().optional(),
  vehicleTrim: z.string().optional(),
  vehicleColor: z.string().optional(),
  vehicleYear: z.number().min(2020, 'Ano deve ser 2020 ou superior'),
  vehiclePrice: z.number().min(1, 'Preço é obrigatório'),
  isReadyDelivery: z.boolean(),
  paymentMethod: z.string().optional(),
  downPayment: z.number().optional(),
  installments: z.number().optional(),
  includeTradeIn: z.boolean(),
  tradeInModel: z.string().optional(),
  tradeInYear: z.number().optional(),
  tradeInMileage: z.number().optional(),
  tradeInValue: z.number().optional(),
});

type ProposalFormValues = z.infer<typeof proposalSchema>;

interface CreateProposalDialogProps {
  lead: Lead;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function CreateProposalDialog({
  lead,
  open,
  onOpenChange,
}: CreateProposalDialogProps) {
  const { toast } = useToast();
  const navigate = useNavigate();
  const [isSubmitting, setIsSubmitting] = useState(false);
  const { data: paymentMethods, isLoading: isLoadingPaymentMethods } = usePaymentMethods();

  const form = useForm<ProposalFormValues>({
    resolver: zodResolver(proposalSchema),
    defaultValues: {
      vehicleModel: lead.interestedModel || '',
      vehicleTrim: lead.interestedTrim || '',
      vehicleColor: lead.interestedColor || '',
      vehicleYear: new Date().getFullYear(),
      vehiclePrice: 0,
      isReadyDelivery: true,
      paymentMethod: lead.qualification?.paymentMethod || '',
      downPayment: 0,
      installments: 0,
      includeTradeIn: lead.qualification?.hasTradeInVehicle || false,
      tradeInModel: lead.qualification?.tradeInVehicle?.model || '',
      tradeInYear: lead.qualification?.tradeInVehicle?.year || undefined,
      tradeInMileage: lead.qualification?.tradeInVehicle?.mileage || undefined,
      tradeInValue: 0,
    },
  });

  const includeTradeIn = form.watch('includeTradeIn');

  const createProposalMutation = useMutation({
    mutationFn: proposalService.create,
    onSuccess: (data) => {
      toast({
        title: 'Proposta criada! 📄',
        description: 'A proposta foi criada com sucesso.',
      });
      onOpenChange(false);
      // Navegar para a página de detalhes da proposta
      navigate(`/commercial/proposals/${data.id}/edit`);
    },
    onError: (error: Error) => {
      toast({
        title: 'Erro ao criar proposta',
        description: error.message || 'Não foi possível criar a proposta.',
        variant: 'destructive',
      });
    },
    onSettled: () => {
      setIsSubmitting(false);
    },
  });

  const onSubmit = async (data: ProposalFormValues) => {
    setIsSubmitting(true);

    await createProposalMutation.mutateAsync({
      leadId: lead.id,
      vehicleModel: data.vehicleModel,
      vehicleTrim: data.vehicleTrim,
      vehicleColor: data.vehicleColor,
      vehicleYear: data.vehicleYear,
      isReadyDelivery: data.isReadyDelivery,
      vehiclePrice: data.vehiclePrice,
      paymentMethod: data.paymentMethod,
      downPayment: data.downPayment || 0,
      installments: data.installments || 0,
      tradeIn:
        data.includeTradeIn && data.tradeInModel
          ? {
              model: data.tradeInModel,
              year: data.tradeInYear || 0,
              mileage: data.tradeInMileage || 0,
              value: data.tradeInValue || 0,
            }
          : undefined,
    });
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[600px] max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>Criar Proposta</DialogTitle>
          <DialogDescription>
            Crie uma nova proposta comercial para {lead.name}
          </DialogDescription>
        </DialogHeader>

        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
            {/* Informações do Veículo */}
            <div className="space-y-4">
              <h3 className="font-semibold">Informações do Veículo</h3>
              
              <div className="grid grid-cols-2 gap-4">
                <FormField
                  control={form.control}
                  name="vehicleModel"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Modelo</FormLabel>
                      <FormControl>
                        <Input placeholder="Ex: Model 3" {...field} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <FormField
                  control={form.control}
                  name="vehicleTrim"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Versão</FormLabel>
                      <FormControl>
                        <Input placeholder="Ex: Long Range" {...field} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
              </div>

              <div className="grid grid-cols-2 gap-4">
                <FormField
                  control={form.control}
                  name="vehicleColor"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Cor</FormLabel>
                      <FormControl>
                        <Input placeholder="Ex: Branco Pérola" {...field} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <FormField
                  control={form.control}
                  name="vehicleYear"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Ano *</FormLabel>
                      <FormControl>
                        <Input 
                          type="number" 
                          {...field}
                          onChange={e => field.onChange(parseInt(e.target.value) || 0)}
                        />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
              </div>

              <div className="grid grid-cols-2 gap-4">
                <FormField
                  control={form.control}
                  name="vehiclePrice"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Preço (R$) *</FormLabel>
                      <FormControl>
                        <Input 
                          type="number" 
                          step="0.01" 
                          {...field}
                          onChange={e => field.onChange(parseFloat(e.target.value) || 0)}
                        />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <FormField
                  control={form.control}
                  name="isReadyDelivery"
                  render={({ field }) => (
                    <FormItem className="flex flex-row items-center space-x-3 space-y-0 pt-8">
                      <FormControl>
                        <Checkbox
                          checked={field.value}
                          onCheckedChange={field.onChange}
                        />
                      </FormControl>
                      <div className="space-y-1 leading-none">
                        <FormLabel>Pronta Entrega</FormLabel>
                      </div>
                    </FormItem>
                  )}
                />
              </div>
            </div>

            {/* Condições de Pagamento */}
            <div className="space-y-4">
              <h3 className="font-semibold">Condições de Pagamento</h3>

              <FormField
                control={form.control}
                name="paymentMethod"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Forma de Pagamento</FormLabel>
                    <Select
                      onValueChange={field.onChange}
                      value={field.value}
                      disabled={isLoadingPaymentMethods}
                    >
                      <FormControl>
                        <SelectTrigger>
                          <SelectValue placeholder="Selecione..." />
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

              <div className="grid grid-cols-2 gap-4">
                <FormField
                  control={form.control}
                  name="downPayment"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Entrada (R$)</FormLabel>
                      <FormControl>
                        <Input 
                          type="number" 
                          step="0.01" 
                          {...field}
                          value={field.value || ''}
                          onChange={e => field.onChange(e.target.value ? parseFloat(e.target.value) : undefined)}
                        />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <FormField
                  control={form.control}
                  name="installments"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Parcelas</FormLabel>
                      <FormControl>
                        <Input 
                          type="number" 
                          {...field}
                          value={field.value || ''}
                          onChange={e => field.onChange(e.target.value ? parseInt(e.target.value) : undefined)}
                        />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
              </div>
            </div>

            {/* Veículo de Troca */}
            <div className="space-y-4">
              <FormField
                control={form.control}
                name="includeTradeIn"
                render={({ field }) => (
                  <FormItem className="flex flex-row items-center space-x-3 space-y-0">
                    <FormControl>
                      <Checkbox
                        checked={field.value}
                        onCheckedChange={field.onChange}
                      />
                    </FormControl>
                    <div className="space-y-1 leading-none">
                      <FormLabel>Incluir veículo de troca</FormLabel>
                    </div>
                  </FormItem>
                )}
              />

              {includeTradeIn && (
                <div className="space-y-4 pl-6 border-l-2">
                  <h4 className="font-medium text-sm">Dados do Veículo de Troca</h4>
                  
                  <div className="grid grid-cols-2 gap-4">
                    <FormField
                      control={form.control}
                      name="tradeInModel"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>Modelo</FormLabel>
                          <FormControl>
                            <Input placeholder="Ex: Civic" {...field} />
                          </FormControl>
                          <FormMessage />
                        </FormItem>
                      )}
                    />

                    <FormField
                      control={form.control}
                      name="tradeInYear"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>Ano</FormLabel>
                          <FormControl>
                            <Input 
                              type="number" 
                              {...field}
                              value={field.value || ''}
                              onChange={e => field.onChange(e.target.value ? parseInt(e.target.value) : undefined)}
                            />
                          </FormControl>
                          <FormMessage />
                        </FormItem>
                      )}
                    />
                  </div>

                  <div className="grid grid-cols-2 gap-4">
                    <FormField
                      control={form.control}
                      name="tradeInMileage"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>Quilometragem</FormLabel>
                          <FormControl>
                            <Input 
                              type="number" 
                              {...field}
                              value={field.value || ''}
                              onChange={e => field.onChange(e.target.value ? parseInt(e.target.value) : undefined)}
                            />
                          </FormControl>
                          <FormMessage />
                        </FormItem>
                      )}
                    />

                    <FormField
                      control={form.control}
                      name="tradeInValue"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>Valor (R$)</FormLabel>
                          <FormControl>
                            <Input 
                              type="number" 
                              step="0.01" 
                              {...field}
                              value={field.value || ''}
                              onChange={e => field.onChange(e.target.value ? parseFloat(e.target.value) : undefined)}
                            />
                          </FormControl>
                          <FormMessage />
                        </FormItem>
                      )}
                    />
                  </div>
                </div>
              )}
            </div>

            <DialogFooter>
              <Button
                type="button"
                variant="outline"
                onClick={() => onOpenChange(false)}
                disabled={isSubmitting}
              >
                Cancelar
              </Button>
              <Button type="submit" disabled={isSubmitting}>
                {isSubmitting ? 'Criando...' : 'Criar Proposta'}
              </Button>
            </DialogFooter>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  );
}
