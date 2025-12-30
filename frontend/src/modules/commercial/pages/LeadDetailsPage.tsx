import { useState } from 'react';
import { useParams } from 'react-router-dom';
import { useQueryClient } from '@tanstack/react-query';
import { useLead } from '../hooks/useLeads';
import { LeadHeader } from '../components/LeadHeader';
import { LeadOverviewTab } from '../components/LeadOverviewTab';
import { LeadTimelineTab } from '../components/LeadTimelineTab';
import { LeadQualificationForm } from '../components/LeadQualificationForm';
import { LeadActionFeedback } from '../components/LeadActionFeedback';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';

export function LeadDetailsPage() {
  const { id } = useParams<{ id: string }>();
  const queryClient = useQueryClient();
  const { data: lead, isLoading, isError } = useLead(id || '');
  const [activeTab, setActiveTab] = useState('overview');

  const handleQualificationSuccess = () => {
    // Invalidar a query do lead para recarregar os dados atualizados
    queryClient.invalidateQueries({ queryKey: ['lead', id] });
    // Mudar para a aba Visão Geral
    setActiveTab('overview');
  };

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

      <Tabs value={activeTab} onValueChange={setActiveTab} className="w-full">
        <TabsList>
          <TabsTrigger value="overview">Visão Geral</TabsTrigger>
          <TabsTrigger value="qualification">Qualificação</TabsTrigger>
          <TabsTrigger value="timeline">Timeline</TabsTrigger>
          <TabsTrigger value="proposals">Propostas</TabsTrigger>
        </TabsList>
        <TabsContent value="overview" className="mt-6">
          <LeadOverviewTab lead={lead} />
        </TabsContent>
        <TabsContent value="qualification" className="mt-6">
          <div className="space-y-6">
            {lead.score && (
              <LeadActionFeedback score={lead.score} />
            )}
            <LeadQualificationForm lead={lead} onSuccess={handleQualificationSuccess} />
          </div>
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
