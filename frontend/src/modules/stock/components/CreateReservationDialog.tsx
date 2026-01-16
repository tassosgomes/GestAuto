import { useEffect, useMemo } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import * as z from 'zod'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from '@/components/ui/form'
import { Input } from '@/components/ui/input'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Button } from '@/components/ui/button'
import { useToast } from '@/hooks/use-toast'
import { useCreateReservation } from '../hooks/useReservations'
import { ReservationType, toBankDeadlineAtUtc } from '../types'

const reservationTypeOptions = [
  { value: String(ReservationType.Standard), label: 'Padrão' },
  { value: String(ReservationType.PaidDeposit), label: 'Entrada paga' },
  { value: String(ReservationType.WaitingBank), label: 'Aguardando banco' },
]

const buildSchema = (hasVehicleId: boolean) =>
  z
    .object({
      vehicleId: z.string().optional(),
      type: z.string().min(1, 'Selecione o tipo'),
      contextType: z.string().min(1, 'Informe o contexto'),
      contextId: z.string().optional(),
      bankDeadlineDate: z.string().optional(),
    })
    .superRefine((data, ctx) => {
      if (!hasVehicleId && (!data.vehicleId || data.vehicleId.trim().length === 0)) {
        ctx.addIssue({
          code: z.ZodIssueCode.custom,
          message: 'Informe o veículo',
          path: ['vehicleId'],
        })
      }
    })

type CreateReservationFormValues = z.infer<ReturnType<typeof buildSchema>>

interface CreateReservationDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  vehicleId?: string
}

export function CreateReservationDialog({ open, onOpenChange, vehicleId }: CreateReservationDialogProps) {
  const { toast } = useToast()
  const createReservation = useCreateReservation()
  const schema = useMemo(() => buildSchema(Boolean(vehicleId)), [vehicleId])

  const form = useForm<CreateReservationFormValues>({
    resolver: zodResolver(schema),
    defaultValues: {
      vehicleId: '',
      type: '',
      contextType: '',
      contextId: '',
      bankDeadlineDate: '',
    },
  })

  useEffect(() => {
    if (!open) {
      form.reset()
    }
  }, [form, open])

  const onSubmit = (data: CreateReservationFormValues) => {
    const targetVehicleId = vehicleId ?? data.vehicleId?.trim()
    if (!targetVehicleId) {
      return
    }

    const bankDeadlineAtUtc = data.bankDeadlineDate
      ? toBankDeadlineAtUtc(data.bankDeadlineDate)
      : undefined

    createReservation.mutate(
      {
        vehicleId: targetVehicleId,
        data: {
          type: Number(data.type) as ReservationType,
          contextType: data.contextType,
          contextId: data.contextId || undefined,
          bankDeadlineAtUtc: bankDeadlineAtUtc || undefined,
        },
      },
      {
        onSuccess: () => {
          toast({
            title: 'Reserva criada',
            description: 'A reserva foi registrada com sucesso.',
          })
          onOpenChange(false)
        },
        onError: () => {
          toast({
            title: 'Erro ao criar reserva',
            description: 'Não foi possível registrar a reserva.',
            variant: 'destructive',
          })
        },
      }
    )
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[460px]">
        <DialogHeader>
          <DialogTitle>Criar reserva</DialogTitle>
          <DialogDescription>Informe o tipo e o contexto para registrar a reserva.</DialogDescription>
        </DialogHeader>
        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
            {!vehicleId && (
              <FormField
                control={form.control}
                name="vehicleId"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Veículo</FormLabel>
                    <FormControl>
                      <Input placeholder="ID do veículo" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
            )}
            <FormField
              control={form.control}
              name="type"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Tipo</FormLabel>
                  <Select onValueChange={field.onChange} value={field.value}>
                    <FormControl>
                      <SelectTrigger>
                        <SelectValue placeholder="Selecione" />
                      </SelectTrigger>
                    </FormControl>
                    <SelectContent>
                      {reservationTypeOptions.map((option) => (
                        <SelectItem key={option.value} value={option.value}>
                          {option.label}
                        </SelectItem>
                      ))}
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
                  <FormLabel>Contexto</FormLabel>
                  <FormControl>
                    <Input placeholder="Ex.: Lead, Proposta" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
            <FormField
              control={form.control}
              name="contextId"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>ID do contexto (opcional)</FormLabel>
                  <FormControl>
                    <Input placeholder="Identificador" {...field} />
                  </FormControl>
                </FormItem>
              )}
            />
            <FormField
              control={form.control}
              name="bankDeadlineDate"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Prazo do banco (opcional)</FormLabel>
                  <FormControl>
                    <Input type="date" {...field} />
                  </FormControl>
                </FormItem>
              )}
            />
            <DialogFooter>
              <Button type="submit" disabled={createReservation.isPending}>
                {createReservation.isPending ? 'Salvando...' : 'Criar reserva'}
              </Button>
            </DialogFooter>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  )
}
