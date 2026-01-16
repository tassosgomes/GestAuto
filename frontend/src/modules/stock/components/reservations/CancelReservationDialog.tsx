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
import { Textarea } from '@/components/ui/textarea';
import { Button } from '@/components/ui/button';
import { useCancelReservation } from '../../hooks/useReservations';
import { useToast } from '@/hooks/use-toast';
import { useAuth } from '@/auth/useAuth';
import type { ReservationListItem } from '../../types';
import { canUserCancelReservation } from '../../utils/reservationUtils';

const cancelReservationSchema = z.object({
  reason: z.string().min(10, 'Motivo deve ter pelo menos 10 caracteres'),
});

type CancelReservationFormValues = z.infer<typeof cancelReservationSchema>;

interface CancelReservationDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  reservation: ReservationListItem;
}

export function CancelReservationDialog({
  open,
  onOpenChange,
  reservation,
}: CancelReservationDialogProps) {
  const { toast } = useToast();
  const authState = useAuth();
  const cancelReservation = useCancelReservation();

  const canCancel = authState.session.isAuthenticated
    ? canUserCancelReservation(
        reservation.salesPersonId,
        authState.session.roles,
        authState.session.username ?? ''
      )
    : false;

  const form = useForm<CancelReservationFormValues>({
    resolver: zodResolver(cancelReservationSchema),
    defaultValues: {
      reason: '',
    },
  });

  const onSubmit = (data: CancelReservationFormValues) => {
    cancelReservation.mutate(
      {
        reservationId: reservation.id,
        data: { reason: data.reason },
      },
      {
        onSuccess: () => {
          toast({
            title: 'Reserva cancelada',
            description: 'A reserva foi cancelada com sucesso.',
          });
          onOpenChange(false);
          form.reset();
        },
        onError: (error) => {
          toast({
            title: 'Erro ao cancelar reserva',
            description: 'Ocorreu um erro ao tentar cancelar a reserva.',
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
          <DialogTitle>Cancelar Reserva</DialogTitle>
          <DialogDescription>
            {reservation.vehicleMake} {reservation.vehicleModel} {reservation.vehiclePlate && `(${reservation.vehiclePlate})`}
          </DialogDescription>
        </DialogHeader>

        {!canCancel ? (
          <div className="text-sm text-muted-foreground py-4">
            Você não tem permissão para cancelar esta reserva.
          </div>
        ) : (
          <Form {...form}>
            <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
              <div className="text-sm text-muted-foreground">
                Tem certeza que deseja cancelar esta reserva? Esta ação não pode ser desfeita.
              </div>

              <FormField
                control={form.control}
                name="reason"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Motivo do Cancelamento</FormLabel>
                    <FormControl>
                      <Textarea
                        placeholder="Descreva o motivo do cancelamento..."
                        rows={3}
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
                >
                  Voltar
                </Button>
                <Button
                  type="submit"
                  variant="destructive"
                  disabled={cancelReservation.isPending}
                >
                  {cancelReservation.isPending ? 'Cancelando...' : 'Confirmar Cancelamento'}
                </Button>
              </DialogFooter>
            </form>
          </Form>
        )}
      </DialogContent>
    </Dialog>
  );
}
