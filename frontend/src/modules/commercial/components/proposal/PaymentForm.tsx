import { useFormContext } from 'react-hook-form';
import {
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from '@/components/ui/form';
import { Input } from '@/components/ui/input';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { usePaymentMethods } from '../../hooks/usePaymentMethods';

export function PaymentForm() {
  const { control } = useFormContext();
  const { data: paymentMethods, isLoading: isLoadingPaymentMethods } = usePaymentMethods();

  return (
    <Card>
      <CardHeader>
        <CardTitle>Condições de Pagamento</CardTitle>
      </CardHeader>
      <CardContent className="grid gap-4 md:grid-cols-2">
        <FormField
          control={control}
          name="paymentMethod"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Forma de Pagamento</FormLabel>
              <Select 
                onValueChange={field.onChange} 
                defaultValue={field.value}
                disabled={isLoadingPaymentMethods}
              >
                <FormControl>
                  <SelectTrigger>
                    <SelectValue placeholder={
                      isLoadingPaymentMethods 
                        ? "Carregando..." 
                        : "Selecione..."
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

        <FormField
          control={control}
          name="downPayment"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Entrada (R$)</FormLabel>
              <FormControl>
                <Input type="number" placeholder="0.00" {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={control}
          name="installments"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Parcelas</FormLabel>
              <Select
                onValueChange={(value) => field.onChange(parseInt(value))}
                defaultValue={field.value?.toString()}
              >
                <FormControl>
                  <SelectTrigger>
                    <SelectValue placeholder="Selecione..." />
                  </SelectTrigger>
                </FormControl>
                <SelectContent>
                  <SelectItem value="12">12x</SelectItem>
                  <SelectItem value="24">24x</SelectItem>
                  <SelectItem value="36">36x</SelectItem>
                  <SelectItem value="48">48x</SelectItem>
                  <SelectItem value="60">60x</SelectItem>
                </SelectContent>
              </Select>
              <FormMessage />
            </FormItem>
          )}
        />
      </CardContent>
    </Card>
  );
}
