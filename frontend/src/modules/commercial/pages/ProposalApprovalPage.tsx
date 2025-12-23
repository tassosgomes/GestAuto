import { useEffect, useState } from 'react';
import { Check, X, AlertCircle } from 'lucide-react';
import { Button } from '@/components/ui/button';
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { Badge } from '@/components/ui/badge';
import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert';
import { proposalService } from '../services/proposalService';
import type { Proposal } from '../types';
import { formatCurrency } from '@/lib/utils';

export function ProposalApprovalPage() {
  const [proposals, setProposals] = useState<Proposal[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [actionLoading, setActionLoading] = useState<string | null>(null);

  useEffect(() => {
    loadProposals();
  }, []);

  const loadProposals = async () => {
    try {
      const data = await proposalService.getPendingApprovals();
      setProposals(data);
    } catch (error) {
      console.error('Failed to load pending approvals', error);
    } finally {
      setIsLoading(false);
    }
  };

  const handleApprove = async (id: string) => {
    try {
      setActionLoading(id);
      await proposalService.approveDiscount(id);
      await loadProposals();
    } catch (error) {
      console.error('Failed to approve', error);
    } finally {
      setActionLoading(null);
    }
  };

  const handleReject = async (id: string) => {
    try {
      setActionLoading(id);
      await proposalService.rejectDiscount(id);
      await loadProposals();
    } catch (error) {
      console.error('Failed to reject', error);
    } finally {
      setActionLoading(null);
    }
  };

  if (isLoading) {
    return <div className="p-8">Carregando aprovações...</div>;
  }

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-3xl font-bold tracking-tight">Aprovações Pendentes</h2>
        <p className="text-muted-foreground">
          Gerencie solicitações de desconto e condições especiais.
        </p>
      </div>

      {proposals.length === 0 ? (
        <Alert>
          <AlertCircle className="h-4 w-4" />
          <AlertTitle>Tudo limpo!</AlertTitle>
          <AlertDescription>
            Não há propostas aguardando aprovação no momento.
          </AlertDescription>
        </Alert>
      ) : (
        <Card>
          <CardHeader>
            <CardTitle>Solicitações</CardTitle>
            <CardDescription>
              Propostas que excedem a alçada do vendedor.
            </CardDescription>
          </CardHeader>
          <CardContent>
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Veículo</TableHead>
                  <TableHead>Valor Tabela</TableHead>
                  <TableHead>Desconto Solicitado</TableHead>
                  <TableHead>Motivo</TableHead>
                  <TableHead>Valor Final</TableHead>
                  <TableHead className="text-right">Ações</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {proposals.map((proposal) => {
                  const discountPercent = (proposal.discountAmount / proposal.vehiclePrice) * 100;
                  
                  return (
                    <TableRow key={proposal.id}>
                      <TableCell>
                        <div className="font-medium">
                          {proposal.vehicleModel} {proposal.vehicleTrim}
                        </div>
                        <div className="text-xs text-muted-foreground">
                          {proposal.vehicleYear}
                        </div>
                      </TableCell>
                      <TableCell>{formatCurrency(proposal.vehiclePrice)}</TableCell>
                      <TableCell>
                        <div className="font-bold text-red-600">
                          {formatCurrency(proposal.discountAmount)}
                        </div>
                        <Badge variant="outline" className="mt-1">
                          {discountPercent.toFixed(1)}%
                        </Badge>
                      </TableCell>
                      <TableCell className="max-w-[200px] truncate" title={proposal.discountReason}>
                        {proposal.discountReason || '-'}
                      </TableCell>
                      <TableCell className="font-bold">
                        {formatCurrency(proposal.totalValue)}
                      </TableCell>
                      <TableCell className="text-right space-x-2">
                        <Button 
                          size="sm" 
                          variant="outline" 
                          className="text-green-600 hover:text-green-700 hover:bg-green-50"
                          onClick={() => handleApprove(proposal.id)}
                          disabled={actionLoading === proposal.id}
                        >
                          <Check className="w-4 h-4 mr-1" /> Aprovar
                        </Button>
                        <Button 
                          size="sm" 
                          variant="outline" 
                          className="text-red-600 hover:text-red-700 hover:bg-red-50"
                          onClick={() => handleReject(proposal.id)}
                          disabled={actionLoading === proposal.id}
                        >
                          <X className="w-4 h-4 mr-1" /> Rejeitar
                        </Button>
                      </TableCell>
                    </TableRow>
                  );
                })}
              </TableBody>
            </Table>
          </CardContent>
        </Card>
      )}
    </div>
  );
}
