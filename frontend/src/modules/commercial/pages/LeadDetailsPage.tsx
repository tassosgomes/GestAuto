import { useParams } from 'react-router-dom';
import { useLead } from '../hooks/useLeads';
import { LeadHeader } from '../components/LeadHeader';
import { LeadOverviewTab } from '../components/LeadOverviewTab';
import { LeadTimelineTab } from '../components/LeadTimelineTab';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';

export function LeadDetailsPage() {
  const { id } = useParams<{ id: string }>();
  const { data: lead, isLoading, isError } = useLead(id || '');

  if (isLoading) {
    return (
      <div className="p-6 space-y-6">
        <div className="h-32 bg-gray-100 rounded-lg animate-pulse" />
        <div className="h-64 bg-gray-100 rounded-lg animate-pulse" />
      </div>
    );
  }

  if (isError || !lead) {
    return (
      <div className="p-6 text-center text-red-500">
        Erro ao carregar detalhes do lead.
      </div>
    );
  }

  return (
    <div className="p-6 space-y-6">
      <LeadHeader lead={lead} />

      <Tabs defaultValue="overview" className="w-full">
        <TabsList>
          <TabsTrigger value="overview">Visão Geral</TabsTrigger>
          <TabsTrigger value="timeline">Timeline</TabsTrigger>
          <TabsTrigger value="proposals">Propostas</TabsTrigger>
        </TabsList>
        <TabsContent value="overview" className="mt-6">
          <LeadOverviewTab lead={lead} />
        </TabsContent>
        <TabsContent value="timeline" className="mt-6">
          <LeadTimelineTab lead={lead} />
        </TabsContent>
        <TabsContent value="proposals" className="mt-6">
          <div className="p-4 text-center text-muted-foreground bg-gray-50 rounded-lg border border-dashed">
            Funcionalidade de Propostas em desenvolvimento (Tarefa 7.0).
          </div>
        </TabsContent>
      </Tabs>
    </div>
  );
}
