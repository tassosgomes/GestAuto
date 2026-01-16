import { useMemo, useState } from 'react'
import { ArrowUpDown } from 'lucide-react'
import { format } from 'date-fns'
import { ptBR } from 'date-fns/locale'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import type {
  CheckInSource,
  CheckOutReason,
  ReservationStatus,
  ReservationType,
  TestDriveOutcome,
  VehicleHistoryItemResponse,
  VehicleStatus,
} from '../types'
import {
  mapCheckInSourceLabel,
  mapCheckOutReasonLabel,
  mapReservationStatusLabel,
  mapReservationTypeLabel,
  mapTestDriveOutcomeLabel,
  mapVehicleStatusLabel,
} from '../types'

type SortOrder = 'asc' | 'desc'

const HISTORY_TYPE_LABELS: Record<string, string> = {
  checkin: 'Check-in',
  checkout: 'Check-out',
  reservationcreated: 'Reserva criada',
  reservationcancelled: 'Reserva cancelada',
  reservationextended: 'Reserva prorrogada',
  reservationexpired: 'Reserva expirada',
  testdrivestarted: 'Test-drive iniciado',
  testdrivecompleted: 'Test-drive finalizado',
  statuschanged: 'Status alterado',
  statuschange: 'Status alterado',
}

const normalizeType = (value: string) => value.replace(/[\s_-]/g, '').toLowerCase()

const getHistoryTypeLabel = (type: string) => HISTORY_TYPE_LABELS[normalizeType(type)] ?? type

const formatHistoryDate = (value: string) => {
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) {
    return value
  }

  return format(date, "d 'de' MMMM 'às' HH:mm", { locale: ptBR })
}

const getStringValue = (details: Record<string, unknown>, key: string) =>
  typeof details[key] === 'string' ? (details[key] as string) : undefined

const getNumberValue = (details: Record<string, unknown>, key: string) =>
  typeof details[key] === 'number' ? (details[key] as number) : undefined

const formatHistorySummary = (item: VehicleHistoryItemResponse) => {
  const details = item.details ?? {}
  const summary =
    getStringValue(details, 'summary') ||
    getStringValue(details, 'message') ||
    getStringValue(details, 'description')

  if (summary) {
    return summary
  }

  const newStatus = getNumberValue(details, 'newStatus') ?? getNumberValue(details, 'status')
  if (newStatus !== undefined) {
    return `Status: ${mapVehicleStatusLabel(newStatus as VehicleStatus)}`
  }

  const reservationType = getNumberValue(details, 'reservationType')
  if (reservationType !== undefined) {
    return `Reserva: ${mapReservationTypeLabel(reservationType as ReservationType)}`
  }

  const reservationStatus = getNumberValue(details, 'reservationStatus')
  if (reservationStatus !== undefined) {
    return `Reserva: ${mapReservationStatusLabel(reservationStatus as ReservationStatus)}`
  }

  const checkInSource = getNumberValue(details, 'source') ?? getNumberValue(details, 'checkInSource')
  if (checkInSource !== undefined) {
    return `Entrada: ${mapCheckInSourceLabel(checkInSource as CheckInSource)}`
  }

  const checkOutReason = getNumberValue(details, 'reason') ?? getNumberValue(details, 'checkOutReason')
  if (checkOutReason !== undefined) {
    return `Saída: ${mapCheckOutReasonLabel(checkOutReason as CheckOutReason)}`
  }

  const testDriveOutcome = getNumberValue(details, 'outcome')
  if (testDriveOutcome !== undefined) {
    return `Resultado: ${mapTestDriveOutcomeLabel(testDriveOutcome as TestDriveOutcome)}`
  }

  const notes = getStringValue(details, 'notes')
  if (notes) {
    return `Observação: ${notes}`
  }

  return 'Sem detalhes adicionais.'
}

interface VehicleHistoryTimelineProps {
  items: VehicleHistoryItemResponse[]
  isLoading?: boolean
  error?: unknown
}

export function VehicleHistoryTimeline({ items, isLoading, error }: VehicleHistoryTimelineProps) {
  const [sortOrder, setSortOrder] = useState<SortOrder>('desc')

  const sortedItems = useMemo(() => {
    const sorted = [...items].sort((a, b) => {
      const timeA = Date.parse(a.occurredAtUtc)
      const timeB = Date.parse(b.occurredAtUtc)
      return timeA - timeB
    })

    if (sortOrder === 'desc') {
      return sorted.reverse()
    }

    return sorted
  }, [items, sortOrder])

  return (
    <Card>
      <CardHeader className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <CardTitle>Histórico</CardTitle>
          <CardDescription>{items.length} evento(s) registrados</CardDescription>
        </div>
        <Button
          type="button"
          variant="ghost"
          size="sm"
          onClick={() => setSortOrder((prev) => (prev === 'asc' ? 'desc' : 'asc'))}
        >
          <ArrowUpDown className="h-4 w-4" />
          {sortOrder === 'asc' ? 'Mais antigos' : 'Mais recentes'}
        </Button>
      </CardHeader>
      <CardContent>
        {isLoading ? (
          <div className="space-y-4">
            <Skeleton className="h-4 w-1/2" />
            <Skeleton className="h-4 w-2/3" />
            <Skeleton className="h-4 w-1/3" />
          </div>
        ) : error ? (
          <p className="text-sm text-red-500">
            {error instanceof Error ? error.message : 'Falha ao carregar histórico do veículo'}
          </p>
        ) : sortedItems.length === 0 ? (
          <p className="text-muted-foreground">Nenhum evento registrado até o momento.</p>
        ) : (
          <div className="space-y-6">
            {sortedItems.map((item, index) => (
              <div key={`${item.type}-${item.occurredAtUtc}-${index}`} className="flex gap-4">
                <div className="flex flex-col items-center">
                  <span className="mt-1 h-2.5 w-2.5 rounded-full bg-primary" />
                  {index < sortedItems.length - 1 && (
                    <span className="mt-2 h-full w-px flex-1 bg-border" />
                  )}
                </div>
                <div className="space-y-1">
                  <div className="flex flex-wrap items-center gap-2">
                    <Badge variant="secondary">{getHistoryTypeLabel(item.type)}</Badge>
                    <span className="text-xs text-muted-foreground">
                      {formatHistoryDate(item.occurredAtUtc)}
                    </span>
                  </div>
                  <p className="text-sm text-muted-foreground">{formatHistorySummary(item)}</p>
                  {item.userId && (
                    <p className="text-xs text-muted-foreground">Usuário: {item.userId}</p>
                  )}
                </div>
              </div>
            ))}
          </div>
        )}
      </CardContent>
    </Card>
  )
}
