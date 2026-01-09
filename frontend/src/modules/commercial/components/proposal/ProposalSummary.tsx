import { useFormContext, useWatch } from 'react-hook-form';
import { AlertTriangle } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Separator } from '@/components/ui/separator';
import { Label } from '@/components/ui/label';
import { CurrencyInput } from '@/components/ui/currency-input';
import { Input } from '@/components/ui/input';
import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert';
import { formatCurrency } from '@/lib/utils';

export function ProposalSummary() {
  const { control, setValue } = useFormContext();
  
  const vehiclePrice = useWatch({ control, name: 'vehiclePrice' }) || 0;
  const downPayment = useWatch({ control, name: 'downPayment' }) || 0;
  const installments = useWatch({ control, name: 'installments' });
  const paymentMethod = useWatch({ control, name: 'paymentMethod' });
  
  // New fields
  const items = useWatch({ control, name: 'items' }) || [];
  const tradeInValue = useWatch({ control, name: 'tradeIn.value' }) || 0;
  const discount = useWatch({ control, name: 'discount' }) || 0;
  const discountReason = useWatch({ control, name: 'discountReason' }) || '';

  const totalAccessories = items.reduce((sum: number, item: any) => sum + (Number(item.value) || 0), 0);
  
  // Cálculo correto: Preço base + acessórios
  const subtotal = Number(vehiclePrice) + totalAccessories;
  
  // Total com desconto
  const totalWithDiscount = subtotal - Number(discount);
  
  // Valor a financiar: Total - Entrada - Troca
  const financedAmount = Math.max(0, totalWithDiscount - Number(downPayment) - Number(tradeInValue));
  
  const discountPercentage = vehiclePrice > 0 ? (Number(discount) / Number(vehiclePrice)) * 100 : 0;
  const isDiscountHigh = discountPercentage > 5;

  return (
    <Card className="h-fit sticky top-4">
      <CardHeader>
        <CardTitle>Resumo da Proposta</CardTitle>
      </CardHeader>
      <CardContent className="space-y-4">
        <div className="flex justify-between">
          <span className="text-muted-foreground">Valor do Veículo</span>
          <span className="font-medium">{formatCurrency(vehiclePrice)}</span>
        </div>

        {totalAccessories > 0 && (
          <div className="flex justify-between">
            <span className="text-muted-foreground">Acessórios</span>
            <span className="font-medium">{formatCurrency(totalAccessories)}</span>
          </div>
        )}
        
        <div className="space-y-2 pt-2">
          <div className="flex justify-between items-center">
             <Label htmlFor="discount" className="text-muted-foreground">Desconto</Label>
             <div className="w-40">
                <CurrencyInput 
                  id="discount"
                  placeholder="0,00"
                  value={discount}
                  onChange={(value) => setValue('discount', value)}
                />
             </div>
          </div>
          {Number(discount) > 0 && (
            <div className="space-y-1">
              <Label htmlFor="discountReason" className="text-muted-foreground text-xs">
                Motivo do desconto
              </Label>
              <Input
                id="discountReason"
                placeholder="Ex: Campanha, negociação, concorrência"
                value={discountReason}
                onChange={(e) => setValue('discountReason', e.target.value)}
              />
            </div>
          )}
          {Number(discount) > 0 && (
             <div className="flex justify-end text-xs text-muted-foreground">
                {discountPercentage.toFixed(1)}% do valor do veículo
             </div>
          )}
        </div>

        {isDiscountHigh && (
          <Alert variant="destructive" className="py-2">
            <AlertTriangle className="h-4 w-4" />
            <AlertTitle className="text-sm font-medium">Aprovação Necessária</AlertTitle>
            <AlertDescription className="text-xs">
              Desconto acima de 5% requer aprovação do gerente.
            </AlertDescription>
          </Alert>
        )}
        
        <Separator />
        
        <div className="flex justify-between font-medium">
          <span>Subtotal</span>
          <span>{formatCurrency(totalWithDiscount)}</span>
        </div>
        
        {Number(downPayment) > 0 && (
          <div className="flex justify-between">
            <span className="text-muted-foreground">(-) Entrada</span>
            <span className="font-medium text-green-600">
              {formatCurrency(downPayment)}
            </span>
          </div>
        )}

        {Number(tradeInValue) > 0 && (
          <div className="flex justify-between">
            <span className="text-muted-foreground">(-) Veículo na Troca</span>
            <span className="font-medium text-green-600">
              {formatCurrency(tradeInValue)}
            </span>
          </div>
        )}

        <Separator />

        <div className="flex justify-between">
          <span className="text-muted-foreground">Valor a Financiar</span>
          <span className="font-medium">{formatCurrency(financedAmount)}</span>
        </div>

        {paymentMethod === 'FINANCING' && installments && (
          <div className="rounded-md bg-muted p-3 text-sm">
            <p className="font-medium mb-1">Simulação (Estimada)</p>
            <div className="flex justify-between">
              <span>{installments}x de</span>
              {/* Placeholder calculation - in real app would come from backend */}
              <span>{formatCurrency((financedAmount * 1.02) / installments)}</span>
            </div>
          </div>
        )}

        <div className="pt-4">
          <div className="flex justify-between text-lg font-bold">
            <span>Total da Venda</span>
            <span>{formatCurrency(totalWithDiscount)}</span>
          </div>
          <p className="text-xs text-muted-foreground mt-1">
            Valor final após desconto
          </p>
        </div>
      </CardContent>
    </Card>
  );
}
