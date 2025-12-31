import { useEffect, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Plus } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { Skeleton } from '@/components/ui/skeleton';
import { useToast } from '@/hooks/use-toast';
import { formatCurrency } from '@/lib/utils';
import { useLeads } from '../hooks/useLeads';
import { useProposals } from '../hooks/useProposals';
import type { ProposalListItem } from '../types';

function formatDateTime(value: string) {
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return value;
  }

  return new Intl.DateTimeFormat('pt-BR', {
    dateStyle: 'short',
    timeStyle: 'short',
  }).format(date);
}

function formatProposalNumber(id: string) {
  const safe = (id ?? '').toString();
  return safe.length > 8 ? safe.slice(0, 8) : safe;
}

function getProposalStatusPresentation(status: string): {
  label: string;
  variant: 'default' | 'secondary' | 'destructive' | 'outline';
} {
  switch (status) {
    case 'Draft':
      return { label: 'Rascunho', variant: 'secondary' };
    case 'InNegotiation':
      return { label: 'Em negociação', variant: 'outline' };
    case 'AwaitingUsedVehicleEvaluation':
      return { label: 'Aguardando avaliação', variant: 'outline' };
    case 'AwaitingDiscountApproval':
      return { label: 'Aguardando aprovação', variant: 'outline' };
    case 'AwaitingCustomer':
      return { label: 'Aguardando cliente', variant: 'outline' };
    case 'Approved':
      return { label: 'Aprovada', variant: 'default' };
    case 'Closed':
      return { label: 'Fechada', variant: 'default' };
    case 'Lost':
      return { label: 'Perdida', variant: 'destructive' };
    default:
      return { label: status, variant: 'secondary' };
  }
}

function ProposalTableSkeleton() {
  return (
    <div className="rounded-md border">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Nº Proposta</TableHead>
            <TableHead>Cliente</TableHead>
            <TableHead>Veículo</TableHead>
            <TableHead>Valor Total</TableHead>
            <TableHead>Status</TableHead>
            <TableHead>Data</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {Array.from({ length: 6 }).map((_, idx) => (
            <TableRow key={idx}>
              <TableCell><Skeleton className="h-4 w-20" /></TableCell>
              <TableCell><Skeleton className="h-4 w-40" /></TableCell>
              <TableCell><Skeleton className="h-4 w-48" /></TableCell>
              <TableCell><Skeleton className="h-4 w-24" /></TableCell>
              <TableCell><Skeleton className="h-6 w-28" /></TableCell>
              <TableCell><Skeleton className="h-4 w-28" /></TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  );
}

export function ProposalListPage() {
  const navigate = useNavigate();
  const { toast } = useToast();
  const [statusFilter, setStatusFilter] = useState<string>('all');

  const proposalsQuery = useProposals({
    page: 1,
    pageSize: 20,
    status: statusFilter === 'all' ? undefined : statusFilter,
  });

  const leadsQuery = useLeads({ page: 1, pageSize: 1000 });

  const leadNameById = useMemo(() => {
    const map = new Map<string, string>();
    for (const lead of leadsQuery.data?.items ?? []) {
      if (lead?.id && lead?.name) {
        map.set(lead.id, lead.name);
      }
    }
    return map;
  }, [leadsQuery.data?.items]);

  useEffect(() => {
    if (!proposalsQuery.isError) {
      return;
    }

    toast({
      variant: 'destructive',
      title: 'Erro ao carregar propostas',
      description: 'Não foi possível buscar as propostas. Tente novamente.',
    });
  }, [proposalsQuery.isError, toast]);

  const items: ProposalListItem[] = proposalsQuery.data?.items ?? [];

  const statusOptions: Array<{ value: string; label: string }> = [
    { value: 'all', label: 'Todos' },
    { value: 'Draft', label: 'Rascunho' },
    { value: 'InNegotiation', label: 'Em negociação' },
    { value: 'AwaitingUsedVehicleEvaluation', label: 'Aguardando avaliação' },
    { value: 'AwaitingDiscountApproval', label: 'Aguardando aprovação' },
    { value: 'AwaitingCustomer', label: 'Aguardando cliente' },
    { value: 'Approved', label: 'Aprovada' },
    { value: 'Closed', label: 'Fechada' },
    { value: 'Lost', label: 'Perdida' },
  ];

  return (
    <div className="p-6 space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Propostas</h1>
          <p className="text-muted-foreground">
            Gerencie as propostas comerciais.
          </p>
        </div>
        <Button onClick={() => navigate('/commercial/proposals/new')}>
          <Plus className="mr-2 h-4 w-4" />
          Nova Proposta
        </Button>
      </div>

      <div className="flex items-center gap-4">
        <div className="w-full max-w-xs">
          <Select value={statusFilter} onValueChange={setStatusFilter}>
            <SelectTrigger>
              <SelectValue placeholder="Filtrar por status" />
            </SelectTrigger>
            <SelectContent>
              {statusOptions.map((opt) => (
                <SelectItem key={opt.value} value={opt.value}>
                  {opt.label}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
      </div>

      {proposalsQuery.isLoading ? (
        <ProposalTableSkeleton />
      ) : proposalsQuery.isError ? (
        <div className="rounded-md border border-dashed p-6 text-center text-muted-foreground">
          Erro ao carregar propostas.
        </div>
      ) : (
        <div className="rounded-md border">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Nº Proposta</TableHead>
                <TableHead>Cliente</TableHead>
                <TableHead>Veículo</TableHead>
                <TableHead>Valor Total</TableHead>
                <TableHead>Status</TableHead>
                <TableHead>Data</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {items.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={6} className="text-center text-muted-foreground">
                    Nenhuma proposta encontrada.
                  </TableCell>
                </TableRow>
              ) : (
                items.map((proposal) => {
                  const status = getProposalStatusPresentation(proposal.status);
                  const leadName = leadNameById.get(proposal.leadId);
                  const vehicle = proposal.vehicleModel?.trim() || 'Veículo não informado';

                  return (
                    <TableRow key={proposal.id}>
                      <TableCell title={proposal.id} className="font-mono">
                        {formatProposalNumber(proposal.id)}
                      </TableCell>
                      <TableCell>
                        {leadsQuery.isLoading ? (
                          <Skeleton className="h-4 w-40" />
                        ) : (
                          leadName ?? proposal.leadId
                        )}
                      </TableCell>
                      <TableCell>{vehicle}</TableCell>
                      <TableCell>{formatCurrency(proposal.totalValue)}</TableCell>
                      <TableCell>
                        <Badge variant={status.variant}>{status.label}</Badge>
                      </TableCell>
                      <TableCell>{formatDateTime(proposal.createdAt)}</TableCell>
                    </TableRow>
                  );
                })
              )}
            </TableBody>
          </Table>
        </div>
      )}
    </div>
  );
}
