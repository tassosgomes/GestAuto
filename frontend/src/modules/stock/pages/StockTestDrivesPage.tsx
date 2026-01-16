import { useState, useMemo } from 'react';
import { useTestDrives, useCompleteTestDrive } from '../hooks/useTestDrives';
import { CompleteTestDriveModal } from '../components/CompleteTestDriveModal';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Skeleton } from '@/components/ui/skeleton';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { Clock, CheckCircle, XCircle, AlertCircle } from 'lucide-react';
import type { TestDriveListItem } from '../types';
import { useToast } from '@/hooks/use-toast';

const SLA_WARNING_HOURS = 2;
const SLA_CRITICAL_HOURS = 4;

export function StockTestDrivesPage() {
  const [selectedTestDrive, setSelectedTestDrive] = useState<TestDriveListItem | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const { toast } = useToast();

  const { data, isLoading, error, refetch } = useTestDrives({
    status: 'Scheduled',
    pageSize: 100,
  });

  const completeTestDriveMutation = useCompleteTestDrive();

  const testDrives = data?.data ?? [];
  const now = new Date();

  const kpis = useMemo(() => {
    const scheduled = testDrives.filter((td) => td.status === 'Scheduled');
    const inProgress = scheduled.filter((td) => {
      const scheduledDate = new Date(td.scheduledAt);
      return scheduledDate <= now;
    });

    const overdue = inProgress.filter((td) => {
      const scheduledDate = new Date(td.scheduledAt);
      const hoursElapsed = (now.getTime() - scheduledDate.getTime()) / (1000 * 60 * 60);
      return hoursElapsed >= SLA_CRITICAL_HOURS;
    });

    const warning = inProgress.filter((td) => {
      const scheduledDate = new Date(td.scheduledAt);
      const hoursElapsed = (now.getTime() - scheduledDate.getTime()) / (1000 * 60 * 60);
      return hoursElapsed >= SLA_WARNING_HOURS && hoursElapsed < SLA_CRITICAL_HOURS;
    });

    return {
      total: scheduled.length,
      inProgress: inProgress.length,
      warning: warning.length,
      overdue: overdue.length,
    };
  }, [testDrives, now]);

  const getElapsedTime = (scheduledAt: string): { hours: number; minutes: number; status: 'ok' | 'warning' | 'critical' } => {
    const scheduledDate = new Date(scheduledAt);
    const elapsed = now.getTime() - scheduledDate.getTime();
    const hours = Math.floor(elapsed / (1000 * 60 * 60));
    const minutes = Math.floor((elapsed % (1000 * 60 * 60)) / (1000 * 60));

    let status: 'ok' | 'warning' | 'critical' = 'ok';
    if (scheduledDate <= now) {
      if (hours >= SLA_CRITICAL_HOURS) {
        status = 'critical';
      } else if (hours >= SLA_WARNING_HOURS) {
        status = 'warning';
      }
    }

    return { hours: Math.max(0, hours), minutes: Math.max(0, minutes), status };
  };

  const handleComplete = async (id: string, data: any) => {
    try {
      await completeTestDriveMutation.mutateAsync({ testDriveId: id, data });
      toast({
        title: 'Test-drive finalizado',
        description: 'O test-drive foi finalizado com sucesso.',
      });
      refetch();
    } catch (error) {
      toast({
        title: 'Erro',
        description: 'Falha ao finalizar test-drive. Tente novamente.',
        variant: 'destructive',
      });
      throw error;
    }
  };

  const openCompleteModal = (testDrive: TestDriveListItem) => {
    setSelectedTestDrive(testDrive);
    setIsModalOpen(true);
  };

  return (
    <div className="p-6 space-y-6">
      <div>
        <h1 className="text-2xl font-semibold">Test-drives</h1>
        <p className="text-muted-foreground">Monitore test-drives em andamento e finalize quando retornarem</p>
      </div>

      {/* KPIs */}
      <div className="grid gap-4 md:grid-cols-4">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Agendados</CardTitle>
            <Clock className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{kpis.total}</div>
            <p className="text-xs text-muted-foreground">Test-drives agendados</p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Em Andamento</CardTitle>
            <CheckCircle className="h-4 w-4 text-blue-600" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{kpis.inProgress}</div>
            <p className="text-xs text-muted-foreground">Iniciados</p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Atenção</CardTitle>
            <AlertCircle className="h-4 w-4 text-yellow-600" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{kpis.warning}</div>
            <p className="text-xs text-muted-foreground">&gt;{SLA_WARNING_HOURS}h em andamento</p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Atrasados</CardTitle>
            <XCircle className="h-4 w-4 text-red-600" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{kpis.overdue}</div>
            <p className="text-xs text-muted-foreground">&gt;{SLA_CRITICAL_HOURS}h em andamento</p>
          </CardContent>
        </Card>
      </div>

      {/* Test-drives List */}
      <Card>
        <CardHeader>
          <CardTitle>Test-drives Agendados</CardTitle>
          <CardDescription>
            Lista de test-drives agendados e em andamento
          </CardDescription>
        </CardHeader>
        <CardContent>
          {isLoading && (
            <div className="space-y-2">
              <Skeleton className="h-12 w-full" />
              <Skeleton className="h-12 w-full" />
              <Skeleton className="h-12 w-full" />
            </div>
          )}

          {error && (
            <div className="text-sm text-red-500">
              {error instanceof Error ? error.message : 'Falha ao carregar test-drives'}
            </div>
          )}

          {!isLoading && !error && testDrives.length === 0 && (
            <div className="text-center py-8 text-muted-foreground">
              Nenhum test-drive agendado no momento
            </div>
          )}

          {!isLoading && !error && testDrives.length > 0 && (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Cliente</TableHead>
                  <TableHead>Veículo</TableHead>
                  <TableHead>Agendado</TableHead>
                  <TableHead>Tempo Decorrido</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead className="text-right">Ações</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {testDrives.map((testDrive) => {
                  const elapsed = getElapsedTime(testDrive.scheduledAt);
                  const scheduledDate = new Date(testDrive.scheduledAt);
                  const isStarted = scheduledDate <= now;

                  return (
                    <TableRow key={testDrive.id}>
                      <TableCell className="font-medium">{testDrive.leadName}</TableCell>
                      <TableCell>{testDrive.vehicleDescription}</TableCell>
                      <TableCell>{scheduledDate.toLocaleString('pt-BR')}</TableCell>
                      <TableCell>
                        {isStarted ? (
                          <span className={
                            elapsed.status === 'critical' ? 'text-red-600 font-semibold' :
                            elapsed.status === 'warning' ? 'text-yellow-600 font-semibold' :
                            'text-muted-foreground'
                          }>
                            {elapsed.hours}h {elapsed.minutes}m
                          </span>
                        ) : (
                          <span className="text-muted-foreground">Aguardando</span>
                        )}
                      </TableCell>
                      <TableCell>
                        {elapsed.status === 'critical' && isStarted && (
                          <Badge variant="destructive">Atrasado</Badge>
                        )}
                        {elapsed.status === 'warning' && isStarted && (
                          <Badge className="bg-yellow-600">Atenção</Badge>
                        )}
                        {elapsed.status === 'ok' && isStarted && (
                          <Badge variant="secondary">Em Andamento</Badge>
                        )}
                        {!isStarted && (
                          <Badge variant="outline">Agendado</Badge>
                        )}
                      </TableCell>
                      <TableCell className="text-right">
                        {isStarted && (
                          <Button
                            size="sm"
                            onClick={() => openCompleteModal(testDrive)}
                          >
                            Finalizar
                          </Button>
                        )}
                      </TableCell>
                    </TableRow>
                  );
                })}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>

      <CompleteTestDriveModal
        testDrive={selectedTestDrive}
        open={isModalOpen}
        onOpenChange={setIsModalOpen}
        onComplete={handleComplete}
      />
    </div>
  );
}
