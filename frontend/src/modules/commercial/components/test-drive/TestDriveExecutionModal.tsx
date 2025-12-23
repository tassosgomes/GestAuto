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
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import type { CompleteTestDriveRequest, TestDrive } from '../../types';

const executionSchema = z.object({
  initialMileage: z.coerce.number().min(0, 'Quilometragem inicial inválida'),
  finalMileage: z.coerce.number().min(0, 'Quilometragem final inválida'),
  fuelLevel: z.string().min(1, 'Nível de combustível é obrigatório'),
  visualObservations: z.string().optional(),
  customerFeedback: z.string().optional(),
}).refine((data) => data.finalMileage >= data.initialMileage, {
  message: "Quilometragem final deve ser maior ou igual à inicial",
  path: ["finalMileage"],
});

interface TestDriveExecutionModalProps {
  testDrive: TestDrive | null;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onComplete: (id: string, data: CompleteTestDriveRequest) => Promise<void>;
}

type ExecutionFormValues = z.infer<typeof executionSchema>;

export function TestDriveExecutionModal({
  testDrive,
  open,
  onOpenChange,
  onComplete,
}: TestDriveExecutionModalProps) {
  const [isSubmitting, setIsSubmitting] = useState(false);

  const form = useForm<ExecutionFormValues>({
    resolver: zodResolver(executionSchema) as any,
    defaultValues: {
      initialMileage: 0,
      finalMileage: 0,
      fuelLevel: '',
      visualObservations: '',
      customerFeedback: '',
    },
  });

  const onSubmit = async (values: ExecutionFormValues) => {
    if (!testDrive) return;

    try {
      setIsSubmitting(true);
      await onComplete(testDrive.id, {
        checklist: {
          initialMileage: values.initialMileage,
          finalMileage: values.finalMileage,
          fuelLevel: values.fuelLevel,
          visualObservations: values.visualObservations,
        },
        customerFeedback: values.customerFeedback,
      });
      form.reset();
      onOpenChange(false);
    } catch (error) {
      console.error('Failed to complete test drive', error);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[600px]">
        <DialogHeader>
          <DialogTitle>Execução de Test-Drive</DialogTitle>
          <DialogDescription>
            Preencha o checklist de retorno e feedback do cliente.
          </DialogDescription>
        </DialogHeader>
        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <FormField
                control={form.control}
                name="initialMileage"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>KM Inicial</FormLabel>
                    <FormControl>
                      <Input type="number" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <FormField
                control={form.control}
                name="finalMileage"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>KM Final</FormLabel>
                    <FormControl>
                      <Input type="number" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </div>

            <FormField
              control={form.control}
              name="fuelLevel"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Nível de Combustível (Retorno)</FormLabel>
                  <Select onValueChange={field.onChange} defaultValue={field.value}>
                    <FormControl>
                      <SelectTrigger>
                        <SelectValue placeholder="Selecione o nível" />
                      </SelectTrigger>
                    </FormControl>
                    <SelectContent>
                      <SelectItem value="Empty">Vazio</SelectItem>
                      <SelectItem value="1/4">1/4</SelectItem>
                      <SelectItem value="1/2">1/2</SelectItem>
                      <SelectItem value="3/4">3/4</SelectItem>
                      <SelectItem value="Full">Cheio</SelectItem>
                    </SelectContent>
                  </Select>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="visualObservations"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Observações Visuais (Avarias, Limpeza)</FormLabel>
                  <FormControl>
                    <Textarea {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="customerFeedback"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Feedback do Cliente</FormLabel>
                  <FormControl>
                    <Textarea {...field} placeholder="O que o cliente achou do carro?" />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <DialogFooter>
              <Button type="submit" disabled={isSubmitting}>
                {isSubmitting ? 'Salvando...' : 'Finalizar Test-Drive'}
              </Button>
            </DialogFooter>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  );
}
