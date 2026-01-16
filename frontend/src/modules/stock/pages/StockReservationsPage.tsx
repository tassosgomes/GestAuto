import { useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import * as z from 'zod'
import { format } from 'date-fns'
import { ptBR } from 'date-fns/locale'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
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
import { Skeleton } from '@/components/ui/skeleton'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import { Textarea } from '@/components/ui/textarea'
import { useAuth } from '@/auth/useAuth'
import { useToast } from '@/hooks/use-toast'
import { CreateReservationDialog } from '../components/CreateReservationDialog'
import {
  useCancelReservation,
  useExtendReservation,
  useReservationsList,
} from '../hooks/useReservations'
import {
  mapReservationStatusLabel,
  mapReservationTypeLabel,
  ReservationStatus,
  ReservationType,
  toBankDeadlineAtUtc,
} from '../types'
import type { ReservationListItem } from '../types'

const cancelSchema = z.object({
  reason: z.string().min(3, 'Informe o motivo do cancelamento'),
})

type CancelFormValues = z.infer<typeof cancelSchema>

const extendSchema = z.object({
  newExpiresDate: z.string().min(1, 'Informe a nova data'),
})

type ExtendFormValues = z.infer<typeof extendSchema>

const reservationStatusOptions = [
  { value: '', label: 'Todos os status' },
  { value: String(ReservationStatus.Active), label: mapReservationStatusLabel(ReservationStatus.Active) },
  { value: String(ReservationStatus.Cancelled), label: mapReservationStatusLabel(ReservationStatus.Cancelled) },
  { value: String(ReservationStatus.Completed), label: mapReservationStatusLabel(ReservationStatus.Completed) },
  { value: String(ReservationStatus.Expired), label: mapReservationStatusLabel(ReservationStatus.Expired) },
]

const reservationTypeOptions = [
  { value: '', label: 'Todos os tipos' },
  { value: String(ReservationType.Standard), label: mapReservationTypeLabel(ReservationType.Standard) },
  { value: String(ReservationType.PaidDeposit), label: mapReservationTypeLabel(ReservationType.PaidDeposit) },
  { value: String(ReservationType.WaitingBank), label: mapReservationTypeLabel(ReservationType.WaitingBank) },
]

const formatDateTime = (value?: string | null) => {
  if (!value) return '—'
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return '—'
  return format(date, "dd/MM/yyyy 'às' HH:mm", { locale: ptBR })
}

const formatDateOnly = (value?: string | null) => {
  if (!value) return '—'
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return '—'
  return format(date, 'dd/MM/yyyy', { locale: ptBR })
}

export function StockReservationsPage() {
  const { toast } = useToast()
  const authState = useAuth()
  const [page] = useState(1)
  const [search, setSearch] = useState('')
  const [statusFilter, setStatusFilter] = useState('')
  const [typeFilter, setTypeFilter] = useState('')
  const [createDialogOpen, setCreateDialogOpen] = useState(false)
  const [cancelDialogOpen, setCancelDialogOpen] = useState(false)
  const [extendDialogOpen, setExtendDialogOpen] = useState(false)
  const [selectedReservation, setSelectedReservation] = useState<ReservationListItem | null>(null)

  const reservationsQuery = useReservationsList({
    page,
    size: 10,
    status: statusFilter || undefined,
    type: typeFilter || undefined,
    q: search || undefined,
  })

  const cancelReservation = useCancelReservation()
  const extendReservation = useExtendReservation()

  const cancelForm = useForm<CancelFormValues>({
    resolver: zodResolver(cancelSchema),
    defaultValues: { reason: '' },
  })

  const extendForm = useForm<ExtendFormValues>({
    resolver: zodResolver(extendSchema),
    defaultValues: { newExpiresDate: '' },
  })

  const roles = useMemo(() => {
    if (authState.status !== 'ready' || !authState.session.isAuthenticated) {
      return []
    }
    return authState.session.roles
  }, [authState])

  const canCreateReservation =
    roles.includes('ADMIN') ||
    roles.includes('MANAGER') ||
    roles.includes('SALES_MANAGER') ||
    roles.includes('SALES_PERSON')

  const isManager = roles.includes('ADMIN') || roles.includes('MANAGER') || roles.includes('SALES_MANAGER')
  const isSales = roles.includes('SALES_PERSON') || roles.includes('SALES_MANAGER')

  const reservations = reservationsQuery.data?.data ?? []

  const getVehicleLabel = (reservation: ReservationListItem) => {
    const vehicle = reservation.vehicle
    if (vehicle) {
      const title = [vehicle.make, vehicle.model, vehicle.trim].filter(Boolean).join(' ')
      return title || vehicle.plate || vehicle.vin || reservation.vehicleId
    }

    return reservation.vehicleId
  }

  const canCancelReservation = (reservation: ReservationListItem) => {
    if (isManager) return true
    if (!isSales) return false
    if (!reservation.salesPersonId) return false
    const username = authState.status === 'ready' ? authState.session.username : undefined
    return Boolean(username && username === reservation.salesPersonId)
  }

  const canExtendReservation = () => isManager

  const handleOpenCancel = (reservation: ReservationListItem) => {
    setSelectedReservation(reservation)
    setCancelDialogOpen(true)
    cancelForm.reset({ reason: '' })
  }

  const handleOpenExtend = (reservation: ReservationListItem) => {
    setSelectedReservation(reservation)
    setExtendDialogOpen(true)
    extendForm.reset({ newExpiresDate: '' })
  }

  const handleCancelSubmit = (data: CancelFormValues) => {
    if (!selectedReservation) return
    cancelReservation.mutate(
      { reservationId: selectedReservation.id, data: { reason: data.reason } },
      {
        onSuccess: () => {
          toast({
            title: 'Reserva cancelada',
            description: 'A reserva foi cancelada com sucesso.',
          })
          setCancelDialogOpen(false)
          setSelectedReservation(null)
        },
        onError: () => {
          toast({
            title: 'Erro ao cancelar reserva',
            description: 'Não foi possível cancelar a reserva.',
            variant: 'destructive',
          })
        },
      }
    )
  }

  const handleExtendSubmit = (data: ExtendFormValues) => {
    if (!selectedReservation) return
    const newExpiresAtUtc = toBankDeadlineAtUtc(data.newExpiresDate)

    extendReservation.mutate(
      { reservationId: selectedReservation.id, data: { newExpiresAtUtc } },
      {
        onSuccess: () => {
          toast({
            title: 'Reserva prorrogada',
            description: 'A reserva foi prorrogada com sucesso.',
          })
          setExtendDialogOpen(false)
          setSelectedReservation(null)
        },
        onError: () => {
          toast({
            title: 'Erro ao prorrogar reserva',
            description: 'Não foi possível prorrogar a reserva.',
            variant: 'destructive',
          })
        },
      }
    )
  }

  return (
    <div className="p-6 space-y-6">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-2xl font-semibold">Reservas</h1>
          <p className="text-muted-foreground">Gestão de reservas ativas e recentes.</p>
        </div>
        {canCreateReservation && (
          <Button onClick={() => setCreateDialogOpen(true)}>Nova reserva</Button>
        )}
      </div>

      <div className="grid gap-4 md:grid-cols-3">
        <Input
          placeholder="Buscar por veículo ou vendedor"
          value={search}
          onChange={(event) => setSearch(event.target.value)}
        />
        <Select value={statusFilter} onValueChange={setStatusFilter}>
          <SelectTrigger>
            <SelectValue placeholder="Filtrar por status" />
          </SelectTrigger>
          <SelectContent>
            {reservationStatusOptions.map((option) => (
              <SelectItem key={option.value || 'all'} value={option.value}>
                {option.label}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
        <Select value={typeFilter} onValueChange={setTypeFilter}>
          <SelectTrigger>
            <SelectValue placeholder="Filtrar por tipo" />
          </SelectTrigger>
          <SelectContent>
            {reservationTypeOptions.map((option) => (
              <SelectItem key={option.value || 'all'} value={option.value}>
                {option.label}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      {reservationsQuery.isLoading ? (
        <div className="space-y-3">
          <Skeleton className="h-6 w-1/3" />
          <Skeleton className="h-32 w-full" />
        </div>
      ) : reservationsQuery.isError ? (
        <p className="text-sm text-red-500">
          {reservationsQuery.error instanceof Error
            ? reservationsQuery.error.message
            : 'Falha ao carregar reservas'}
        </p>
      ) : reservations.length === 0 ? (
        <div className="rounded-lg border border-dashed p-8 text-center text-muted-foreground">
          Nenhuma reserva encontrada.
        </div>
      ) : (
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Veículo</TableHead>
              <TableHead>Tipo</TableHead>
              <TableHead>Status</TableHead>
              <TableHead>Vendedor</TableHead>
              <TableHead>Criada em</TableHead>
              <TableHead>Expira em</TableHead>
              <TableHead>Prazo banco</TableHead>
              <TableHead className="text-right">Ações</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {reservations.map((reservation) => (
              <TableRow key={reservation.id}>
                <TableCell className="font-medium">
                  <Link
                    to={`/stock/vehicles/${reservation.vehicleId}`}
                    className="text-primary hover:underline"
                  >
                    {getVehicleLabel(reservation)}
                  </Link>
                </TableCell>
                <TableCell>{mapReservationTypeLabel(reservation.type)}</TableCell>
                <TableCell>
                  <Badge variant="secondary">{mapReservationStatusLabel(reservation.status)}</Badge>
                </TableCell>
                <TableCell>{reservation.salesPersonId ?? '—'}</TableCell>
                <TableCell>{formatDateTime(reservation.createdAtUtc)}</TableCell>
                <TableCell>{formatDateTime(reservation.expiresAtUtc)}</TableCell>
                <TableCell>{formatDateOnly(reservation.bankDeadlineAtUtc)}</TableCell>
                <TableCell className="text-right">
                  <div className="flex flex-wrap justify-end gap-2">
                    {canCancelReservation(reservation) && (
                      <Button
                        variant="outline"
                        size="sm"
                        onClick={() => handleOpenCancel(reservation)}
                      >
                        Cancelar
                      </Button>
                    )}
                    {canExtendReservation() && (
                      <Button
                        variant="outline"
                        size="sm"
                        onClick={() => handleOpenExtend(reservation)}
                      >
                        Prorrogar
                      </Button>
                    )}
                  </div>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      )}

      <CreateReservationDialog open={createDialogOpen} onOpenChange={setCreateDialogOpen} />

      <Dialog open={cancelDialogOpen} onOpenChange={setCancelDialogOpen}>
        <DialogContent className="sm:max-w-[420px]">
          <DialogHeader>
            <DialogTitle>Cancelar reserva</DialogTitle>
            <DialogDescription>Informe o motivo para cancelar a reserva.</DialogDescription>
          </DialogHeader>
          <Form {...cancelForm}>
            <form onSubmit={cancelForm.handleSubmit(handleCancelSubmit)} className="space-y-4">
              <FormField
                control={cancelForm.control}
                name="reason"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Motivo</FormLabel>
                    <FormControl>
                      <Textarea placeholder="Descreva o motivo" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <DialogFooter>
                <Button type="submit" disabled={cancelReservation.isPending}>
                  {cancelReservation.isPending ? 'Cancelando...' : 'Confirmar cancelamento'}
                </Button>
              </DialogFooter>
            </form>
          </Form>
        </DialogContent>
      </Dialog>

      <Dialog open={extendDialogOpen} onOpenChange={setExtendDialogOpen}>
        <DialogContent className="sm:max-w-[420px]">
          <DialogHeader>
            <DialogTitle>Prorrogar reserva</DialogTitle>
            <DialogDescription>Selecione a nova data de expiração.</DialogDescription>
          </DialogHeader>
          <Form {...extendForm}>
            <form onSubmit={extendForm.handleSubmit(handleExtendSubmit)} className="space-y-4">
              <FormField
                control={extendForm.control}
                name="newExpiresDate"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Nova data</FormLabel>
                    <FormControl>
                      <Input type="date" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <DialogFooter>
                <Button type="submit" disabled={extendReservation.isPending}>
                  {extendReservation.isPending ? 'Salvando...' : 'Confirmar prorrogação'}
                </Button>
              </DialogFooter>
            </form>
          </Form>
        </DialogContent>
      </Dialog>
    </div>
  )
}
