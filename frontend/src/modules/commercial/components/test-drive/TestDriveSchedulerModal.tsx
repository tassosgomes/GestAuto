import { useMemo, useState } from 'react';
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
import type { ScheduleTestDriveRequest } from '../../types';

type MockLead = {
  id: string;
  name: string;
};

const scheduleSchema = z.object({
  leadId: z.string().min(1, 'Lead é obrigatório'),
  vehicleId: z.string().min(1, 'Veículo é obrigatório'),
  scheduledAt: z.string().min(1, 'Data e hora são obrigatórias'),
  notes: z.string().optional(),
});

interface TestDriveSchedulerModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onSchedule: (data: ScheduleTestDriveRequest) => Promise<void>;
}

export function TestDriveSchedulerModal({
  open,
  onOpenChange,
  onSchedule,
}: TestDriveSchedulerModalProps) {
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [leadSearch, setLeadSearch] = useState('');

  const leads = useMemo<MockLead[]>(
    () => [
      { id: 'lead-1', name: 'João Silva' },
      { id: 'lead-2', name: 'Maria Santos' },
    ],
    [],
  );

  const form = useForm<z.infer<typeof scheduleSchema>>({
    resolver: zodResolver(scheduleSchema),
    defaultValues: {
      leadId: '',
      vehicleId: '',
      scheduledAt: '',
      notes: '',
    },
  });

  const onSubmit = async (values: z.infer<typeof scheduleSchema>) => {
    try {
      setIsSubmitting(true);
      await onSchedule(values);
      form.reset();
      setLeadSearch('');
      onOpenChange(false);
    } catch (error) {
      console.error('Failed to schedule test drive', error);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[425px]">
        <DialogHeader>
          <DialogTitle>Agendar Test-Drive</DialogTitle>
          <DialogDescription>
            Preencha os dados para agendar um novo test-drive.
          </DialogDescription>
        </DialogHeader>
        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
            <FormField
              control={form.control}
              name="leadId"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Lead</FormLabel>
                  <FormControl>
                    <>
                      <Input
                        placeholder="Pesquise e selecione um lead"
                        name={field.name}
                        value={leadSearch}
                        list="test-drive-lead-options"
                        onBlur={field.onBlur}
                        onChange={(e) => {
                          const value = e.target.value;
                          setLeadSearch(value);
                          const matchedLead = leads.find((lead) => lead.name === value);
                          field.onChange(matchedLead?.id ?? '');
                        }}
                        ref={field.ref}
                      />
                      <datalist id="test-drive-lead-options">
                        {/* TODO: Fetch leads from API */}
                        {leads.map((lead) => (
                          <option key={lead.id} value={lead.name} />
                        ))}
                      </datalist>
                    </>
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
            <FormField
              control={form.control}
              name="vehicleId"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Veículo</FormLabel>
                  <Select onValueChange={field.onChange} defaultValue={field.value}>
                    <FormControl>
                      <SelectTrigger>
                        <SelectValue placeholder="Selecione um veículo" />
                      </SelectTrigger>
                    </FormControl>
                    <SelectContent>
                      {/* TODO: Fetch vehicles from API */}
                      <SelectItem value="vehicle-1">Honda Civic Touring</SelectItem>
                      <SelectItem value="vehicle-2">Toyota Corolla Altis</SelectItem>
                    </SelectContent>
                  </Select>
                  <FormMessage />
                </FormItem>
              )}
            />
            <FormField
              control={form.control}
              name="scheduledAt"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Data e Hora</FormLabel>
                  <FormControl>
                    <Input type="datetime-local" {...field} />
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
                    <Textarea {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
            <DialogFooter>
              <Button type="submit" disabled={isSubmitting}>
                {isSubmitting ? 'Agendando...' : 'Agendar'}
              </Button>
            </DialogFooter>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  );
}
