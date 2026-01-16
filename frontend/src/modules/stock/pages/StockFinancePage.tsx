import { useState } from 'react'
import { useVehiclesList } from '../hooks/useVehicles'
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
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'

export function StockFinancePage() {
  const [page, setPage] = useState(1)
  const [statusFilter, setStatusFilter] = useState<string>('all')

  const getStatusParam = () => {
    if (statusFilter === 'all') return undefined
    return statusFilter
  }

  const { data, isLoading, isError, error } = useVehiclesList({
    page,
    size: 20,
    status: getStatusParam(),
  })

  const vehicles = data?.data ?? []
  const pagination = data?.pagination

  const financialVehicles =
    statusFilter === 'all'
      ? vehicles.filter(
          (v) => v.currentStatus === VehicleStatus.Sold || v.currentStatus === VehicleStatus.Reserved
        )
      : vehicles

  const formatCurrency = (value?: number | null) => {
    if (value == null) return '-'
    return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(value)
  }

  return (
    <div className="p-6 space-y-4">
      <div>
        <h1 className="text-2xl font-semibold">Financeiro</h1>
        <p className="text-muted-foreground">Veículos vendidos e reservados</p>
      </div>

      <div className="flex items-center gap-4">
        <div className="flex items-center gap-2">
          <label htmlFor="status-filter" className="text-sm font-medium">
            Filtrar por:
          </label>
          <Select value={statusFilter} onValueChange={setStatusFilter}>
            <SelectTrigger id="status-filter" className="w-[180px]">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">Vendidos + Reservados</SelectItem>
              <SelectItem value={VehicleStatus.Sold.toString()}>
                {mapVehicleStatusLabel(VehicleStatus.Sold)}
              </SelectItem>
              <SelectItem value={VehicleStatus.Reserved.toString()}>
                {mapVehicleStatusLabel(VehicleStatus.Reserved)}
              </SelectItem>
            </SelectContent>
          </Select>
        </div>
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
      ) : financialVehicles.length === 0 ? (
        <Alert>
          <AlertDescription>Nenhum veículo encontrado com os filtros selecionados.</AlertDescription>
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
                  <TableHead>Preço</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead>Atualizado</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {financialVehicles.map((vehicle) => (
                  <TableRow key={vehicle.id}>
                    <TableCell className="font-mono text-xs">{vehicle.vin}</TableCell>
                    <TableCell>
                      {vehicle.make} {vehicle.model}
                      {vehicle.trim && ` ${vehicle.trim}`}
                    </TableCell>
                    <TableCell>{vehicle.yearModel}</TableCell>
                    <TableCell>{mapVehicleCategoryLabel(vehicle.category)}</TableCell>
                    <TableCell className="font-semibold">{formatCurrency(vehicle.price)}</TableCell>
                    <TableCell>
                      <Badge
                        variant={vehicle.currentStatus === VehicleStatus.Sold ? 'default' : 'secondary'}
                      >
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
                Página {pagination.page} de {pagination.totalPages} ({financialVehicles.length} de{' '}
                {pagination.total} veículos)
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
    </div>
  )
}
