import { useEffect, useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
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
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu'
import { useVehiclesList } from '../hooks/useVehicles'
import { VehicleStatus, VehicleCategory, mapVehicleStatusLabel, mapVehicleCategoryLabel } from '../types'
import { useAuth } from '@/auth/useAuth'
import { useToast } from '@/hooks/use-toast'
import { formatCurrency } from '@/lib/utils'
import { Search, Eye, CarFront, MoreHorizontal, Bookmark, PlayCircle } from 'lucide-react'

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

export function StockDashboardPage() {
  const [searchQuery, setSearchQuery] = useState('')
  const [statusFilter, setStatusFilter] = useState<string>('')
  const [categoryFilter, setCategoryFilter] = useState<string>('')

  const normalizedStatusFilter = statusFilter && statusFilter !== 'all' ? statusFilter : undefined
  const normalizedCategoryFilter = categoryFilter && categoryFilter !== 'all' ? categoryFilter : undefined
  
  const { data, isLoading, isError } = useVehiclesList({
    page: 1,
    size: 10,
    q: searchQuery || undefined,
    status: normalizedStatusFilter,
    category: normalizedCategoryFilter,
  })

  const authState = useAuth()
  const { toast } = useToast()
  const userRoles = authState.status === 'ready' ? authState.session.roles : []

  const canViewDetails = userRoles.some(role => 
    ['ADMIN', 'MANAGER', 'STOCK_MANAGER', 'STOCK_PERSON', 'SALES_MANAGER', 'SALES_PERSON'].includes(role)
  )

  const canReserve = userRoles.some(role => 
    ['ADMIN', 'MANAGER', 'SALES_MANAGER', 'SALES_PERSON'].includes(role)
  )

  const canStartTestDrive = userRoles.some(role => 
    ['ADMIN', 'MANAGER', 'SALES_MANAGER', 'SALES_PERSON'].includes(role)
  )

  useEffect(() => {
    if (!isError) {
      return
    }

    toast({
      variant: 'destructive',
      title: 'Erro ao carregar estoque',
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

  const totalVehicles = data?.pagination?.total ?? 0
  const inStockCount = data?.data?.filter(v => v.currentStatus === VehicleStatus.InStock).length ?? 0
  const reservedCount = data?.data?.filter(v => v.currentStatus === VehicleStatus.Reserved).length ?? 0
  const inTestDriveCount = data?.data?.filter(v => v.currentStatus === VehicleStatus.InTestDrive).length ?? 0

  return (
    <div className="space-y-6 p-6">
      <div>
        <h1 className="text-3xl font-bold">Estoque</h1>
        <p className="text-muted-foreground">Visão geral e gestão de veículos</p>
      </div>

      {/* KPIs */}
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Total de Veículos</CardTitle>
            <CarFront className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            {isLoading ? (
              <Skeleton className="h-8 w-20" />
            ) : (
              <div className="text-2xl font-bold">{totalVehicles}</div>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Em Estoque</CardTitle>
          </CardHeader>
          <CardContent>
            {isLoading ? (
              <Skeleton className="h-8 w-20" />
            ) : (
              <div className="text-2xl font-bold">{inStockCount}</div>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Reservados</CardTitle>
          </CardHeader>
          <CardContent>
            {isLoading ? (
              <Skeleton className="h-8 w-20" />
            ) : (
              <div className="text-2xl font-bold">{reservedCount}</div>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Em Test-Drive</CardTitle>
          </CardHeader>
          <CardContent>
            {isLoading ? (
              <Skeleton className="h-8 w-20" />
            ) : (
              <div className="text-2xl font-bold">{inTestDriveCount}</div>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Filters */}
      <Card>
        <CardHeader>
          <CardTitle>Veículos Recentes</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="mb-4 flex flex-col gap-4 sm:flex-row">
            <div className="relative flex-1">
              <Search className="absolute left-2 top-2.5 h-4 w-4 text-muted-foreground" />
              <Input
                placeholder="Buscar por VIN, modelo ou placa..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="pl-8"
              />
            </div>
            <Select value={statusFilter} onValueChange={setStatusFilter}>
              <SelectTrigger className="w-full sm:w-[180px]">
                <SelectValue placeholder="Status" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">Todos os status</SelectItem>
                <SelectItem value="2">Em estoque</SelectItem>
                <SelectItem value="3">Reservado</SelectItem>
                <SelectItem value="4">Em test-drive</SelectItem>
                <SelectItem value="6">Vendido</SelectItem>
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
          </div>

          {/* Table */}
          {isLoading ? (
            <div className="space-y-2">
              <Skeleton className="h-10 w-full" />
              <Skeleton className="h-10 w-full" />
              <Skeleton className="h-10 w-full" />
            </div>
          ) : isError ? (
            <div className="rounded-md border border-dashed p-6 text-center text-muted-foreground">
              Não foi possível carregar os veículos.
            </div>
          ) : data?.data && data.data.length > 0 ? (
            <div className="rounded-md border">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Veículo</TableHead>
                    <TableHead>VIN</TableHead>
                    <TableHead>Placa</TableHead>
                    <TableHead>Ano</TableHead>
                    <TableHead>Categoria</TableHead>
                    <TableHead>Status</TableHead>
                    <TableHead>Dias no estoque</TableHead>
                    <TableHead>Preço</TableHead>
                    {(canViewDetails || canReserve || canStartTestDrive) && (
                      <TableHead className="text-right">Ações</TableHead>
                    )}
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
                      <TableCell>{vehicle.vin}</TableCell>
                      <TableCell>{vehicle.plate || '-'}</TableCell>
                      <TableCell>{vehicle.yearModel}</TableCell>
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
                      {(canViewDetails || canReserve || canStartTestDrive) && (
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
                            </DropdownMenuContent>
                          </DropdownMenu>
                        </TableCell>
                      )}
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </div>
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

          <div className="mt-4 flex justify-center">
            <Button variant="outline" asChild>
              <Link to="/stock/vehicles">Ver todos os veículos</Link>
            </Button>
          </div>
        </CardContent>
      </Card>
    </div>
  )
}
