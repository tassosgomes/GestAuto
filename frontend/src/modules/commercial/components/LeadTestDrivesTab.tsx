import { Badge } from '@/components/ui/badge';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { format } from 'date-fns';
import { ptBR } from 'date-fns/locale';
import type { TestDrive } from '../types';
import { useTestDrives } from '../hooks/useTestDrives';

function getTestDriveStatusPresentation(status: string): {
  label: string;
  variant: 'default' | 'secondary' | 'destructive' | 'outline';
} {
  switch (status) {
    case 'Scheduled':
      return { label: 'Agendado', variant: 'outline' };
    case 'Completed':
      return { label: 'Realizado', variant: 'default' };
    case 'Cancelled':
      return { label: 'Cancelado', variant: 'destructive' };
    case 'NoShow':
      return { label: 'Não Compareceu', variant: 'secondary' };
    default:
      return { label: status, variant: 'secondary' };
  }
}

export function LeadTestDrivesTab({
  leadId,
  enabled,
}: {
  leadId: string;
  enabled: boolean;
}) {
  const { data, isLoading, isError } = useTestDrives(
    { leadId, page: 1, pageSize: 50 },
    { enabled }
  );

  if (!enabled) {
    return null;
  }

  if (isLoading) {
    return (
      <div className="p-6 space-y-4">
        <div className="h-10 bg-gray-100 rounded-lg animate-pulse" />
        <div className="h-48 bg-gray-100 rounded-lg animate-pulse" />
      </div>
    );
  }

  if (isError) {
    return (
      <div className="p-4 text-center text-red-500 bg-gray-50 rounded-lg border border-dashed">
        Erro ao carregar histórico de test-drives.
      </div>
    );
  }

  const items: TestDrive[] = data?.items ?? [];

  return (
    <div className="space-y-4">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Data/Hora</TableHead>
            <TableHead>Veículo</TableHead>
            <TableHead>Status</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {items.map((td) => {
            const status = getTestDriveStatusPresentation(td.status);
            return (
              <TableRow key={td.id}>
                <TableCell>
                  {format(new Date(td.scheduledAt), 'dd/MM/yyyy HH:mm', {
                    locale: ptBR,
                  })}
                </TableCell>
                <TableCell>{td.vehicleId}</TableCell>
                <TableCell>
                  <Badge variant={status.variant}>{status.label}</Badge>
                </TableCell>
              </TableRow>
            );
          })}

          {items.length === 0 && (
            <TableRow>
              <TableCell colSpan={3} className="text-center py-8 text-muted-foreground">
                Nenhum test-drive encontrado para este lead.
              </TableCell>
            </TableRow>
          )}
        </TableBody>
      </Table>
    </div>
  );
}
