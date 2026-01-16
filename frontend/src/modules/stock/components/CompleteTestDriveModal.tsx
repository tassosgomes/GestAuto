import { useState } from 'react';
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
import { Textarea } from '@/components/ui/textarea';
import type { TestDriveListItem, CompleteTestDriveRequest, TestDriveOutcome } from '../types';
import { TestDriveOutcome as TestDriveOutcomeEnum } from '../types';

const completeSchema = z.object({
  outcome: z.number().min(1, 'Resultado é obrigatório'),
  notes: z.string().optional(),
});

interface CompleteTestDriveModalProps {
  testDrive: TestDriveListItem | null;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onComplete: (id: string, data: CompleteTestDriveRequest) => Promise<void>;
}

type CompleteFormValues = z.infer<typeof completeSchema>;

export function CompleteTestDriveModal({
  testDrive,
  open,
  onOpenChange,
  onComplete,
}: CompleteTestDriveModalProps) {
  const [isSubmitting, setIsSubmitting] = useState(false);

  const form = useForm<CompleteFormValues>({
    resolver: zodResolver(completeSchema),
    defaultValues: {
      outcome: undefined,
      notes: '',
    },
  });

  const onSubmit = async (values: CompleteFormValues) => {
    if (!testDrive) return;

    try {
      setIsSubmitting(true);
      await onComplete(testDrive.id, {
        outcome: values.outcome as TestDriveOutcome,
        endedAt: new Date().toISOString(),
        reservation: values.outcome === TestDriveOutcomeEnum.ConvertedToReservation
          ? {
              type: 1,
              contextType: 'Lead',
              contextId: testDrive.leadId,
            }
          : null,
      });
      form.reset();
      onOpenChange(false);
    } catch (error) {
      console.error('Falha ao finalizar test-drive', error);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle>Finalizar Test-Drive</DialogTitle>
          <DialogDescription>
            {testDrive && (
              <>
                <div className="mt-2 space-y-1 text-sm">
                  <p><strong>Cliente:</strong> {testDrive.leadName}</p>
                  <p><strong>Veículo:</strong> {testDrive.vehicleDescription}</p>
                  <p><strong>Agendado:</strong> {new Date(testDrive.scheduledAt).toLocaleString('pt-BR')}</p>
                </div>
              </>
            )}
          </DialogDescription>
        </DialogHeader>
        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
            <FormField
              control={form.control}
              name="outcome"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Resultado do Test-Drive</FormLabel>
                  <Select 
                    onValueChange={(value) => field.onChange(Number(value))} 
                    value={field.value?.toString()}
                  >
                    <FormControl>
                      <SelectTrigger>
                        <SelectValue placeholder="Selecione o resultado" />
                      </SelectTrigger>
                    </FormControl>
                    <SelectContent>
                      <SelectItem value={TestDriveOutcomeEnum.ReturnedToStock.toString()}>
                        Retornou ao Estoque
                      </SelectItem>
                      <SelectItem value={TestDriveOutcomeEnum.ConvertedToReservation.toString()}>
                        Convertido em Reserva
                      </SelectItem>
                    </SelectContent>
                  </Select>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="notes"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Observações (Opcional)</FormLabel>
                  <FormControl>
                    <Textarea 
                      {...field} 
                      placeholder="Observações sobre a finalização do test-drive..."
                      rows={3}
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

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
                {isSubmitting ? 'Finalizando...' : 'Finalizar'}
              </Button>
            </DialogFooter>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  );
}
