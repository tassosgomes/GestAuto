import { useEffect } from 'react';
import { Badge } from '@/components/ui/badge';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { useToast } from '@/hooks/use-toast';
import { formatCurrency } from '@/lib/utils';
import { useProposals } from '../hooks/useProposals';
import type { ProposalListItem } from '../types';

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

export function LeadProposalsTab({
  leadId,
  enabled,
}: {
  leadId: string;
  enabled: boolean;
}) {
  const { toast } = useToast();

  const { data, isLoading, isError } = useProposals(
    { leadId, page: 1, pageSize: 50 },
    { enabled }
  );

  useEffect(() => {
    if (!enabled || !isError) {
      return;
    }

    toast({
      variant: 'destructive',
      title: 'Erro ao carregar propostas',
      description: 'Não foi possível buscar as propostas deste lead. Tente novamente.',
    });
  }, [enabled, isError, toast]);

  if (!enabled) {
    return null;
  }

  if (isLoading) {
    return (
      <div className="space-y-4">
        <div className="h-24 bg-gray-100 rounded-lg animate-pulse" />
        <div className="h-24 bg-gray-100 rounded-lg animate-pulse" />
        <div className="h-24 bg-gray-100 rounded-lg animate-pulse" />
      </div>
    );
  }

  if (isError) {
    return (
      <div className="p-4 text-center text-red-500 bg-gray-50 rounded-lg border border-dashed">
        Erro ao carregar propostas do lead.
      </div>
    );
  }

  const items: ProposalListItem[] = data?.items ?? [];

  if (items.length === 0) {
    return (
      <div className="p-4 text-center text-muted-foreground bg-gray-50 rounded-lg border border-dashed">
        Nenhuma proposta encontrada para este lead.
      </div>
    );
  }

  return (
    <div className="grid gap-4">
      {items.map((proposal) => {
        const status = getProposalStatusPresentation(proposal.status);
        const vehicle = proposal.vehicleModel?.trim() || 'Veículo não informado';

        return (
          <Card key={proposal.id}>
            <CardHeader className="flex flex-row items-start justify-between gap-4">
              <div className="space-y-1">
                <CardTitle className="text-base">{vehicle}</CardTitle>
                <div className="text-sm text-muted-foreground">{formatCurrency(proposal.totalValue)}</div>
              </div>
              <Badge variant={status.variant}>{status.label}</Badge>
            </CardHeader>
            <CardContent />
          </Card>
        );
      })}
    </div>
  );
}
