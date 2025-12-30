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
import { Button } from '@/components/ui/button';
import { useToast } from '@/hooks/use-toast';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { testDriveService } from '../services/testDriveService';
import type { Lead } from '../types';

const testDriveSchema = z.object({
  vehicleId: z.string().min(1, 'Veículo é obrigatório'),
  scheduledAt: z.string().min(1, 'Data e hora são obrigatórias'),
  notes: z.string().optional(),
});

type TestDriveFormValues = z.infer<typeof testDriveSchema>;

interface ScheduleTestDriveDialogProps {
  lead: Lead;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function ScheduleTestDriveDialog({
  lead,
  open,
  onOpenChange,
}: ScheduleTestDriveDialogProps) {
  const { toast } = useToast();
  const queryClient = useQueryClient();
  const [isSubmitting, setIsSubmitting] = useState(false);

  const form = useForm<TestDriveFormValues>({
    resolver: zodResolver(testDriveSchema),
    defaultValues: {
      vehicleId: lead.interestedModel || '',
      scheduledAt: '',
      notes: '',
    },
  });

  const scheduleTestDriveMutation = useMutation({
    mutationFn: testDriveService.schedule,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['test-drives'] });
      queryClient.invalidateQueries({ queryKey: ['lead', lead.id] });
      toast({
        title: 'Test-Drive agendado! 🚗',
        description: 'O test-drive foi agendado com sucesso.',
      });
      onOpenChange(false);
      form.reset();
    },
    onError: (error: Error) => {
      toast({
        title: 'Erro ao agendar',
        description: error.message || 'Não foi possível agendar o test-drive.',
        variant: 'destructive',
      });
    },
    onSettled: () => {
      setIsSubmitting(false);
    },
  });

  const onSubmit = async (data: TestDriveFormValues) => {
    setIsSubmitting(true);
    
    // Converter data/hora local para ISO string
    const scheduledAtISO = new Date(data.scheduledAt).toISOString();
    
    await scheduleTestDriveMutation.mutateAsync({
      leadId: lead.id,
      vehicleId: data.vehicleId,
      scheduledAt: scheduledAtISO,
      notes: data.notes,
    });
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle>Agendar Test-Drive</DialogTitle>
          <DialogDescription>
            Agende um test-drive para o lead {lead.name}
          </DialogDescription>
        </DialogHeader>

        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
            <FormField
              control={form.control}
              name="vehicleId"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Modelo do Veículo *</FormLabel>
                  <FormControl>
                    <Input
                      placeholder="Ex: Tesla Model 3"
                      {...field}
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="scheduledAt"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Data e Hora *</FormLabel>
                  <FormControl>
                    <Input
                      type="datetime-local"
                      {...field}
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="notes"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Observações</FormLabel>
                  <FormControl>
                    <Textarea
                      placeholder="Informações adicionais sobre o test-drive..."
                      className="min-h-[100px]"
                      {...field}
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
                {isSubmitting ? 'Agendando...' : 'Agendar Test-Drive'}
              </Button>
            </DialogFooter>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  );
}
