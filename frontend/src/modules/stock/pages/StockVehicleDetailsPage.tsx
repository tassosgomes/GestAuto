import { useMemo, useState } from 'react'
import { useParams } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import * as z from 'zod'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
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
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { Textarea } from '@/components/ui/textarea'
import { useToast } from '@/hooks/use-toast'
import { useAuth } from '@/auth/useAuth'
import { formatCurrency } from '@/lib/utils'
import { differenceInDays } from 'date-fns'
import { VehicleHistoryTimeline } from '../components/VehicleHistoryTimeline'
import { useCreateReservation } from '../hooks/useReservations'
import { useChangeVehicleStatus, useStartTestDrive, useVehicle, useVehicleHistory } from '../hooks/useVehicles'
import {
  mapDemoPurposeLabel,
  mapVehicleCategoryLabel,
  mapVehicleStatusLabel,
  ReservationType,
  toBankDeadlineAtUtc,
  VehicleStatus,
} from '../types'

const changeStatusSchema = z.object({
  newStatus: z.string().min(1, 'Selecione o status'),
  reason: z.string().min(3, 'Informe o motivo'),
})

type ChangeStatusFormValues = z.infer<typeof changeStatusSchema>

const createReservationSchema = z.object({
  type: z.string().min(1, 'Selecione o tipo'),
  contextType: z.string().min(1, 'Informe o contexto'),
  contextId: z.string().optional(),
  bankDeadlineDate: z.string().optional(),
})

type CreateReservationFormValues = z.infer<typeof createReservationSchema>

const startTestDriveSchema = z.object({
  customerRef: z.string().optional(),
})

type StartTestDriveFormValues = z.infer<typeof startTestDriveSchema>

const vehicleStatusOptions = [
  { value: String(VehicleStatus.InTransit), label: mapVehicleStatusLabel(VehicleStatus.InTransit) },
  { value: String(VehicleStatus.InStock), label: mapVehicleStatusLabel(VehicleStatus.InStock) },
  { value: String(VehicleStatus.Reserved), label: mapVehicleStatusLabel(VehicleStatus.Reserved) },
  { value: String(VehicleStatus.InTestDrive), label: mapVehicleStatusLabel(VehicleStatus.InTestDrive) },
  { value: String(VehicleStatus.InPreparation), label: mapVehicleStatusLabel(VehicleStatus.InPreparation) },
  { value: String(VehicleStatus.Sold), label: mapVehicleStatusLabel(VehicleStatus.Sold) },
  { value: String(VehicleStatus.WrittenOff), label: mapVehicleStatusLabel(VehicleStatus.WrittenOff) },
]

const reservationTypeOptions = [
  { value: String(ReservationType.Standard), label: 'Padrão' },
  { value: String(ReservationType.PaidDeposit), label: 'Entrada paga' },
  { value: String(ReservationType.WaitingBank), label: 'Aguardando banco' },
]

const getStatusVariant = (status?: VehicleStatus | null) => {
  switch (status) {
    case VehicleStatus.InStock:
      return 'default'
    case VehicleStatus.Reserved:
      return 'secondary'
    case VehicleStatus.InTestDrive:
    case VehicleStatus.InPreparation:
      return 'outline'
    case VehicleStatus.WrittenOff:
      return 'destructive'
    default:
      return 'secondary'
  }
}

export function StockVehicleDetailsPage() {
  const { id } = useParams()
  const vehicleQuery = useVehicle(id)
  const historyQuery = useVehicleHistory(id)
  const changeStatus = useChangeVehicleStatus()
  const createReservation = useCreateReservation()
  const startTestDrive = useStartTestDrive()
  const { toast } = useToast()
  const authState = useAuth()
  const [statusDialogOpen, setStatusDialogOpen] = useState(false)
  const [reservationDialogOpen, setReservationDialogOpen] = useState(false)
  const [testDriveDialogOpen, setTestDriveDialogOpen] = useState(false)

  const changeStatusForm = useForm<ChangeStatusFormValues>({
    resolver: zodResolver(changeStatusSchema),
    defaultValues: {
      newStatus: '',
      reason: '',
    },
  })

  const reservationForm = useForm<CreateReservationFormValues>({
    resolver: zodResolver(createReservationSchema),
    defaultValues: {
      type: '',
      contextType: '',
      contextId: '',
      bankDeadlineDate: '',
    },
  })

  const testDriveForm = useForm<StartTestDriveFormValues>({
    resolver: zodResolver(startTestDriveSchema),
    defaultValues: {
      customerRef: '',
    },
  })

  const roles = useMemo(() => {
    if (authState.status !== 'ready' || !authState.session.isAuthenticated) {
      return []
    }

    return authState.session.roles
  }, [authState])

  const canChangeStatus =
    roles.includes('ADMIN') || roles.includes('MANAGER') || roles.includes('STOCK_MANAGER')

  const canReserve =
    roles.includes('ADMIN') ||
    roles.includes('MANAGER') ||
    roles.includes('SALES_MANAGER') ||
    roles.includes('SALES_PERSON')

  const canStartTestDrive =
    roles.includes('ADMIN') ||
    roles.includes('MANAGER') ||
    roles.includes('SALES_MANAGER') ||
    roles.includes('SALES_PERSON')

  const vehicle = vehicleQuery.data
  const historyItems = historyQuery.data?.items ?? []

  const daysInStock = useMemo(() => {
    if (!vehicle?.createdAt) {
      return null
    }

    const createdAt = new Date(vehicle.createdAt)
    if (Number.isNaN(createdAt.getTime())) {
      return null
    }

    return differenceInDays(new Date(), createdAt)
  }, [vehicle?.createdAt])

  const technicalFields = useMemo(() => {
    if (!vehicle) {
      return []
    }

    return [
      { label: 'Categoria', value: mapVehicleCategoryLabel(vehicle.category) },
      { label: 'Ano/modelo', value: vehicle.yearModel },
      { label: 'Cor', value: vehicle.color },
      { label: 'VIN', value: vehicle.vin },
      { label: 'Placa', value: vehicle.plate, hideWhenEmpty: true },
      {
        label: 'KM',
        value:
          vehicle.mileageKm != null
            ? `${vehicle.mileageKm.toLocaleString('pt-BR')} km`
            : null,
        hideWhenEmpty: true,
      },
      {
        label: 'Preço',
        value: vehicle.price != null ? formatCurrency(vehicle.price) : null,
        hideWhenEmpty: true,
      },
      { label: 'Localização', value: vehicle.location, hideWhenEmpty: true },
      {
        label: 'Finalidade',
        value: vehicle.demoPurpose != null ? mapDemoPurposeLabel(vehicle.demoPurpose) : null,
        hideWhenEmpty: true,
      },
      {
        label: 'Dias em estoque',
        value: daysInStock != null ? `${daysInStock} dia(s)` : '—',
      },
    ]
  }, [vehicle, daysInStock])

  const renderField = (
    label: string,
    value: React.ReactNode,
    options?: { hideWhenEmpty?: boolean }
  ) => {
    const isEmpty = value === null || value === undefined || value === ''
    if (options?.hideWhenEmpty && isEmpty) {
      return null
    }

    return (
      <div key={label} className="flex flex-col gap-1">
        <span className="text-xs text-muted-foreground">{label}</span>
        <span className="text-sm font-medium">{isEmpty ? '—' : value}</span>
      </div>
    )
  }

  const handleChangeStatus = (data: ChangeStatusFormValues) => {
    if (!id) {
      return
    }

    changeStatus.mutate(
      {
        id,
        data: {
          newStatus: Number(data.newStatus) as VehicleStatus,
          reason: data.reason,
        },
      },
      {
        onSuccess: () => {
          toast({
            title: 'Status atualizado',
            description: 'O status do veículo foi atualizado com sucesso.',
          })
          setStatusDialogOpen(false)
          changeStatusForm.reset()
        },
        onError: () => {
          toast({
            title: 'Erro ao atualizar status',
            description: 'Não foi possível alterar o status do veículo.',
            variant: 'destructive',
          })
        },
      }
    )
  }

  const handleCreateReservation = (data: CreateReservationFormValues) => {
    if (!id) {
      return
    }

    const bankDeadlineAtUtc = data.bankDeadlineDate
      ? toBankDeadlineAtUtc(data.bankDeadlineDate)
      : undefined

    createReservation.mutate(
      {
        vehicleId: id,
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
          setReservationDialogOpen(false)
          reservationForm.reset()
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

  const handleStartTestDrive = (data: StartTestDriveFormValues) => {
    if (!id) {
      return
    }

    startTestDrive.mutate(
      {
        id,
        data: {
          customerRef: data.customerRef || undefined,
        },
      },
      {
        onSuccess: () => {
          toast({
            title: 'Test-drive iniciado',
            description: 'O test-drive foi iniciado com sucesso.',
          })
          setTestDriveDialogOpen(false)
          testDriveForm.reset()
        },
        onError: () => {
          toast({
            title: 'Erro ao iniciar test-drive',
            description: 'Não foi possível iniciar o test-drive.',
            variant: 'destructive',
          })
        },
      }
    )
  }

  if (!id) {
    return (
      <div className="p-6">
        <h1 className="text-2xl font-semibold">Detalhe do veículo</h1>
        <p className="text-muted-foreground">Veículo não encontrado.</p>
      </div>
    )
  }

  return (
    <div className="p-6 space-y-6">
      {vehicleQuery.isLoading ? (
        <div className="space-y-4">
          <Skeleton className="h-8 w-1/3" />
          <Skeleton className="h-4 w-1/4" />
          <Skeleton className="h-32 w-full" />
        </div>
      ) : vehicleQuery.error ? (
        <div className="space-y-2">
          <h1 className="text-2xl font-semibold">Detalhe do veículo</h1>
          <p className="text-sm text-red-500">
            {vehicleQuery.error instanceof Error
              ? vehicleQuery.error.message
              : 'Falha ao carregar veículo'}
          </p>
        </div>
      ) : !vehicle ? (
        <div className="space-y-2">
          <h1 className="text-2xl font-semibold">Detalhe do veículo</h1>
          <p className="text-muted-foreground">Veículo não encontrado.</p>
        </div>
      ) : (
        <>
          <div className="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
            <div className="flex flex-col gap-3 sm:flex-row sm:items-center">
              <div className="h-24 w-36 rounded-lg bg-muted overflow-hidden flex items-center justify-center">
                {vehicle.imageUrl ? (
                  <img
                    src={vehicle.imageUrl}
                    alt={`${vehicle.make} ${vehicle.model}`}
                    className="h-full w-full object-cover"
                  />
                ) : (
                  <span className="text-xs text-muted-foreground">Sem imagem</span>
                )}
              </div>
              <div className="space-y-2">
                <div className="flex flex-wrap items-center gap-2">
                  <h1 className="text-3xl font-semibold">
                    {[vehicle.make, vehicle.model, vehicle.trim].filter(Boolean).join(' ')}
                  </h1>
                  <Badge variant={getStatusVariant(vehicle.currentStatus)}>
                    {mapVehicleStatusLabel(vehicle.currentStatus)}
                  </Badge>
                  <Badge variant="outline">{mapVehicleCategoryLabel(vehicle.category)}</Badge>
                </div>
                <div className="text-sm text-muted-foreground flex flex-wrap gap-4">
                  <span>VIN: {vehicle.vin}</span>
                  {vehicle.plate && <span>Placa: {vehicle.plate}</span>}
                </div>
              </div>
            </div>
            {(canChangeStatus || canReserve || canStartTestDrive) && (
              <div className="flex flex-wrap gap-2">
                {canChangeStatus && (
                  <Button variant="outline" onClick={() => setStatusDialogOpen(true)}>
                    Alterar status
                  </Button>
                )}
                {canReserve && (
                  <Button variant="outline" onClick={() => setReservationDialogOpen(true)}>
                    Criar reserva
                  </Button>
                )}
                {canStartTestDrive && (
                  <Button onClick={() => setTestDriveDialogOpen(true)}>Iniciar test-drive</Button>
                )}
              </div>
            )}
          </div>

          <div className="grid gap-6 lg:grid-cols-[1.7fr_1fr]">
            <div className="space-y-6">
              <Tabs defaultValue="history" className="w-full">
                <TabsList>
                  <TabsTrigger value="history">Histórico</TabsTrigger>
                </TabsList>
                <TabsContent value="history">
                  <VehicleHistoryTimeline
                    items={historyItems}
                    isLoading={historyQuery.isLoading}
                    error={historyQuery.error}
                  />
                </TabsContent>
              </Tabs>
            </div>
            <Card>
              <CardHeader>
                <CardTitle>Ficha técnica</CardTitle>
              </CardHeader>
              <CardContent className="grid gap-4 sm:grid-cols-2">
                {technicalFields.map((field) =>
                  renderField(field.label, field.value, { hideWhenEmpty: field.hideWhenEmpty })
                )}
              </CardContent>
            </Card>
          </div>
        </>
      )}

      <Dialog
        open={statusDialogOpen}
        onOpenChange={(open) => {
          setStatusDialogOpen(open)
          if (!open) changeStatusForm.reset()
        }}
      >
        <DialogContent className="sm:max-w-[420px]">
          <DialogHeader>
            <DialogTitle>Alterar status</DialogTitle>
            <DialogDescription>Atualize o status do veículo e registre o motivo.</DialogDescription>
          </DialogHeader>
          <Form {...changeStatusForm}>
            <form onSubmit={changeStatusForm.handleSubmit(handleChangeStatus)} className="space-y-4">
              <FormField
                control={changeStatusForm.control}
                name="newStatus"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Novo status</FormLabel>
                    <Select onValueChange={field.onChange} value={field.value}>
                      <FormControl>
                        <SelectTrigger>
                          <SelectValue placeholder="Selecione" />
                        </SelectTrigger>
                      </FormControl>
                      <SelectContent>
                        {vehicleStatusOptions.map((option) => (
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
                control={changeStatusForm.control}
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
                <Button type="submit" disabled={changeStatus.isPending}>
                  {changeStatus.isPending ? 'Salvando...' : 'Atualizar status'}
                </Button>
              </DialogFooter>
            </form>
          </Form>
        </DialogContent>
      </Dialog>

      <Dialog
        open={reservationDialogOpen}
        onOpenChange={(open) => {
          setReservationDialogOpen(open)
          if (!open) reservationForm.reset()
        }}
      >
        <DialogContent className="sm:max-w-[460px]">
          <DialogHeader>
            <DialogTitle>Criar reserva</DialogTitle>
            <DialogDescription>Informe o tipo e o contexto para registrar a reserva.</DialogDescription>
          </DialogHeader>
          <Form {...reservationForm}>
            <form onSubmit={reservationForm.handleSubmit(handleCreateReservation)} className="space-y-4">
              <FormField
                control={reservationForm.control}
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
                control={reservationForm.control}
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
                control={reservationForm.control}
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
                control={reservationForm.control}
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

      <Dialog
        open={testDriveDialogOpen}
        onOpenChange={(open) => {
          setTestDriveDialogOpen(open)
          if (!open) testDriveForm.reset()
        }}
      >
        <DialogContent className="sm:max-w-[420px]">
          <DialogHeader>
            <DialogTitle>Iniciar test-drive</DialogTitle>
            <DialogDescription>Opcionalmente informe o identificador do cliente.</DialogDescription>
          </DialogHeader>
          <Form {...testDriveForm}>
            <form onSubmit={testDriveForm.handleSubmit(handleStartTestDrive)} className="space-y-4">
              <FormField
                control={testDriveForm.control}
                name="customerRef"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Cliente (opcional)</FormLabel>
                    <FormControl>
                      <Input placeholder="CPF, nome ou ID" {...field} />
                    </FormControl>
                  </FormItem>
                )}
              />
              <DialogFooter>
                <Button type="submit" disabled={startTestDrive.isPending}>
                  {startTestDrive.isPending ? 'Iniciando...' : 'Iniciar test-drive'}
                </Button>
              </DialogFooter>
            </form>
          </Form>
        </DialogContent>
      </Dialog>
    </div>
  )
}
