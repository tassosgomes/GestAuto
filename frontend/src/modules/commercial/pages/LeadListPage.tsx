import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
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
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Badge } from '@/components/ui/badge';
import { useLeads } from '../hooks/useLeads';
import { CreateLeadModal } from '../components/CreateLeadModal';
import { LeadScoreBadge } from '../components/LeadScoreBadge';
import { Plus, Search } from 'lucide-react';
import { format } from 'date-fns';
import { ptBR } from 'date-fns/locale';

export function LeadListPage() {
  const navigate = useNavigate();
  const [page, setPage] = useState(1);
  const [statusFilter, setStatusFilter] = useState<string | undefined>(undefined);
  const [searchTerm, setSearchTerm] = useState('');
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);

  const { data, isLoading, isError } = useLeads({
    page,
    pageSize: 10,
    status: statusFilter === 'ALL' ? undefined : statusFilter,
    search: searchTerm,
  });

  const getStatusBadgeVariant = (status: string) => {
    switch (status) {
      case 'New':
        return 'default';
      case 'Contacted':
        return 'secondary';
      case 'Qualified':
        return 'outline';
      case 'Converted':
        return 'default'; // success variant not standard in shadcn badge, using default
      case 'Lost':
        return 'destructive';
      default:
        return 'secondary';
    }
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
        <Select
          value={statusFilter}
          onValueChange={(value) => setStatusFilter(value)}
        >
          <SelectTrigger className="w-[180px]">
            <SelectValue placeholder="Filtrar por Status" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="ALL">Todos</SelectItem>
            <SelectItem value="New">Novo</SelectItem>
            <SelectItem value="Contacted">Contatado</SelectItem>
            <SelectItem value="Qualified">Qualificado</SelectItem>
            <SelectItem value="Converted">Convertido</SelectItem>
            <SelectItem value="Lost">Perdido</SelectItem>
          </SelectContent>
        </Select>
      </div>

      <div className="border rounded-md">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Nome</TableHead>
              <TableHead>Status</TableHead>
              <TableHead>Score</TableHead>
              <TableHead>Interesse</TableHead>
              <TableHead>Data Criação</TableHead>
              <TableHead className="text-right">Ações</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {isLoading ? (
              <TableRow>
                <TableCell colSpan={6} className="text-center h-24">
                  Carregando...
                </TableCell>
              </TableRow>
            ) : isError ? (
              <TableRow>
                <TableCell colSpan={6} className="text-center h-24 text-red-500">
                  Erro ao carregar leads.
                </TableCell>
              </TableRow>
            ) : data?.items?.length === 0 ? (
              <TableRow>
                <TableCell colSpan={6} className="text-center h-24">
                  Nenhum lead encontrado.
                </TableCell>
              </TableRow>
            ) : (
              data?.items?.map((lead) => (
                <TableRow key={lead.id}>
                  <TableCell className="font-medium">
                    <div className="flex flex-col">
                      <span>{lead.name}</span>
                      <span className="text-xs text-muted-foreground">
                        {lead.email}
                      </span>
                    </div>
                  </TableCell>
                  <TableCell>
                    <Badge variant={getStatusBadgeVariant(lead.status)}>
                      {lead.status}
                    </Badge>
                  </TableCell>
                  <TableCell>
                    <LeadScoreBadge score={lead.score} />
                  </TableCell>
                  <TableCell>
                    {lead.interestedModel || '-'}
                  </TableCell>
                  <TableCell>
                    {format(new Date(lead.createdAt), "dd/MM/yyyy HH:mm", {
                      locale: ptBR,
                    })}
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
              ))
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
        onOpenChange={setIsCreateModalOpen}
      />
    </div>
  );
}
