import { useMemo, useState, useEffect } from 'react';
import { Plus, Calendar, CheckCircle, XCircle, Clock } from 'lucide-react';
import { Button } from '@/components/ui/button';
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { Badge } from '@/components/ui/badge';
import { useToast } from '@/hooks/use-toast';
import { TestDriveSchedulerModal } from '../components/test-drive/TestDriveSchedulerModal';
import { TestDriveExecutionModal } from '../components/test-drive/TestDriveExecutionModal';
import { testDriveService } from '../services/testDriveService';
import type { TestDrive, ScheduleTestDriveRequest, CompleteTestDriveRequest } from '../types';
import { format } from 'date-fns';
import { ptBR } from 'date-fns/locale';

export function TestDrivePage() {
  const [testDrives, setTestDrives] = useState<TestDrive[]>([]);
  const [hasLoadError, setHasLoadError] = useState(false);
  const [isSchedulerOpen, setIsSchedulerOpen] = useState(false);
  const [isExecutionOpen, setIsExecutionOpen] = useState(false);
  const [selectedTestDrive, setSelectedTestDrive] = useState<TestDrive | null>(null);
  const [selectedDate, setSelectedDate] = useState(() => format(new Date(), 'yyyy-MM-dd'));
  const { toast } = useToast();

  useEffect(() => {
    loadTestDrives();
  }, []);

  const loadTestDrives = async () => {
    try {
      setHasLoadError(false);
      const response = await testDriveService.getAll();
      setTestDrives(response.items);
    } catch (error) {
      setHasLoadError(true);
      console.error('Failed to load test drives', error);
      toast({
        title: 'Falha ao carregar test-drives',
        description: 'Tente novamente em instantes.',
        variant: 'destructive',
      });
    }
  };

  const handleSchedule = async (data: ScheduleTestDriveRequest) => {
    try {
      await testDriveService.schedule(data);
      await loadTestDrives();
      toast({ title: 'Test-drive agendado' });
    } catch (error) {
      console.error('Failed to schedule test drive', error);
      toast({
        title: 'Falha ao agendar test-drive',
        description: 'Verifique os dados e tente novamente.',
        variant: 'destructive',
      });
      throw error;
    }
  };

  const handleComplete = async (id: string, data: CompleteTestDriveRequest) => {
    try {
      await testDriveService.complete(id, data);
      await loadTestDrives();
      toast({ title: 'Test-drive finalizado' });
    } catch (error) {
      console.error('Failed to complete test drive', error);
      toast({
        title: 'Falha ao finalizar test-drive',
        description: 'Verifique os dados e tente novamente.',
        variant: 'destructive',
      });
      throw error;
    }
  };

  const handleOpenExecution = (testDrive: TestDrive) => {
    setSelectedTestDrive(testDrive);
    setIsExecutionOpen(true);
  };

  const getStatusBadge = (status: string) => {
    switch (status) {
      case 'Scheduled':
        return <Badge variant="outline" className="bg-blue-50 text-blue-700 border-blue-200"><Clock className="w-3 h-3 mr-1" /> Agendado</Badge>;
      case 'Completed':
        return <Badge variant="outline" className="bg-green-50 text-green-700 border-green-200"><CheckCircle className="w-3 h-3 mr-1" /> Realizado</Badge>;
      case 'Cancelled':
        return <Badge variant="outline" className="bg-red-50 text-red-700 border-red-200"><XCircle className="w-3 h-3 mr-1" /> Cancelado</Badge>;
      default:
        return <Badge variant="secondary">{status}</Badge>;
    }
  };

  const selectedDayItems = useMemo(() => {
    const selected = new Date(`${selectedDate}T00:00:00`);
    return testDrives
      .filter((td) => {
        const d = new Date(td.scheduledAt);
        return (
          d.getFullYear() === selected.getFullYear() &&
          d.getMonth() === selected.getMonth() &&
          d.getDate() === selected.getDate()
        );
      })
      .sort((a, b) => new Date(a.scheduledAt).getTime() - new Date(b.scheduledAt).getTime());
  }, [selectedDate, testDrives]);

  const weekGroups = useMemo(() => {
    const groups = new Map<string, TestDrive[]>();
    for (const td of [...testDrives].sort(
      (a, b) => new Date(a.scheduledAt).getTime() - new Date(b.scheduledAt).getTime(),
    )) {
      const key = format(new Date(td.scheduledAt), 'yyyy-MM-dd');
      const arr = groups.get(key) ?? [];
      arr.push(td);
      groups.set(key, arr);
    }
    return Array.from(groups.entries());
  }, [testDrives]);

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h2 className="text-3xl font-bold tracking-tight">Test-Drives</h2>
          <p className="text-muted-foreground">
            Gerencie a agenda e execução de test-drives.
          </p>
        </div>
        <Button onClick={() => setIsSchedulerOpen(true)}>
          <Plus className="mr-2 h-4 w-4" /> Agendar Test-Drive
        </Button>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Agenda</CardTitle>
          <CardDescription>
            Próximos agendamentos e histórico recente.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <Tabs defaultValue="daily" className="space-y-4">
            <TabsList>
              <TabsTrigger value="daily">Diária</TabsTrigger>
              <TabsTrigger value="weekly">Semanal</TabsTrigger>
            </TabsList>

            <TabsContent value="daily" className="space-y-4">
              <div className="flex items-end justify-between gap-4">
                <div className="space-y-1">
                  <div className="text-sm font-medium">Data</div>
                  <input
                    type="date"
                    className="h-10 rounded-md border border-input bg-background px-3 py-2 text-sm"
                    value={selectedDate}
                    onChange={(e) => setSelectedDate(e.target.value)}
                  />
                </div>
                {hasLoadError && (
                  <div className="text-sm text-muted-foreground">Dados indisponíveis no momento.</div>
                )}
              </div>

              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Data/Hora</TableHead>
                    <TableHead>Lead</TableHead>
                    <TableHead>Veículo</TableHead>
                    <TableHead>Status</TableHead>
                    <TableHead className="text-right">Ações</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {selectedDayItems.map((td) => (
                    <TableRow key={td.id}>
                      <TableCell>
                        <div className="flex items-center">
                          <Calendar className="mr-2 h-4 w-4 text-muted-foreground" />
                          {format(new Date(td.scheduledAt), 'dd/MM/yyyy HH:mm', { locale: ptBR })}
                        </div>
                      </TableCell>
                      <TableCell>{td.leadId}</TableCell>
                      <TableCell>{td.vehicleId}</TableCell>
                      <TableCell>{getStatusBadge(td.status)}</TableCell>
                      <TableCell className="text-right">
                        {td.status === 'Scheduled' && (
                          <Button variant="outline" size="sm" onClick={() => handleOpenExecution(td)}>
                            Iniciar/Finalizar
                          </Button>
                        )}
                      </TableCell>
                    </TableRow>
                  ))}
                  {selectedDayItems.length === 0 && (
                    <TableRow>
                      <TableCell colSpan={5} className="text-center py-8 text-muted-foreground">
                        Nenhum test-drive agendado para este dia.
                      </TableCell>
                    </TableRow>
                  )}
                </TableBody>
              </Table>
            </TabsContent>

            <TabsContent value="weekly" className="space-y-4">
              <div className="text-sm text-muted-foreground">
                Visualização de slots ocupados por dia (agendamentos).
              </div>

              <div className="space-y-3">
                {weekGroups.map(([day, items]) => (
                  <div key={day} className="rounded-md border p-3">
                    <div className="flex items-center justify-between">
                      <div className="font-medium">
                        {format(new Date(`${day}T00:00:00`), 'dd/MM/yyyy', { locale: ptBR })}
                      </div>
                      <div className="text-sm text-muted-foreground">{items.length} agendamento(s)</div>
                    </div>
                    <div className="mt-2 flex flex-wrap gap-2">
                      {items.map((td) => (
                        <Button
                          key={td.id}
                          variant={td.status === 'Scheduled' ? 'outline' : 'secondary'}
                          size="sm"
                          onClick={() => td.status === 'Scheduled' && handleOpenExecution(td)}
                        >
                          {format(new Date(td.scheduledAt), 'HH:mm', { locale: ptBR })} — {td.vehicleId}
                        </Button>
                      ))}
                      {items.length === 0 && (
                        <div className="text-sm text-muted-foreground">Sem agendamentos.</div>
                      )}
                    </div>
                  </div>
                ))}
                {testDrives.length === 0 && (
                  <div className="text-center py-8 text-muted-foreground">Nenhum test-drive agendado.</div>
                )}
              </div>
            </TabsContent>
          </Tabs>
        </CardContent>
      </Card>

      <TestDriveSchedulerModal
        open={isSchedulerOpen}
        onOpenChange={setIsSchedulerOpen}
        onSchedule={handleSchedule}
      />

      <TestDriveExecutionModal
        testDrive={selectedTestDrive}
        open={isExecutionOpen}
        onOpenChange={setIsExecutionOpen}
        onComplete={handleComplete}
      />
    </div>
  );
}
