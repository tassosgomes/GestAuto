import { useEffect, useMemo, useState } from 'react';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import {
  DropdownMenu,
  DropdownMenuCheckboxItem,
  DropdownMenuContent,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { Skeleton } from '@/components/ui/skeleton';
import { useReservationsList } from '../hooks/useReservations';
import { ReservationStatusBadge } from '../components/reservations/ReservationStatusBadge';
import { CancelReservationDialog } from '../components/reservations/CancelReservationDialog';
import { ExtendReservationDialog } from '../components/reservations/ExtendReservationDialog';
import { mapReservationTypeLabel } from '../types';
import { formatBankDeadline, formatReservationDeadline, canUserCancelReservation, canUserExtendReservation } from '../utils/reservationUtils';
import { useAuth } from '@/auth/useAuth';
import { Search } from 'lucide-react';
import { formatDistanceToNow, parseISO } from 'date-fns';
import { ptBR } from 'date-fns/locale';
import type { ReservationListItem } from '../types';

export function StockReservationsPage() {
  const authState = useAuth();
  const [page, setPage] = useState(1);
  const [statusFilters, setStatusFilters] = useState<string[]>([]);
  const [typeFilters, setTypeFilters] = useState<string[]>([]);
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedReservation, setSelectedReservation] = useState<ReservationListItem | null>(null);
  const [isCancelDialogOpen, setIsCancelDialogOpen] = useState(false);
  const [isExtendDialogOpen, setIsExtendDialogOpen] = useState(false);

  const { data, isLoading, isError } = useReservationsList({
    page,
    size: 10,
    status: statusFilters.length > 0 ? statusFilters.join(',') : undefined,
    type: typeFilters.length > 0 ? typeFilters.join(',') : undefined,
    q: searchTerm.trim() ? searchTerm.trim() : undefined,
  });

  useEffect(() => {
    setPage(1);
  }, [statusFilters, typeFilters]);

  const session = useMemo(() => {
    if (authState.status !== 'ready') return null;
    return authState.session;
  }, [authState]);

  const userId = session?.username ?? '';

  const statusOptions: Array<{ value: string; label: string }> = [
    { value: '1', label: 'Ativa' },
    { value: '2', label: 'Cancelada' },
    { value: '3', label: 'Concluída' },
    { value: '4', label: 'Expirada' },
  ];

  const typeOptions: Array<{ value: string; label: string }> = [
    { value: '1', label: 'Padrão' },
    { value: '2', label: 'Entrada paga' },
    { value: '3', label: 'Aguardando banco' },
  ];

  const selectedStatusLabel = statusFilters.length === 0
    ? 'Todos os status'
    : `${statusFilters.length} selecionado(s)`;

  const selectedTypeLabel = typeFilters.length === 0
    ? 'Todos os tipos'
    : `${typeFilters.length} selecionado(s)`;

  const toggleStatus = (status: string, checked: boolean) => {
    setStatusFilters((prev) => {
      if (!checked) return prev.filter((value) => value !== status);
      if (prev.includes(status)) return prev;
      return [...prev, status];
    });
  };

  const toggleType = (type: string, checked: boolean) => {
    setTypeFilters((prev) => {
      if (!checked) return prev.filter((value) => value !== type);
      if (prev.includes(type)) return prev;
      return [...prev, type];
    });
  };

  const handleCancel = (reservation: ReservationListItem) => {
    setSelectedReservation(reservation);
    setIsCancelDialogOpen(true);
  };

  const handleExtend = (reservation: ReservationListItem) => {
    setSelectedReservation(reservation);
    setIsExtendDialogOpen(true);
  };

  return (
    <div className="p-6 space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Reservas</h1>
          <p className="text-muted-foreground">
            Gerencie as reservas de veículos.
          </p>
        </div>
      </div>

      <div className="flex items-center gap-4">
        <div className="relative flex-1 max-w-sm">
          <Search className="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground" />
          <Input
            placeholder="Buscar por veículo..."
            className="pl-8"
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
          />
        </div>

        <DropdownMenu>
          <DropdownMenuTrigger asChild>
            <Button variant="outline" className="min-w-[180px] justify-between">
              {selectedStatusLabel}
            </Button>
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end" className="w-[240px]">
            <DropdownMenuLabel>Status</DropdownMenuLabel>
            <DropdownMenuSeparator />
            <DropdownMenuCheckboxItem
              checked={statusFilters.length === 0}
              onCheckedChange={(checked) => {
                if (checked) setStatusFilters([]);
              }}
            >
              Todos
            </DropdownMenuCheckboxItem>
            <DropdownMenuSeparator />
            {statusOptions.map((option) => (
              <DropdownMenuCheckboxItem
                key={option.value}
                checked={statusFilters.includes(option.value)}
                onCheckedChange={(checked) => toggleStatus(option.value, !!checked)}
              >
                {option.label}
              </DropdownMenuCheckboxItem>
            ))}
          </DropdownMenuContent>
        </DropdownMenu>

        <DropdownMenu>
          <DropdownMenuTrigger asChild>
            <Button variant="outline" className="min-w-[180px] justify-between">
              {selectedTypeLabel}
            </Button>
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end" className="w-[240px]">
            <DropdownMenuLabel>Tipo</DropdownMenuLabel>
            <DropdownMenuSeparator />
            <DropdownMenuCheckboxItem
              checked={typeFilters.length === 0}
              onCheckedChange={(checked) => {
                if (checked) setTypeFilters([]);
              }}
            >
              Todos
            </DropdownMenuCheckboxItem>
            <DropdownMenuSeparator />
            {typeOptions.map((option) => (
              <DropdownMenuCheckboxItem
                key={option.value}
                checked={typeFilters.includes(option.value)}
                onCheckedChange={(checked) => toggleType(option.value, !!checked)}
              >
                {option.label}
              </DropdownMenuCheckboxItem>
            ))}
          </DropdownMenuContent>
        </DropdownMenu>
      </div>

      <div className="rounded-md border">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Veículo</TableHead>
              <TableHead>Tipo</TableHead>
              <TableHead>Status</TableHead>
              <TableHead>Vendedor</TableHead>
              <TableHead>Criada em</TableHead>
              <TableHead>Validade</TableHead>
              <TableHead>Prazo Banco</TableHead>
              <TableHead className="text-right">Ações</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {isLoading ? (
              <TableRow>
                <TableCell colSpan={8} className="text-center py-8">
                  <Skeleton className="h-24 w-full" />
                </TableCell>
              </TableRow>
            ) : isError ? (
              <TableRow>
                <TableCell colSpan={8} className="text-center text-red-500 py-8">
                  Erro ao carregar reservas. Tente novamente.
                </TableCell>
              </TableRow>
            ) : data?.data && data.data.length > 0 ? (
              data.data.map((reservation) => {
                const canCancel = session?.isAuthenticated
                  ? canUserCancelReservation(reservation.salesPersonId, session.roles, userId)
                  : false;
                const canExtend = session?.isAuthenticated
                  ? canUserExtendReservation(session.roles)
                  : false;

                return (
                  <TableRow key={reservation.id}>
                    <TableCell>
                      <div className="font-medium">{reservation.vehicleMake} {reservation.vehicleModel}</div>
                      <div className="text-sm text-muted-foreground">
                        {reservation.vehicleYearModel} {reservation.vehicleTrim && `- ${reservation.vehicleTrim}`}
                      </div>
                      {reservation.vehiclePlate && (
                        <Badge variant="outline" className="mt-1">{reservation.vehiclePlate}</Badge>
                      )}
                    </TableCell>
                    <TableCell>{mapReservationTypeLabel(reservation.type)}</TableCell>
                    <TableCell><ReservationStatusBadge status={reservation.status} /></TableCell>
                    <TableCell>{reservation.salesPersonName}</TableCell>
                    <TableCell>
                      {formatDistanceToNow(parseISO(reservation.createdAtUtc), { locale: ptBR, addSuffix: true })}
                    </TableCell>
                    <TableCell>
                      {formatReservationDeadline(reservation.expiresAtUtc, reservation.status)}
                    </TableCell>
                    <TableCell>
                      {reservation.bankDeadlineAtUtc ? formatBankDeadline(reservation.bankDeadlineAtUtc) : '-'}
                    </TableCell>
                    <TableCell className="text-right">
                      {canCancel && (
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => handleCancel(reservation)}
                        >
                          Cancelar
                        </Button>
                      )}
                      {canExtend && (
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => handleExtend(reservation)}
                        >
                          Prorrogar
                        </Button>
                      )}
                    </TableCell>
                  </TableRow>
                );
              })
            ) : (
              <TableRow>
                <TableCell colSpan={8} className="text-center h-24">
                  <div className="flex flex-col items-center justify-center py-8">
                    <p className="text-muted-foreground mb-2">Nenhuma reserva encontrada</p>
                    <p className="text-sm text-muted-foreground">
                      {statusFilters.length > 0 || typeFilters.length > 0 || searchTerm
                        ? 'Tente ajustar os filtros de busca'
                        : 'As reservas aparecerão aqui'}
                    </p>
                  </div>
                </TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
      </div>

      {data?.pagination && (
        <div className="flex items-center justify-between">
          <div className="text-sm text-muted-foreground">
            Página {data.pagination.page} de {data.pagination.totalPages} ({data.pagination.total} total)
          </div>
          <div className="flex gap-2">
            <Button
              variant="outline"
              size="sm"
              onClick={() => setPage((p) => Math.max(1, p - 1))}
              disabled={page <= 1}
            >
              Anterior
            </Button>
            <Button
              variant="outline"
              size="sm"
              onClick={() => setPage((p) => p + 1)}
              disabled={page >= (data.pagination.totalPages || 1)}
            >
              Próxima
            </Button>
          </div>
        </div>
      )}

      {selectedReservation && (
        <>
          <CancelReservationDialog
            open={isCancelDialogOpen}
            onOpenChange={setIsCancelDialogOpen}
            reservation={selectedReservation}
          />
          <ExtendReservationDialog
            open={isExtendDialogOpen}
            onOpenChange={setIsExtendDialogOpen}
            reservation={selectedReservation}
          />
        </>
      )}
    </div>
  );
}
