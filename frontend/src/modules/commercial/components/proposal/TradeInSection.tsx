import { useState } from 'react';
import { useFormContext } from 'react-hook-form';
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
import { Switch } from '@/components/ui/switch';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { formatCurrency } from '@/lib/utils';

export function TradeInSection() {
  const { control, watch, setValue } = useFormContext();
  const [isEvaluating, setIsEvaluating] = useState(false);
  
  const hasTradeIn = watch('tradeIn.hasTradeIn');
  const tradeInValue = watch('tradeIn.value');

  const handleEvaluate = () => {
    setIsEvaluating(true);
    // Mock API call for evaluation
    setTimeout(() => {
      // Random value between 30k and 80k
      const mockValue = Math.floor(Math.random() * (80000 - 30000 + 1) + 30000);
      setValue('tradeIn.value', mockValue);
      setValue('tradeIn.evaluationId', crypto.randomUUID());
      setIsEvaluating(false);
    }, 1500);
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
          </div>

          <div className="rounded-lg border bg-muted/50 p-4 mt-4">
            <div className="flex items-center justify-between">
              <div>
                <h4 className="font-medium mb-1">Avaliação do Veículo</h4>
                <p className="text-sm text-muted-foreground">
                  {tradeInValue 
                    ? `Avaliação concluída: ${formatCurrency(tradeInValue)}`
                    : "Solicite uma avaliação para obter o valor de troca."}
                </p>
              </div>
              
              <Button 
                type="button" 
                onClick={handleEvaluate} 
                disabled={isEvaluating || !!tradeInValue}
                variant={tradeInValue ? "outline" : "default"}
              >
                {isEvaluating ? (
                  <>
                    <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                    Avaliando...
                  </>
                ) : tradeInValue ? (
                  <>
                    <CheckCircle2 className="mr-2 h-4 w-4 text-green-600" />
                    Avaliado
                  </>
                ) : (
                  "Solicitar Avaliação"
                )}
              </Button>
            </div>
            
            {tradeInValue > 0 && (
               <FormField
               control={control}
               name="tradeIn.value"
               render={({ field }) => (
                 <FormItem className="mt-4">
                   <FormLabel>Valor de Avaliação (R$)</FormLabel>
                   <FormControl>
                     <Input 
                        type="number" 
                        {...field} 
                        onChange={(e) => field.onChange(parseFloat(e.target.value))}
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
