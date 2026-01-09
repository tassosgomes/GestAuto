import { useEffect, useMemo, useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { Button } from '@/components/ui/button';

// Lead List Page - Updated with navigation
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
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { useLeads } from '../hooks/useLeads';
import { CreateLeadModal } from '../components/CreateLeadModal';
import { LeadScoreBadge } from '../components/LeadScoreBadge';
import { getLeadStatusPresentation } from '../utils/leadStatus';
import { Plus, Search } from 'lucide-react';
import { formatDistanceToNow } from 'date-fns';
import { ptBR } from 'date-fns/locale';
import { useAuth } from '@/auth/useAuth';

export function LeadListPage() {
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();
  const authState = useAuth();
  const [page, setPage] = useState(1);
  const [statusFilters, setStatusFilters] = useState<string[]>([]);
  const [scoreFilter, setScoreFilter] = useState<string | undefined>(undefined);
  const [searchTerm, setSearchTerm] = useState('');
  const [createdFrom, setCreatedFrom] = useState('');
  const [createdTo, setCreatedTo] = useState('');
  const [salesPersonIdFilter, setSalesPersonIdFilter] = useState('');
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);

  useEffect(() => {
    const shouldOpenCreate = searchParams.get('create') === '1';
    if (shouldOpenCreate) {
      setIsCreateModalOpen(true);
    }
  }, [searchParams]);

  const handleCreateModalOpenChange = (open: boolean) => {
    setIsCreateModalOpen(open);

    if (!open && searchParams.get('create') === '1') {
      const nextParams = new URLSearchParams(searchParams);
      nextParams.delete('create');
      setSearchParams(nextParams, { replace: true });
    }
  };

  const { data, isLoading, isError } = useLeads({
    page,
    pageSize: 10,
    status: statusFilters.length > 0 ? statusFilters.join(',') : undefined,
    score: scoreFilter === 'ALL' ? undefined : scoreFilter,
    search: searchTerm,
    createdFrom: createdFrom || undefined,
    createdTo: createdTo || undefined,
    salesPersonId: salesPersonIdFilter || undefined,
  });

  useEffect(() => {
    setPage(1);
  }, [searchTerm, statusFilters, scoreFilter, createdFrom, createdTo, salesPersonIdFilter]);

  const isManager = useMemo(() => {
    if (authState.status !== 'ready' || !authState.session.isAuthenticated) return false;
    return authState.session.roles.includes('MANAGER') || authState.session.roles.includes('SALES_MANAGER') || authState.session.roles.includes('ADMIN');
  }, [authState]);

  const statusOptions: Array<{ value: string; label: string }> = [
    { value: 'New', label: 'Novo' },
    { value: 'InContact', label: 'Contatado' },
    { value: 'InNegotiation', label: 'Em Negociação' },
    { value: 'TestDriveScheduled', label: 'Test-Drive Agendado' },
    { value: 'ProposalSent', label: 'Proposta Enviada' },
    { value: 'Lost', label: 'Perdido' },
    { value: 'Converted', label: 'Convertido' },
  ];

  const selectedStatusLabel = statusFilters.length === 0
    ? 'Todos os status'
    : `${statusFilters.length} selecionado(s)`;

  const toggleStatus = (status: string, checked: boolean) => {
    setStatusFilters((prev) => {
      if (!checked) {
        return prev.filter((value) => value !== status);
      }
      if (prev.includes(status)) return prev;
      return [...prev, status];
    });
  };

  const formatWhatsAppUrl = (phone: string) => {
    const digits = phone.replace(/\D/g, '');
    if (!digits) return undefined;
    const withCountry = digits.startsWith('55') ? digits : `55${digits}`;
    return `https://wa.me/${withCountry}`;
  };

  return (
    <div className="p-6 space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Leads</h1>
          <p className="text-muted-foreground">
            Gerencie seus leads e oportunidades de venda.
          </p>
        </div>
        <Button onClick={() => setIsCreateModalOpen(true)}>
          <Plus className="mr-2 h-4 w-4" /> Novo Lead
        </Button>
      </div>

      <div className="flex items-center gap-4">
        <div className="relative flex-1 max-w-sm">
          <Search className="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground" />
          <Input
            placeholder="Buscar por nome..."
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

        <Select value={scoreFilter} onValueChange={(value) => setScoreFilter(value)}>
          <SelectTrigger className="w-[180px]">
            <SelectValue placeholder="Filtrar por Score" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="ALL">Todos</SelectItem>
            <SelectItem value="Diamond">Diamante</SelectItem>
            <SelectItem value="Gold">Ouro</SelectItem>
            <SelectItem value="Silver">Prata</SelectItem>
            <SelectItem value="Bronze">Bronze</SelectItem>
          </SelectContent>
        </Select>

        <Input
          type="date"
          value={createdFrom}
          onChange={(e) => setCreatedFrom(e.target.value)}
          className="w-[160px]"
          aria-label="Data de criação (de)"
        />
        <Input
          type="date"
          value={createdTo}
          onChange={(e) => setCreatedTo(e.target.value)}
          className="w-[160px]"
          aria-label="Data de criação (até)"
        />

        {isManager && (
          <Input
            value={salesPersonIdFilter}
            onChange={(e) => setSalesPersonIdFilter(e.target.value)}
            placeholder="Vendedor (ID)"
            className="w-[220px]"
          />
        )}
      </div>

      <div className="border rounded-md">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Nome/Contato</TableHead>
              <TableHead>Status</TableHead>
              <TableHead>Score</TableHead>
              <TableHead>Interesse</TableHead>
              <TableHead>Última Interação</TableHead>
              <TableHead>Origem</TableHead>
              <TableHead className="text-right">Ações</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {isLoading ? (
              <TableRow>
                <TableCell colSpan={7} className="text-center h-24">
                  Carregando...
                </TableCell>
              </TableRow>
            ) : isError ? (
              <TableRow>
                <TableCell colSpan={7} className="text-center h-24 text-red-500">
                  Erro ao carregar leads.
                </TableCell>
              </TableRow>
            ) : data?.items?.length === 0 ? (
              <TableRow>
                <TableCell colSpan={7} className="text-center h-24">
                  Nenhum lead encontrado.
                </TableCell>
              </TableRow>
            ) : (
              data?.items?.map((lead) => {
                const statusPresentation = getLeadStatusPresentation(lead.status);
                const whatsappUrl = lead.phone ? formatWhatsAppUrl(lead.phone) : undefined;
                const lastInteraction = lead.lastInteractionAt ? new Date(lead.lastInteractionAt) : undefined;
                const lastInteractionLabel = lastInteraction
                  ? formatDistanceToNow(lastInteraction, { addSuffix: true, locale: ptBR })
                  : '-';

                return (
                  <TableRow key={lead.id}>
                  <TableCell className="font-medium">
                    <div className="flex flex-col">
                      <span>{lead.name}</span>
                      <div className="flex flex-wrap items-center gap-x-2 gap-y-1 text-xs text-muted-foreground">
                        {lead.phone ? <span>{lead.phone}</span> : null}
                        {lead.email ? <span>{lead.email}</span> : null}
                        {whatsappUrl ? (
                          <Button
                            type="button"
                            variant="ghost"
                            size="sm"
                            className="h-6 px-2"
                            onClick={() => window.open(whatsappUrl, '_blank', 'noreferrer')}
                          >
                            WhatsApp
                          </Button>
                        ) : null}
                      </div>
                    </div>
                  </TableCell>
                  <TableCell>
                    <Badge variant={statusPresentation.variant}>
                      {statusPresentation.label}
                    </Badge>
                  </TableCell>
                  <TableCell>
                    <LeadScoreBadge score={lead.score} />
                  </TableCell>
                  <TableCell>
                    {lead.interestedModel || '-'}
                  </TableCell>
                  <TableCell>
                    {lastInteractionLabel}
                  </TableCell>
                  <TableCell>
                    {lead.source ? <Badge variant="outline">{lead.source}</Badge> : '-'}
                  </TableCell>
                  <TableCell className="text-right">
                    <Button 
                      variant="ghost" 
                      size="sm"
                      onClick={() => navigate(`/commercial/leads/${lead.id}`)}
                    >
                      Detalhes
                    </Button>
                  </TableCell>
                  </TableRow>
                );
              })
            )}
          </TableBody>
        </Table>
      </div>

      {/* Pagination Controls (Simple Implementation) */}
      <div className="flex items-center justify-end space-x-2">
        <Button
          variant="outline"
          size="sm"
          onClick={() => setPage((p) => Math.max(1, p - 1))}
          disabled={page === 1 || isLoading}
        >
          Anterior
        </Button>
        <div className="text-sm text-muted-foreground">
          Página {page} de {data?.totalPages || 1}
        </div>
        <Button
          variant="outline"
          size="sm"
          onClick={() => setPage((p) => p + 1)}
          disabled={!data?.hasNextPage || isLoading}
        >
          Próxima
        </Button>
      </div>

      <CreateLeadModal
        open={isCreateModalOpen}
        onOpenChange={handleCreateModalOpenChange}
      />
    </div>
  );
}
