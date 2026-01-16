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
import { useExtendReservation } from '../../hooks/useReservations';
import { useToast } from '@/hooks/use-toast';
import { useAuth } from '@/auth/useAuth';
import type { ReservationListItem } from '../../types';
import { canUserExtendReservation, formatReservationDeadline } from '../../utils/reservationUtils';

const extendReservationSchema = z.object({
  newExpiresAtUtc: z.string().min(1, 'Nova data de expiração é obrigatória'),
});

type ExtendReservationFormValues = z.infer<typeof extendReservationSchema>;

interface ExtendReservationDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  reservation: ReservationListItem;
}

export function ExtendReservationDialog({
  open,
  onOpenChange,
  reservation,
}: ExtendReservationDialogProps) {
  const { toast } = useToast();
  const authState = useAuth();
  const extendReservation = useExtendReservation();

  const canExtend = authState.session.isAuthenticated
    ? canUserExtendReservation(authState.session.roles)
    : false;

  const form = useForm<ExtendReservationFormValues>({
    resolver: zodResolver(extendReservationSchema),
    defaultValues: {
      newExpiresAtUtc: '',
    },
  });

  const onSubmit = (data: ExtendReservationFormValues) => {
    // Convert datetime-local to UTC ISO string
    const date = new Date(data.newExpiresAtUtc);
    const utcString = date.toISOString();

    extendReservation.mutate(
      {
        reservationId: reservation.id,
        data: { newExpiresAtUtc: utcString },
      },
      {
        onSuccess: () => {
          toast({
            title: 'Reserva prorrogada',
            description: `Nova validade: ${new Date(utcString).toLocaleDateString('pt-BR')}`,
          });
          onOpenChange(false);
          form.reset();
        },
        onError: (error) => {
          toast({
            title: 'Erro ao prorrogar reserva',
            description: 'Ocorreu um erro ao tentar prorrogar a reserva.',
            variant: 'destructive',
          });
          console.error(error);
        },
      }
    );
  };

  const currentDeadline = reservation.expiresAtUtc
    ? formatReservationDeadline(reservation.expiresAtUtc, reservation.status)
    : '-';

  // Set min datetime to now
  const minDateTime = new Date().toISOString().slice(0, 16);

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[425px]">
        <DialogHeader>
          <DialogTitle>Prorrogar Reserva</DialogTitle>
          <DialogDescription>
            {reservation.vehicleMake} {reservation.vehicleModel} {reservation.vehiclePlate && `(${reservation.vehiclePlate})`}
          </DialogDescription>
        </DialogHeader>

        {!canExtend ? (
          <div className="text-sm text-muted-foreground py-4">
            Você não tem permissão para prorrogar reservas.
          </div>
        ) : (
          <Form {...form}>
            <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
              <div className="text-sm">
                <span className="text-muted-foreground">Validade atual: </span>
                <span className="font-medium">{currentDeadline}</span>
              </div>

              <FormField
                control={form.control}
                name="newExpiresAtUtc"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Nova Data de Expiração</FormLabel>
                    <FormControl>
                      <Input
                        type="datetime-local"
                        {...field}
                        min={minDateTime}
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
                  Cancelar
                </Button>
                <Button
                  type="submit"
                  disabled={extendReservation.isPending}
                >
                  {extendReservation.isPending ? 'Prorrogando...' : 'Prorrogar'}
                </Button>
              </DialogFooter>
            </form>
          </Form>
        )}
      </DialogContent>
    </Dialog>
  );
}
