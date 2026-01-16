import { useState } from 'react'
import { useVehiclesList, useCheckOutVehicle } from '../hooks/useVehicles'
import {
  VehicleStatus,
  CheckOutReason,
  mapVehicleStatusLabel,
  mapVehicleCategoryLabel,
  mapCheckOutReasonLabel,
} from '../types'
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
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'

export function StockWriteOffsPage() {
  const [page, setPage] = useState(1)
  const [activeTab, setActiveTab] = useState<'written-off' | 'available'>('written-off')
  const [selectedVehicle, setSelectedVehicle] = useState<{ id: string; vin: string } | null>(null)
  const [notes, setNotes] = useState('')

  const writtenOffQuery = useVehiclesList({
    page: activeTab === 'written-off' ? page : 1,
    size: 20,
    status: VehicleStatus.WrittenOff.toString(),
  })

  const availableQuery = useVehiclesList({
    page: activeTab === 'available' ? page : 1,
    size: 20,
    status: VehicleStatus.InStock.toString(),
  })

  const checkOutMutation = useCheckOutVehicle()

  const handleWriteOff = async () => {
    if (!selectedVehicle || !notes.trim()) return

    try {
      await checkOutMutation.mutateAsync({
        id: selectedVehicle.id,
        data: {
          reason: CheckOutReason.TotalLoss,
          notes: notes.trim(),
        },
      })
      setSelectedVehicle(null)
      setNotes('')
    } catch {
      // Error handled by mutation
    }
  }

  const currentQuery = activeTab === 'written-off' ? writtenOffQuery : availableQuery
  const vehicles = currentQuery.data?.data ?? []
  const pagination = currentQuery.data?.pagination

  return (
    <div className="p-6 space-y-4">
      <div>
        <h1 className="text-2xl font-semibold">Baixas / Exceções</h1>
        <p className="text-muted-foreground">Gestão de baixas e veículos sinistrados</p>
      </div>

      <Tabs value={activeTab} onValueChange={(v) => setActiveTab(v as typeof activeTab)}>
        <TabsList>
          <TabsTrigger value="written-off">Veículos Baixados</TabsTrigger>
          <TabsTrigger value="available">Registrar Baixa</TabsTrigger>
        </TabsList>

        <TabsContent value="written-off" className="space-y-4">
          {currentQuery.isError && (
            <Alert variant="destructive">
              <AlertDescription>
                {currentQuery.error instanceof Error
                  ? currentQuery.error.message
                  : 'Falha ao carregar veículos'}
              </AlertDescription>
            </Alert>
          )}

          {currentQuery.isLoading ? (
            <p className="text-muted-foreground">Carregando...</p>
          ) : vehicles.length === 0 ? (
            <Alert>
              <AlertDescription>Nenhum veículo baixado encontrado.</AlertDescription>
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
                      <TableHead>Status</TableHead>
                      <TableHead>Data Baixa</TableHead>
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
                        <TableCell>
                          <Badge variant="destructive">
                            {mapVehicleStatusLabel(vehicle.currentStatus)}
                          </Badge>
                        </TableCell>
                        <TableCell className="text-muted-foreground text-xs">
                          {new Date(vehicle.updatedAt).toLocaleString('pt-BR')}
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
        </TabsContent>

        <TabsContent value="available" className="space-y-4">
          <Alert>
            <AlertDescription>
              Selecione um veículo disponível em estoque para registrar uma baixa (sinistro/perda total).
            </AlertDescription>
          </Alert>

          {currentQuery.isError && (
            <Alert variant="destructive">
              <AlertDescription>
                {currentQuery.error instanceof Error
                  ? currentQuery.error.message
                  : 'Falha ao carregar veículos'}
              </AlertDescription>
            </Alert>
          )}

          {currentQuery.isLoading ? (
            <p className="text-muted-foreground">Carregando...</p>
          ) : vehicles.length === 0 ? (
            <Alert>
              <AlertDescription>Nenhum veículo disponível em estoque.</AlertDescription>
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
                        <TableCell>
                          <Badge variant="secondary">
                            {mapVehicleStatusLabel(vehicle.currentStatus)}
                          </Badge>
                        </TableCell>
                        <TableCell className="text-right">
                          <Button
                            size="sm"
                            variant="destructive"
                            onClick={() => setSelectedVehicle({ id: vehicle.id, vin: vehicle.vin })}
                          >
                            Registrar Baixa
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
        </TabsContent>
      </Tabs>

      <Dialog open={!!selectedVehicle} onOpenChange={(open) => !open && setSelectedVehicle(null)}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Registrar Baixa de Veículo</DialogTitle>
            <DialogDescription>
              VIN: {selectedVehicle?.vin}
              <br />
              Motivo: {mapCheckOutReasonLabel(CheckOutReason.TotalLoss)}
            </DialogDescription>
          </DialogHeader>

          <div className="space-y-4">
            <Alert variant="destructive">
              <AlertDescription>
                <strong>Atenção:</strong> Esta ação registrará uma baixa permanente do veículo (sinistro/perda
                total). O veículo será marcado como baixado e não poderá ser vendido.
              </AlertDescription>
            </Alert>

            <div className="space-y-2">
              <Label htmlFor="notes">Observações *</Label>
              <Textarea
                id="notes"
                placeholder="Descreva o motivo da baixa (sinistro, perda total, etc.)..."
                value={notes}
                onChange={(e) => setNotes(e.target.value)}
                rows={4}
              />
            </div>
          </div>

          <DialogFooter>
            <Button variant="outline" onClick={() => setSelectedVehicle(null)}>
              Cancelar
            </Button>
            <Button
              variant="destructive"
              onClick={handleWriteOff}
              disabled={!notes.trim() || checkOutMutation.isPending}
            >
              {checkOutMutation.isPending ? 'Registrando...' : 'Confirmar Baixa'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  )
}
