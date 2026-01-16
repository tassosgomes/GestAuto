import { useState } from 'react'
import { useVehiclesList, useChangeVehicleStatus } from '../hooks/useVehicles'
import { VehicleStatus, mapVehicleStatusLabel, mapVehicleCategoryLabel } from '../types'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { Alert, AlertDescription } from '@/components/ui/alert'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import { Label } from '@/components/ui/label'
import { Textarea } from '@/components/ui/textarea'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'

export function StockPreparationPage() {
  const [page, setPage] = useState(1)
  const [selectedVehicle, setSelectedVehicle] = useState<{ id: string; vin: string } | null>(null)
  const [newStatus, setNewStatus] = useState<VehicleStatus>(VehicleStatus.InStock)
  const [reason, setReason] = useState('')

  const { data, isLoading, isError, error } = useVehiclesList({
    page,
    size: 20,
    status: VehicleStatus.InPreparation.toString(),
  })

  const changeStatusMutation = useChangeVehicleStatus()

  const handleChangeStatus = async () => {
    if (!selectedVehicle || !reason.trim()) return

    try {
      await changeStatusMutation.mutateAsync({
        id: selectedVehicle.id,
        data: { newStatus, reason: reason.trim() },
      })
      setSelectedVehicle(null)
      setReason('')
      setNewStatus(VehicleStatus.InStock)
    } catch {
      // Error handled by mutation
    }
  }

  const vehicles = data?.data ?? []
  const pagination = data?.pagination

  return (
    <div className="p-6 space-y-4">
      <div>
        <h1 className="text-2xl font-semibold">Preparação</h1>
        <p className="text-muted-foreground">Veículos em preparação / oficina</p>
      </div>

      {isError && (
        <Alert variant="destructive">
          <AlertDescription>
            {error instanceof Error ? error.message : 'Falha ao carregar veículos'}
          </AlertDescription>
        </Alert>
      )}

      {isLoading ? (
        <p className="text-muted-foreground">Carregando...</p>
      ) : vehicles.length === 0 ? (
        <Alert>
          <AlertDescription>Nenhum veículo em preparação no momento.</AlertDescription>
        </Alert>
      ) : (
        <>
          <div className="rounded-md border">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>VIN</TableHead>
                  <TableHead>Marca/Modelo</TableHead>
                  <TableHead>Ano</TableHead>
                  <TableHead>Categoria</TableHead>
                  <TableHead>Localização</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead className="text-right">Ações</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {vehicles.map((vehicle) => (
                  <TableRow key={vehicle.id}>
                    <TableCell className="font-mono text-xs">{vehicle.vin}</TableCell>
                    <TableCell>
                      {vehicle.make} {vehicle.model}
                      {vehicle.trim && ` ${vehicle.trim}`}
                    </TableCell>
                    <TableCell>{vehicle.yearModel}</TableCell>
                    <TableCell>{mapVehicleCategoryLabel(vehicle.category)}</TableCell>
                    <TableCell>{vehicle.location || '-'}</TableCell>
                    <TableCell>
                      <Badge variant="secondary">{mapVehicleStatusLabel(vehicle.currentStatus)}</Badge>
                    </TableCell>
                    <TableCell className="text-right">
                      <Button
                        size="sm"
                        variant="outline"
                        onClick={() => setSelectedVehicle({ id: vehicle.id, vin: vehicle.vin })}
                      >
                        Alterar Status
                      </Button>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>

          {pagination && pagination.totalPages > 1 && (
            <div className="flex items-center justify-between">
              <p className="text-sm text-muted-foreground">
                Página {pagination.page} de {pagination.totalPages} ({pagination.total} veículos)
              </p>
              <div className="flex gap-2">
                <Button
                  variant="outline"
                  size="sm"
                  disabled={pagination.page === 1}
                  onClick={() => setPage(page - 1)}
                >
                  Anterior
                </Button>
                <Button
                  variant="outline"
                  size="sm"
                  disabled={pagination.page >= pagination.totalPages}
                  onClick={() => setPage(page + 1)}
                >
                  Próxima
                </Button>
              </div>
            </div>
          )}
        </>
      )}

      <Dialog open={!!selectedVehicle} onOpenChange={(open) => !open && setSelectedVehicle(null)}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Alterar Status do Veículo</DialogTitle>
            <DialogDescription>VIN: {selectedVehicle?.vin}</DialogDescription>
          </DialogHeader>

          <div className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="new-status">Novo Status</Label>
              <Select value={newStatus.toString()} onValueChange={(v) => setNewStatus(Number(v) as VehicleStatus)}>
                <SelectTrigger id="new-status">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value={VehicleStatus.InStock.toString()}>
                    {mapVehicleStatusLabel(VehicleStatus.InStock)}
                  </SelectItem>
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-2">
              <Label htmlFor="reason">Motivo</Label>
              <Textarea
                id="reason"
                placeholder="Descreva o motivo da mudança de status..."
                value={reason}
                onChange={(e) => setReason(e.target.value)}
                rows={3}
              />
            </div>
          </div>

          <DialogFooter>
            <Button variant="outline" onClick={() => setSelectedVehicle(null)}>
              Cancelar
            </Button>
            <Button
              onClick={handleChangeStatus}
              disabled={!reason.trim() || changeStatusMutation.isPending}
            >
              {changeStatusMutation.isPending ? 'Alterando...' : 'Confirmar'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  )
}
