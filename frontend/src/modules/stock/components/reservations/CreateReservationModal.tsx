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
import { useCreateReservation } from '../../hooks/useReservations';
import { useToast } from '@/hooks/use-toast';
import { ReservationType } from '../../types';
import { toBankDeadlineAtUtc } from '../../types';

const createReservationSchema = z.object({
  type: z.number().min(1).max(3),
  contextType: z.string().min(1, 'Tipo de contexto é obrigatório'),
  contextId: z.string().optional(),
  bankDeadlineAtUtc: z.string().optional(),
}).refine(
  (data) => data.type !== 3 || (data.type === 3 && data.bankDeadlineAtUtc),
  {
    message: "Prazo do banco é obrigatório para reservas 'Aguardando banco'",
    path: ["bankDeadlineAtUtc"],
  }
);

type CreateReservationFormValues = z.infer<typeof createReservationSchema>;

interface CreateReservationModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  vehicleId: string;
  vehicleInfo?: {
    make: string;
    model: string;
    plate?: string;
  };
}

export function CreateReservationModal({
  open,
  onOpenChange,
  vehicleId,
  vehicleInfo,
}: CreateReservationModalProps) {
  const { toast } = useToast();
  const createReservation = useCreateReservation();

  const form = useForm<CreateReservationFormValues>({
    resolver: zodResolver(createReservationSchema),
    defaultValues: {
      type: ReservationType.Standard,
      contextType: 'Lead',
      contextId: '',
      bankDeadlineAtUtc: '',
    },
  });

  const watchType = form.watch('type');

  const onSubmit = (data: CreateReservationFormValues) => {
    const processedData = {
      type: data.type,
      contextType: data.contextType,
      contextId: data.contextId || null,
      bankDeadlineAtUtc: data.type === ReservationType.WaitingBank && data.bankDeadlineAtUtc
        ? toBankDeadlineAtUtc(data.bankDeadlineAtUtc)
        : null,
    };

    createReservation.mutate(
      { vehicleId, data: processedData },
      {
        onSuccess: () => {
          toast({
            title: 'Reserva criada com sucesso',
            description: `Veículo ${vehicleInfo?.make ?? ''} ${vehicleInfo?.model ?? ''} reservado.`,
          });
          onOpenChange(false);
          form.reset();
        },
        onError: (error) => {
          toast({
            title: 'Erro ao criar reserva',
            description: 'Ocorreu um erro ao tentar criar a reserva.',
            variant: 'destructive',
          });
          console.error(error);
        },
      }
    );
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[425px]">
        <DialogHeader>
          <DialogTitle>Nova Reserva</DialogTitle>
          <DialogDescription>
            {vehicleInfo && (
              <span className="text-sm">
                Veículo: {vehicleInfo.make} {vehicleInfo.model}
                {vehicleInfo.plate && ` (${vehicleInfo.plate})`}
              </span>
            )}
          </DialogDescription>
        </DialogHeader>
        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
            <FormField
              control={form.control}
              name="type"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Tipo de Reserva</FormLabel>
                  <Select onValueChange={(value) => field.onChange(Number(value))} defaultValue={String(field.value)}>
                    <FormControl>
                      <SelectTrigger>
                        <SelectValue placeholder="Selecione o tipo" />
                      </SelectTrigger>
                    </FormControl>
                    <SelectContent>
                      <SelectItem value={String(ReservationType.Standard)}>Padrão</SelectItem>
                      <SelectItem value={String(ReservationType.PaidDeposit)}>Entrada paga</SelectItem>
                      <SelectItem value={String(ReservationType.WaitingBank)}>Aguardando banco</SelectItem>
                    </SelectContent>
                  </Select>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="contextType"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Tipo de Contexto</FormLabel>
                  <Select onValueChange={field.onChange} defaultValue={field.value}>
                    <FormControl>
                      <SelectTrigger>
                        <SelectValue placeholder="Selecione o contexto" />
                      </SelectTrigger>
                    </FormControl>
                    <SelectContent>
                      <SelectItem value="Lead">Lead</SelectItem>
                      <SelectItem value="TestDrive">Test Drive</SelectItem>
                      <SelectItem value="Other">Outro</SelectItem>
                    </SelectContent>
                  </Select>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="contextId"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>ID do Contexto (opcional)</FormLabel>
                  <FormControl>
                    <Input placeholder="ID do lead, test drive, etc." {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            {watchType === ReservationType.WaitingBank && (
              <FormField
                control={form.control}
                name="bankDeadlineAtUtc"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Prazo do Banco</FormLabel>
                    <FormControl>
                      <Input
                        type="date"
                        {...field}
                        min={new Date().toISOString().split('T')[0]}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
            )}

            <DialogFooter>
              <Button type="submit" disabled={createReservation.isPending}>
                {createReservation.isPending ? 'Criando...' : 'Criar Reserva'}
              </Button>
            </DialogFooter>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  );
}
