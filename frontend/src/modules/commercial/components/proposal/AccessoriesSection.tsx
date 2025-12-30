import { useFieldArray, useFormContext } from 'react-hook-form';
import { Plus, Trash2 } from 'lucide-react';
import { Button } from '@/components/ui/button';
import {
  FormControl,
  FormField,
  FormItem,
  FormMessage,
} from '@/components/ui/form';
import { Input } from '@/components/ui/input';
import { CurrencyInput } from '@/components/ui/currency-input';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { formatCurrency } from '@/lib/utils';

export function AccessoriesSection() {
  const { control, watch } = useFormContext();
  const { fields, append, remove } = useFieldArray({
    control,
    name: 'items',
  });

  const items = watch('items') || [];
  const totalAccessories = items.reduce((sum: number, item: any) => sum + (Number(item.value) || 0), 0);

  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
        <CardTitle className="text-lg font-medium">Acessórios e Serviços</CardTitle>
        <Button
          type="button"
          variant="outline"
          size="sm"
          onClick={() => append({ description: '', value: 0 })}
        >
          <Plus className="mr-2 h-4 w-4" />
          Adicionar Item
        </Button>
      </CardHeader>
      <CardContent className="space-y-4 pt-4">
        {fields.length === 0 && (
          <p className="text-sm text-muted-foreground text-center py-4">
            Nenhum acessório adicionado.
          </p>
        )}

        {fields.map((field, index) => (
          <div key={field.id} className="flex gap-4 items-start">
            <FormField
              control={control}
              name={`items.${index}.description`}
              render={({ field }) => (
                <FormItem className="flex-1">
                  <FormControl>
                    <Input placeholder="Descrição do item" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={control}
              name={`items.${index}.value`}
              render={({ field }) => (
                <FormItem className="w-40">
                  <FormControl>
                    <CurrencyInput
                      placeholder="0,00"
                      value={field.value}
                      onChange={field.onChange}
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <Button
              type="button"
              variant="ghost"
              size="icon"
              className="mt-0.5"
              onClick={() => remove(index)}
            >
              <Trash2 className="h-4 w-4 text-destructive" />
            </Button>
          </div>
        ))}

        {fields.length > 0 && (
          <div className="flex justify-end pt-4 border-t">
            <div className="text-sm font-medium">
              Total Acessórios: {formatCurrency(totalAccessories)}
            </div>
          </div>
        )}
      </CardContent>
    </Card>
  );
}
