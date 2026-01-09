import { useFormContext } from 'react-hook-form';
import { useMemo, useState } from 'react';
import { Car, Loader2, CheckCircle2 } from 'lucide-react';
import { Button } from '@/components/ui/button';
import {
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
  FormDescription,
} from '@/components/ui/form';
import { Input } from '@/components/ui/input';
import { CurrencyInput } from '@/components/ui/currency-input';
import { Switch } from '@/components/ui/switch';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Textarea } from '@/components/ui/textarea';
import { useToast } from '@/hooks/use-toast';
import { formatCurrency } from '@/lib/utils';
import {
  useRequestUsedVehicleEvaluation,
  useUsedVehicleEvaluation,
  useUsedVehicleEvaluationCustomerResponse,
} from '../../hooks/useUsedVehicleEvaluation';

export function TradeInSection(props: { proposalId?: string }) {
  const { control, watch, setValue } = useFormContext();
  const [rejectionReason, setRejectionReason] = useState('');
  const { toast } = useToast();
  const requestEvaluation = useRequestUsedVehicleEvaluation();
  const customerResponse = useUsedVehicleEvaluationCustomerResponse();
  
  const hasTradeIn = watch('tradeIn.hasTradeIn');
  const tradeInValue = watch('tradeIn.value');
  const evaluationId = watch('tradeIn.evaluationId');
  const tradeInBrand = watch('tradeIn.brand');
  const tradeInModel = watch('tradeIn.model');
  const tradeInYear = watch('tradeIn.year');
  const tradeInMileage = watch('tradeIn.mileage');
  const tradeInPlate = watch('tradeIn.plate');
  const tradeInColor = watch('tradeIn.color');
  const tradeInGeneralCondition = watch('tradeIn.generalCondition');
  const tradeInHasDealershipServiceHistory = watch('tradeIn.hasDealershipServiceHistory');

  const { data: evaluation } = useUsedVehicleEvaluation(evaluationId);

  const canRequestEvaluation = useMemo(() => {
    if (!props.proposalId) return false;
    if (!hasTradeIn) return false;
    return (
      !!tradeInBrand &&
      !!tradeInModel &&
      !!tradeInYear &&
      !!tradeInMileage &&
      !!tradeInPlate &&
      !!tradeInColor &&
      !!tradeInGeneralCondition
    );
  }, [
    props.proposalId,
    hasTradeIn,
    tradeInBrand,
    tradeInModel,
    tradeInYear,
    tradeInMileage,
    tradeInPlate,
    tradeInColor,
    tradeInGeneralCondition,
  ]);

  const handleRequestEvaluation = async () => {
    if (!props.proposalId) {
      toast({
        title: 'Salve a proposta primeiro',
        description: 'Para solicitar avaliação, a proposta precisa existir no sistema.',
        variant: 'destructive',
      });
      return;
    }

    try {
      const result = await requestEvaluation.mutateAsync({
        proposalId: props.proposalId,
        brand: tradeInBrand || '',
        model: tradeInModel || '',
        year: Number(tradeInYear) || 0,
        mileage: Number(tradeInMileage) || 0,
        licensePlate: String(tradeInPlate || ''),
        color: String(tradeInColor || ''),
        generalCondition: String(tradeInGeneralCondition || ''),
        hasDealershipServiceHistory: Boolean(tradeInHasDealershipServiceHistory),
      });
      setValue('tradeIn.evaluationId', result.id);
      toast({ title: 'Avaliação solicitada' });
    } catch (e) {
      toast({
        title: 'Falha ao solicitar avaliação',
        description: 'Verifique os dados e tente novamente.',
        variant: 'destructive',
      });
    }
  };

  const handleCustomerAccept = async (accepted: boolean) => {
    if (!evaluationId) return;

    try {
      const result = await customerResponse.mutateAsync({
        id: evaluationId,
        data: {
          accepted,
          rejectionReason: accepted ? undefined : rejectionReason || undefined,
        },
      });

      if (accepted && result.evaluatedValue != null) {
        setValue('tradeIn.value', result.evaluatedValue);
      }

      toast({ title: accepted ? 'Avaliação aceita' : 'Avaliação recusada' });
    } catch (e) {
      toast({
        title: 'Falha ao registrar resposta do cliente',
        variant: 'destructive',
      });
    }
  };

  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
        <CardTitle className="text-lg font-medium flex items-center gap-2">
          <Car className="h-5 w-5" />
          Veículo na Troca
        </CardTitle>
        <FormField
          control={control}
          name="tradeIn.hasTradeIn"
          render={({ field }) => (
            <FormItem className="flex flex-row items-center space-x-2 space-y-0">
              <FormControl>
                <Switch
                  checked={field.value}
                  onCheckedChange={field.onChange}
                />
              </FormControl>
              <FormLabel className="font-normal">
                Incluir veículo na troca
              </FormLabel>
            </FormItem>
          )}
        />
      </CardHeader>
      
      {hasTradeIn && (
        <CardContent className="space-y-4 pt-4">
          <div className="grid gap-4 md:grid-cols-2">
            <FormField
              control={control}
              name="tradeIn.brand"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Marca</FormLabel>
                  <FormControl>
                    <Input placeholder="Ex: Honda" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={control}
              name="tradeIn.model"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Modelo</FormLabel>
                  <FormControl>
                    <Input placeholder="Ex: Honda Civic" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={control}
              name="tradeIn.year"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Ano</FormLabel>
                  <FormControl>
                    <Input type="number" placeholder="Ex: 2020" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={control}
              name="tradeIn.mileage"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Quilometragem</FormLabel>
                  <FormControl>
                    <Input type="number" placeholder="Ex: 45000" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={control}
              name="tradeIn.plate"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Placa</FormLabel>
                  <FormControl>
                    <Input placeholder="ABC-1234" {...field} className="uppercase" />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={control}
              name="tradeIn.color"
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
              control={control}
              name="tradeIn.generalCondition"
              render={({ field }) => (
                <FormItem className="md:col-span-2">
                  <FormLabel>Estado Geral</FormLabel>
                  <FormControl>
                    <Textarea placeholder="Descreva o estado geral do veículo" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={control}
              name="tradeIn.hasDealershipServiceHistory"
              render={({ field }) => (
                <FormItem className="md:col-span-2 flex flex-row items-center space-x-2 space-y-0">
                  <FormControl>
                    <Switch checked={field.value} onCheckedChange={field.onChange} />
                  </FormControl>
                  <FormLabel className="font-normal">Possui histórico de manutenção na concessionária</FormLabel>
                </FormItem>
              )}
            />
          </div>

          <div className="rounded-lg border bg-muted/50 p-4 mt-4">
            <div className="flex items-center justify-between">
              <div>
                <h4 className="font-medium mb-1">Avaliação do Veículo</h4>
                <p className="text-sm text-muted-foreground">
                  {evaluation?.evaluatedValue != null
                    ? `Avaliação recebida: ${formatCurrency(evaluation.evaluatedValue)}`
                    : tradeInValue
                      ? `Valor definido: ${formatCurrency(tradeInValue)}`
                      : evaluationId
                        ? `Avaliação solicitada (status: ${evaluation?.status ?? 'Requested'})`
                        : 'Solicite uma avaliação para obter o valor de troca.'}
                </p>
              </div>
              
              <Button 
                type="button" 
                onClick={handleRequestEvaluation} 
                disabled={requestEvaluation.isPending || !!evaluationId || !canRequestEvaluation}
                variant={tradeInValue ? "outline" : "default"}
              >
                {requestEvaluation.isPending ? (
                  <>
                    <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                    Solicitando...
                  </>
                ) : evaluationId ? (
                  <>
                    <CheckCircle2 className="mr-2 h-4 w-4 text-green-600" />
                    Solicitado
                  </>
                ) : (
                  "Solicitar Avaliação"
                )}
              </Button>
            </div>

            {evaluation?.evaluatedValue != null && evaluation.customerAccepted == null && (
              <div className="mt-4 space-y-2">
                <div className="text-sm font-medium">Resposta do cliente</div>
                <div className="flex flex-col gap-2 sm:flex-row">
                  <Button type="button" variant="outline" onClick={() => void handleCustomerAccept(true)}>
                    Aceitar
                  </Button>
                  <Button type="button" variant="destructive" onClick={() => void handleCustomerAccept(false)}>
                    Recusar
                  </Button>
                </div>
                <div className="text-sm text-muted-foreground">
                  Para recusar, informe o motivo abaixo.
                </div>
                <Textarea
                  value={rejectionReason}
                  onChange={(e) => setRejectionReason(e.target.value)}
                  placeholder="Motivo da recusa (obrigatório se recusar)"
                />
              </div>
            )}
            
            {tradeInValue > 0 && (
               <FormField
               control={control}
               name="tradeIn.value"
               render={({ field }) => (
                 <FormItem className="mt-4">
                   <FormLabel>Valor de Avaliação</FormLabel>
                   <FormControl>
                     <CurrencyInput 
                        value={field.value}
                        onChange={field.onChange}
                      />
                   </FormControl>
                   <FormDescription>
                     Este valor será abatido do total da proposta.
                   </FormDescription>
                   <FormMessage />
                 </FormItem>
               )}
             />
            )}
          </div>
        </CardContent>
      )}
    </Card>
  );
}
