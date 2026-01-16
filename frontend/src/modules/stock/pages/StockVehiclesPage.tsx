import { useEffect, useMemo, useState } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
  DropdownMenuSeparator,
} from '@/components/ui/dropdown-menu'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import { Textarea } from '@/components/ui/textarea'
import { useVehiclesList, useChangeVehicleStatus } from '../hooks/useVehicles'
import { VehicleStatus, VehicleCategory, mapVehicleStatusLabel, mapVehicleCategoryLabel } from '../types'
import { useAuth } from '@/auth/useAuth'
import { useToast } from '@/hooks/use-toast'
import { formatCurrency } from '@/lib/utils'
import { Search, Eye, MoreHorizontal, CarFront, Bookmark, PlayCircle, ChevronLeft, ChevronRight, Wrench } from 'lucide-react'

function getStatusVariant(status: number) {
  switch (status) {
    case VehicleStatus.InStock:
      return 'default'
    case VehicleStatus.Reserved:
      return 'secondary'
    case VehicleStatus.InTestDrive:
      return 'outline'
    case VehicleStatus.Sold:
      return 'default'
    default:
      return 'outline'
  }
}

function getCategoryVariant(category: number) {
  switch (category) {
    case VehicleCategory.New:
      return 'default'
    case VehicleCategory.Used:
      return 'secondary'
    case VehicleCategory.Demonstration:
      return 'outline'
    default:
      return 'outline'
  }
}

export function StockVehiclesPage() {
  const [searchParams, setSearchParams] = useSearchParams()
  const [searchQuery, setSearchQuery] = useState(searchParams.get('q') || '')
  const [statusFilter, setStatusFilter] = useState(searchParams.get('status') || '')
  const [categoryFilter, setCategoryFilter] = useState(searchParams.get('category') || '')
  const [statusDialogOpen, setStatusDialogOpen] = useState(false)
  const [statusTargetId, setStatusTargetId] = useState<string | null>(null)
  const [statusTargetLabel, setStatusTargetLabel] = useState<string>('')
  const [selectedStatus, setSelectedStatus] = useState<string>('')
  const [statusReason, setStatusReason] = useState('')
  const [reasonError, setReasonError] = useState<string | null>(null)
  
  const currentPage = parseInt(searchParams.get('page') || '1', 10)
  const pageSize = parseInt(searchParams.get('size') || '20', 10)

  const normalizedStatusFilter = statusFilter && statusFilter !== 'all' ? statusFilter : undefined
  const normalizedCategoryFilter = categoryFilter && categoryFilter !== 'all' ? categoryFilter : undefined
  
  const { data, isLoading, isError } = useVehiclesList({
    page: currentPage,
    size: pageSize,
    q: searchQuery || undefined,
    status: normalizedStatusFilter,
    category: normalizedCategoryFilter,
  })

  const authState = useAuth()
  const { toast } = useToast()
  const userRoles = authState.status === 'ready' ? authState.session.roles : []

  const changeStatusMutation = useChangeVehicleStatus()

  const canViewDetails = userRoles.some(role => 
    ['ADMIN', 'MANAGER', 'STOCK_MANAGER', 'STOCK_PERSON', 'SALES_MANAGER', 'SALES_PERSON'].includes(role)
  )

  const canReserve = userRoles.some(role => 
    ['ADMIN', 'MANAGER', 'SALES_MANAGER', 'SALES_PERSON'].includes(role)
  )

  const canStartTestDrive = userRoles.some(role => 
    ['ADMIN', 'MANAGER', 'SALES_MANAGER', 'SALES_PERSON'].includes(role)
  )

  const canChangeStatus = userRoles.some(role => 
    ['ADMIN', 'MANAGER', 'STOCK_MANAGER'].includes(role)
  )

  useEffect(() => {
    if (!isError) {
      return
    }

    toast({
      variant: 'destructive',
      title: 'Erro ao carregar veículos',
      description: 'Não foi possível buscar os veículos. Tente novamente.',
    })
  }, [isError, toast])

  const getDaysInStock = useMemo(() => {
    return (createdAt: string) => {
      if (!createdAt) {
        return '-'
      }

      const created = new Date(createdAt)
      if (Number.isNaN(created.getTime())) {
        return '-'
      }

      const diffMs = Date.now() - created.getTime()
      const days = Math.max(0, Math.floor(diffMs / (1000 * 60 * 60 * 24)))
      return `${days} dia${days === 1 ? '' : 's'}`
    }
  }, [])

  const statusOptions = useMemo(
    () => [
      { value: String(VehicleStatus.InTransit), label: mapVehicleStatusLabel(VehicleStatus.InTransit) },
      { value: String(VehicleStatus.InStock), label: mapVehicleStatusLabel(VehicleStatus.InStock) },
      { value: String(VehicleStatus.Reserved), label: mapVehicleStatusLabel(VehicleStatus.Reserved) },
      { value: String(VehicleStatus.InTestDrive), label: mapVehicleStatusLabel(VehicleStatus.InTestDrive) },
      { value: String(VehicleStatus.InPreparation), label: mapVehicleStatusLabel(VehicleStatus.InPreparation) },
      { value: String(VehicleStatus.Sold), label: mapVehicleStatusLabel(VehicleStatus.Sold) },
      { value: String(VehicleStatus.WrittenOff), label: mapVehicleStatusLabel(VehicleStatus.WrittenOff) },
    ],
    []
  )

  const openStatusDialog = (vehicleId: string, label: string, currentStatus: number) => {
    setStatusTargetId(vehicleId)
    setStatusTargetLabel(label)
    setSelectedStatus(String(currentStatus))
    setStatusReason('')
    setReasonError(null)
    setStatusDialogOpen(true)
  }

  const handleChangeStatus = async () => {
    if (!statusTargetId) {
      return
    }

    if (!statusReason.trim()) {
      setReasonError('Informe o motivo da alteração.')
      return
    }

    setReasonError(null)

    try {
      await changeStatusMutation.mutateAsync({
        id: statusTargetId,
        data: {
          newStatus: Number(selectedStatus) as VehicleStatus,
          reason: statusReason.trim(),
        },
      })

      toast({
        title: 'Status atualizado',
        description: 'O status do veículo foi alterado com sucesso.',
      })

      setStatusDialogOpen(false)
    } catch (error) {
      const message = error instanceof Error ? error.message : 'Não foi possível alterar o status.'
      toast({
        variant: 'destructive',
        title: 'Erro ao alterar status',
        description: message,
      })
    }
  }

  const handleSearch = () => {
    const params = new URLSearchParams()
    if (searchQuery) params.set('q', searchQuery)
    if (statusFilter && statusFilter !== 'all') params.set('status', statusFilter)
    if (categoryFilter && categoryFilter !== 'all') params.set('category', categoryFilter)
    params.set('page', '1')
    params.set('size', pageSize.toString())
    setSearchParams(params)
  }

  const handlePageChange = (newPage: number) => {
    const params = new URLSearchParams(searchParams)
    params.set('page', newPage.toString())
    setSearchParams(params)
  }

  const handlePageSizeChange = (newSize: string) => {
    const params = new URLSearchParams(searchParams)
    params.set('page', '1')
    params.set('size', newSize)
    setSearchParams(params)
  }

  const totalPages = data?.pagination?.totalPages ?? 0

  return (
    <div className="space-y-6 p-6">
      <div>
        <h1 className="text-3xl font-bold">Veículos</h1>
        <p className="text-muted-foreground">Gestão completa de veículos em estoque</p>
      </div>

      {/* Filters */}
      <Card>
        <CardHeader>
          <CardTitle>Filtros</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="flex flex-col gap-4 sm:flex-row">
            <div className="relative flex-1">
              <Search className="absolute left-2 top-2.5 h-4 w-4 text-muted-foreground" />
              <Input
                placeholder="Buscar por VIN, modelo ou placa..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                onKeyDown={(e) => e.key === 'Enter' && handleSearch()}
                className="pl-8"
              />
            </div>
            <Select value={statusFilter} onValueChange={setStatusFilter}>
              <SelectTrigger className="w-full sm:w-[180px]">
                <SelectValue placeholder="Status" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">Todos os status</SelectItem>
                <SelectItem value="1">Em trânsito</SelectItem>
                <SelectItem value="2">Em estoque</SelectItem>
                <SelectItem value="3">Reservado</SelectItem>
                <SelectItem value="4">Em test-drive</SelectItem>
                <SelectItem value="5">Em preparação</SelectItem>
                <SelectItem value="6">Vendido</SelectItem>
                <SelectItem value="7">Baixado</SelectItem>
              </SelectContent>
            </Select>
            <Select value={categoryFilter} onValueChange={setCategoryFilter}>
              <SelectTrigger className="w-full sm:w-[180px]">
                <SelectValue placeholder="Categoria" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">Todas</SelectItem>
                <SelectItem value="1">Novo</SelectItem>
                <SelectItem value="2">Seminovo</SelectItem>
                <SelectItem value="3">Demonstração</SelectItem>
              </SelectContent>
            </Select>
            <Button onClick={handleSearch}>Buscar</Button>
          </div>
        </CardContent>
      </Card>

      {/* Results */}
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <CardTitle>
              Resultados {data?.pagination && `(${data.pagination.total} veículos)`}
            </CardTitle>
            <Select value={pageSize.toString()} onValueChange={handlePageSizeChange}>
              <SelectTrigger className="w-[120px]">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="10">10 por página</SelectItem>
                <SelectItem value="20">20 por página</SelectItem>
                <SelectItem value="50">50 por página</SelectItem>
                <SelectItem value="100">100 por página</SelectItem>
              </SelectContent>
            </Select>
          </div>
        </CardHeader>
        <CardContent>
          {isLoading ? (
            <div className="space-y-2">
              <Skeleton className="h-10 w-full" />
              <Skeleton className="h-10 w-full" />
              <Skeleton className="h-10 w-full" />
              <Skeleton className="h-10 w-full" />
              <Skeleton className="h-10 w-full" />
            </div>
          ) : isError ? (
            <div className="rounded-md border border-dashed p-6 text-center text-muted-foreground">
              Não foi possível carregar a lista de veículos.
            </div>
          ) : data?.data && data.data.length > 0 ? (
            <>
              <div className="rounded-md border">
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead>Veículo</TableHead>
                      <TableHead>VIN</TableHead>
                      <TableHead>Placa</TableHead>
                      <TableHead>Ano</TableHead>
                      <TableHead>Cor</TableHead>
                      <TableHead>KM</TableHead>
                      <TableHead>Categoria</TableHead>
                      <TableHead>Status</TableHead>
                      <TableHead>Dias no estoque</TableHead>
                      <TableHead>Preço</TableHead>
                      <TableHead className="text-right">Ações</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {data.data.map((vehicle) => (
                      <TableRow key={vehicle.id}>
                        <TableCell className="font-medium">
                          <div>
                            <div>{vehicle.make} {vehicle.model}</div>
                            {vehicle.trim && <div className="text-sm text-muted-foreground">{vehicle.trim}</div>}
                          </div>
                        </TableCell>
                        <TableCell className="font-mono text-xs">{vehicle.vin}</TableCell>
                        <TableCell>{vehicle.plate || '-'}</TableCell>
                        <TableCell>{vehicle.yearModel}</TableCell>
                        <TableCell>{vehicle.color}</TableCell>
                        <TableCell>{vehicle.mileageKm ? vehicle.mileageKm.toLocaleString('pt-BR') : '-'}</TableCell>
                        <TableCell>
                          <Badge variant={getCategoryVariant(vehicle.category)}>
                            {mapVehicleCategoryLabel(vehicle.category)}
                          </Badge>
                        </TableCell>
                        <TableCell>
                          <Badge variant={getStatusVariant(vehicle.currentStatus)}>
                            {mapVehicleStatusLabel(vehicle.currentStatus)}
                          </Badge>
                        </TableCell>
                        <TableCell>
                          {getDaysInStock(vehicle.createdAt)}
                        </TableCell>
                        <TableCell>
                          {vehicle.price ? formatCurrency(vehicle.price) : '-'}
                        </TableCell>
                        <TableCell className="text-right">
                          <DropdownMenu>
                            <DropdownMenuTrigger asChild>
                              <Button variant="ghost" size="sm">
                                <MoreHorizontal className="h-4 w-4" />
                                <span className="sr-only">Abrir menu</span>
                              </Button>
                            </DropdownMenuTrigger>
                            <DropdownMenuContent align="end">
                              {canViewDetails && (
                                <DropdownMenuItem asChild>
                                  <Link to={`/stock/vehicles/${vehicle.id}`}>
                                    <Eye className="mr-2 h-4 w-4" />
                                    Ver detalhes
                                  </Link>
                                </DropdownMenuItem>
                              )}
                              {canReserve && vehicle.currentStatus === VehicleStatus.InStock && (
                                <>
                                  <DropdownMenuSeparator />
                                  <DropdownMenuItem asChild>
                                    <Link to={`/stock/vehicles/${vehicle.id}?action=reserve`}>
                                      <Bookmark className="mr-2 h-4 w-4" />
                                      Reservar
                                    </Link>
                                  </DropdownMenuItem>
                                </>
                              )}
                              {canStartTestDrive && vehicle.currentStatus === VehicleStatus.InStock && (
                                <DropdownMenuItem asChild>
                                  <Link to={`/stock/vehicles/${vehicle.id}?action=testdrive`}>
                                    <PlayCircle className="mr-2 h-4 w-4" />
                                    Iniciar test-drive
                                  </Link>
                                </DropdownMenuItem>
                              )}
                              {canChangeStatus && (
                                <>
                                  <DropdownMenuSeparator />
                                  <DropdownMenuItem
                                    onClick={() =>
                                      openStatusDialog(
                                        vehicle.id,
                                        `${vehicle.make} ${vehicle.model}`,
                                        vehicle.currentStatus,
                                      )
                                    }
                                  >
                                    <Wrench className="mr-2 h-4 w-4" />
                                    Alterar status
                                  </DropdownMenuItem>
                                </>
                              )}
                            </DropdownMenuContent>
                          </DropdownMenu>
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </div>

              {/* Pagination */}
              {totalPages > 1 && (
                <div className="mt-4 flex items-center justify-between">
                  <div className="text-sm text-muted-foreground">
                    Página {currentPage} de {totalPages}
                  </div>
                  <div className="flex gap-2">
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => handlePageChange(currentPage - 1)}
                      disabled={currentPage === 1}
                    >
                      <ChevronLeft className="h-4 w-4" />
                      Anterior
                    </Button>
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => handlePageChange(currentPage + 1)}
                      disabled={currentPage === totalPages}
                    >
                      Próxima
                      <ChevronRight className="h-4 w-4" />
                    </Button>
                  </div>
                </div>
              )}
            </>
          ) : (
            <div className="flex flex-col items-center justify-center py-10 text-center">
              <CarFront className="h-12 w-12 text-muted-foreground" />
              <h3 className="mt-4 text-lg font-semibold">Nenhum veículo encontrado</h3>
              <p className="text-muted-foreground">
                {searchQuery || normalizedStatusFilter || normalizedCategoryFilter
                  ? 'Tente ajustar os filtros de busca'
                  : 'Não há veículos cadastrados no momento'}
              </p>
            </div>
          )}
        </CardContent>
      </Card>

      <Dialog open={statusDialogOpen} onOpenChange={setStatusDialogOpen}>
        <DialogContent className="sm:max-w-[520px]">
          <DialogHeader>
            <DialogTitle>Alterar status</DialogTitle>
            <DialogDescription>
              {statusTargetLabel ? `Atualize o status de ${statusTargetLabel}.` : 'Atualize o status do veículo.'}
            </DialogDescription>
          </DialogHeader>

          <div className="space-y-4">
            <div className="space-y-2">
              <label className="text-sm font-medium">Novo status</label>
              <Select value={selectedStatus} onValueChange={setSelectedStatus}>
                <SelectTrigger>
                  <SelectValue placeholder="Selecione" />
                </SelectTrigger>
                <SelectContent>
                  {statusOptions.map((option) => (
                    <SelectItem key={option.value} value={option.value}>
                      {option.label}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-2">
              <label className="text-sm font-medium">Motivo</label>
              <Textarea
                placeholder="Descreva o motivo da alteração"
                value={statusReason}
                onChange={(event) => {
                  setStatusReason(event.target.value)
                  if (reasonError) setReasonError(null)
                }}
              />
              {reasonError && <p className="text-sm text-destructive">{reasonError}</p>}
            </div>
          </div>

          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={() => setStatusDialogOpen(false)}
              disabled={changeStatusMutation.isPending}
            >
              Cancelar
            </Button>
            <Button
              type="button"
              onClick={() => void handleChangeStatus()}
              disabled={changeStatusMutation.isPending}
            >
              {changeStatusMutation.isPending ? 'Salvando...' : 'Salvar'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  )
}
