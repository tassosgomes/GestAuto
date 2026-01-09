import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Plus, TrendingUp, FileText, Calendar, Users } from 'lucide-react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
import { dashboardService, type DashboardData } from '../services/dashboardService';
import { getLeadStatusPresentation } from '../utils/leadStatus';
import { formatDistanceToNow } from 'date-fns';
import { ptBR } from 'date-fns/locale';

export function DashboardPage() {
  const navigate = useNavigate();
  const [data, setData] = useState<DashboardData | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadDashboard();
  }, []);

  const loadDashboard = async () => {
    try {
      setLoading(true);
      const dashboardData = await dashboardService.getDashboardData();
      setData(dashboardData);
    } catch (error) {
      console.error('Erro ao carregar dashboard:', error);
    } finally {
      setLoading(false);
    }
  };

  const getScoreIcon = (score: string) => {
    switch (score) {
      case 'Diamond':
        return '💎';
      case 'Gold':
        return '🥇';
      case 'Silver':
        return '🥈';
      case 'Bronze':
        return '🥉';
      default:
        return '';
    }
  };

  if (loading) {
    return (
      <div className="p-6 space-y-6">
        <div>
          <Skeleton className="h-8 w-64 mb-2" />
          <Skeleton className="h-4 w-96" />
        </div>
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
          {[1, 2, 3, 4].map((i) => (
            <Skeleton key={i} className="h-32" />
          ))}
        </div>
      </div>
    );
  }

  if (!data) {
    return (
      <div className="p-6">
        <p className="text-muted-foreground">Erro ao carregar dados do dashboard.</p>
      </div>
    );
  }

  return (
    <div className="p-6 space-y-6">
      {/* Header */}
      <div>
        <h1 className="text-3xl font-bold tracking-tight">Dashboard Comercial</h1>
        <p className="text-muted-foreground">
          Visão geral das suas atividades e métricas do dia
        </p>
      </div>

      {/* KPIs Cards */}
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Leads Novos</CardTitle>
            <Users className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{data.kpis.newLeads}</div>
            <p className="text-xs text-muted-foreground">
              Atribuídos a você
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Propostas em Aberto</CardTitle>
            <FileText className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{data.kpis.openProposals}</div>
            <p className="text-xs text-muted-foreground">
              Em negociação
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Test-Drives Hoje</CardTitle>
            <Calendar className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{data.kpis.testDrivesToday}</div>
            <p className="text-xs text-muted-foreground">
              Agendamentos para hoje
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Taxa de Conversão</CardTitle>
            <TrendingUp className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{data.kpis.conversionRate}%</div>
            <p className="text-xs text-muted-foreground">
              Neste mês
            </p>
          </CardContent>
        </Card>
      </div>

      {/* Action Lists */}
      <div className="grid gap-4 md:grid-cols-2">
        {/* Hot Leads */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              🔥 Leads Quentes
            </CardTitle>
            <CardDescription>
              Top 5 leads Diamante/Ouro sem interação nas últimas 24h
            </CardDescription>
          </CardHeader>
          <CardContent>
            {data.hotLeads.length === 0 ? (
              <p className="text-sm text-muted-foreground text-center py-4">
                Nenhum lead quente no momento
              </p>
            ) : (
              <div className="space-y-3">
                {data.hotLeads.map((lead) => {
                  const statusPresentation = getLeadStatusPresentation(lead.status);

                  return (
                    <div
                      key={lead.id}
                      className="flex items-center justify-between p-3 border rounded-lg hover:bg-accent cursor-pointer transition-colors"
                      onClick={() => navigate(`/commercial/leads/${lead.id}`)}
                    >
                      <div className="flex-1">
                        <div className="flex items-center gap-2 mb-1">
                          <span className="text-lg">{getScoreIcon(lead.score || '')}</span>
                          <span className="font-medium">{lead.name}</span>
                          <Badge variant={statusPresentation.variant} className="text-xs">
                            {statusPresentation.label}
                          </Badge>
                        </div>
                        <div className="flex items-center gap-2 text-sm text-muted-foreground">
                          <span>{lead.phone}</span>
                          <span>•</span>
                          <span>
                            {formatDistanceToNow(new Date(lead.createdAt), {
                              addSuffix: true,
                              locale: ptBR,
                            })}
                          </span>
                        </div>
                      </div>
                    </div>
                  );
                })}
              </div>
            )}
          </CardContent>
        </Card>

        {/* Pending Actions */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              ⏰ Aguardando Você
            </CardTitle>
            <CardDescription>
              Propostas que precisam de ajuste ou leads novos sem contato
            </CardDescription>
          </CardHeader>
          <CardContent>
            {data.pendingActions.length === 0 ? (
              <p className="text-sm text-muted-foreground text-center py-4">
                Nenhuma ação pendente 🎉
              </p>
            ) : (
              <div className="space-y-3">
                {data.pendingActions.map((proposal) => (
                  <div
                    key={proposal.id}
                    className="flex items-center justify-between p-3 border rounded-lg hover:bg-accent cursor-pointer transition-colors"
                    onClick={() => navigate(`/commercial/proposals/${proposal.id}/edit`)}
                  >
                    <div className="flex-1">
                      <div className="font-medium mb-1">
                        {proposal.vehicleModel || 'Proposta'} - {proposal.status}
                      </div>
                      <div className="text-sm text-muted-foreground">
                        Proposta #{proposal.id.slice(0, 8)} • R$ {proposal.totalValue.toLocaleString('pt-BR')}
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Quick Actions - FAB Style Buttons */}
      <Card>
        <CardHeader>
          <CardTitle>Atalhos Rápidos</CardTitle>
          <CardDescription>Ações frequentes para agilizar seu trabalho</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="flex flex-wrap gap-3">
            <Button
              size="lg"
              onClick={() => navigate('/commercial/leads?create=1')}
              className="flex items-center gap-2"
            >
              <Plus className="h-5 w-5" />
              Novo Lead
            </Button>
            <Button
              size="lg"
              variant="outline"
              onClick={() => navigate('/commercial/proposals/new')}
              className="flex items-center gap-2"
            >
              <Plus className="h-5 w-5" />
              Nova Proposta
            </Button>
            <Button
              size="lg"
              variant="outline"
              onClick={() => navigate('/commercial/leads')}
              className="flex items-center gap-2"
            >
              <Users className="h-5 w-5" />
              Ver Todos os Leads
            </Button>
            <Button
              size="lg"
              variant="outline"
              onClick={() => navigate('/commercial/test-drives')}
              className="flex items-center gap-2"
            >
              <Calendar className="h-5 w-5" />
              Agendar Test-Drive
            </Button>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
