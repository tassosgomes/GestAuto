import { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { AccessDeniedPage } from '@/pages/AccessDeniedPage';
import { useAuth } from '@/auth/useAuth';
import { useLeads } from '../hooks/useLeads';
import type { Lead } from '../types';
import { LeadScoreBadge } from '../components/LeadScoreBadge';
import { getLeadStatusPresentation } from '../utils/leadStatus';

function normalizeStatusKey(status: string): string {
  return status
    .trim()
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, '');
}

const PIPELINE_STATUS_COLUMNS: string[] = [
  'New',
  'InContact',
  'InNegotiation',
  'TestDriveScheduled',
  'ProposalSent',
  'Converted',
  'Lost',
];

export function PipelinePage() {
  const navigate = useNavigate();
  const authState = useAuth();

  const isAuthenticated = authState.status === 'ready' && authState.session.isAuthenticated;
  const roles = authState.status === 'ready' ? authState.session.roles : [];
  const canAccess = roles.includes('ADMIN') || roles.includes('MANAGER') || roles.includes('SALES_MANAGER');

  const [salesPersonFilter, setSalesPersonFilter] = useState<string>('ALL');

  const { data, isLoading, isError } = useLeads({ page: 1, pageSize: 1000 });

  const sellers = useMemo(() => {
    const ids = new Set<string>();

    for (const lead of data?.items ?? []) {
      if (lead.salesPersonId) {
        ids.add(lead.salesPersonId);
      }
    }

    return Array.from(ids).sort();
  }, [data?.items]);

  const filteredLeads = useMemo(() => {
    const allLeads = data?.items ?? [];

    if (salesPersonFilter === 'ALL') {
      return allLeads;
    }

    return allLeads.filter((lead) => lead.salesPersonId === salesPersonFilter);
  }, [data?.items, salesPersonFilter]);

  const leadsByColumn = useMemo(() => {
    const groups: Record<string, Lead[]> = Object.fromEntries(
      PIPELINE_STATUS_COLUMNS.map((status) => [normalizeStatusKey(status), [] as Lead[]])
    );

    for (const lead of filteredLeads) {
      const key = normalizeStatusKey(lead.status ?? '');
      if (!groups[key]) {
        groups[key] = [];
      }
      groups[key].push(lead);
    }

    return groups;
  }, [filteredLeads]);

  if (authState.status === 'loading') {
    return <div className="p-6">Carregando…</div>;
  }

  if (!isAuthenticated) {
    return <div className="p-6">Redirecionando para login…</div>;
  }

  if (!canAccess) {
    return <AccessDeniedPage />;
  }

  return (
    <div className="p-6 space-y-6">
      <div className="flex items-start justify-between gap-4">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Pipeline</h1>
          <p className="text-muted-foreground">Visão em kanban dos leads por status.</p>
        </div>

        <div className="w-[260px]">
          <Select value={salesPersonFilter} onValueChange={setSalesPersonFilter}>
            <SelectTrigger>
              <SelectValue placeholder="Filtrar por vendedor" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="ALL">Todos os vendedores</SelectItem>
              {sellers.map((id) => (
                <SelectItem key={id} value={id}>
                  {id}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>
      </div>

      {isLoading ? (
        <div className="text-muted-foreground">Carregando leads…</div>
      ) : isError ? (
        <div className="text-destructive">Erro ao carregar leads.</div>
      ) : (
        <div className="overflow-x-auto">
          <div className="flex gap-4 min-w-[900px]">
            {PIPELINE_STATUS_COLUMNS.map((status) => {
              const statusPresentation = getLeadStatusPresentation(status);
              const key = normalizeStatusKey(status);
              const leads = leadsByColumn[key] ?? [];

              return (
                <Card key={status} className="w-[320px] shrink-0">
                  <CardHeader className="pb-3">
                    <CardTitle className="flex items-center justify-between text-base">
                      <span>{statusPresentation.label}</span>
                      <Badge variant={statusPresentation.variant}>{leads.length}</Badge>
                    </CardTitle>
                  </CardHeader>
                  <CardContent className="space-y-3">
                    {leads.length === 0 ? (
                      <div className="text-sm text-muted-foreground">Sem leads.</div>
                    ) : (
                      leads.map((lead) => {
                        const leadStatus = getLeadStatusPresentation(lead.status);

                        return (
                          <Card key={lead.id} className="shadow-none">
                            <CardContent className="p-3 space-y-2">
                              <div className="flex items-start justify-between gap-2">
                                <div className="min-w-0">
                                  <div className="font-medium truncate">{lead.name}</div>
                                  <div className="text-xs text-muted-foreground truncate">
                                    {lead.interestedModel || 'Sem interesse informado'}
                                  </div>
                                </div>
                                <Badge variant={leadStatus.variant}>{leadStatus.label}</Badge>
                              </div>

                              <div className="flex items-center justify-between gap-2">
                                <LeadScoreBadge score={lead.score} />
                                <Button
                                  variant="ghost"
                                  size="sm"
                                  onClick={() => navigate(`/commercial/leads/${lead.id}`)}
                                >
                                  Abrir
                                </Button>
                              </div>
                            </CardContent>
                          </Card>
                        );
                      })
                    )}
                  </CardContent>
                </Card>
              );
            })}
          </div>
        </div>
      )}
    </div>
  );
}
