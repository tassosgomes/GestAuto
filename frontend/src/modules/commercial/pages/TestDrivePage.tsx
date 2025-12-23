import { useState, useEffect } from 'react';
import { Plus, Calendar, CheckCircle, XCircle, Clock } from 'lucide-react';
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
import { TestDriveSchedulerModal } from '../components/test-drive/TestDriveSchedulerModal';
import { TestDriveExecutionModal } from '../components/test-drive/TestDriveExecutionModal';
import { testDriveService } from '../services/testDriveService';
import type { TestDrive, ScheduleTestDriveRequest, CompleteTestDriveRequest } from '../types';
import { format } from 'date-fns';
import { ptBR } from 'date-fns/locale';

export function TestDrivePage() {
  const [testDrives, setTestDrives] = useState<TestDrive[]>([]);
  const [isSchedulerOpen, setIsSchedulerOpen] = useState(false);
  const [isExecutionOpen, setIsExecutionOpen] = useState(false);
  const [selectedTestDrive, setSelectedTestDrive] = useState<TestDrive | null>(null);

  useEffect(() => {
    loadTestDrives();
  }, []);

  const loadTestDrives = async () => {
    try {
      const response = await testDriveService.getAll();
      setTestDrives(response.items);
    } catch (error) {
      console.error('Failed to load test drives', error);
    }
  };

  const handleSchedule = async (data: ScheduleTestDriveRequest) => {
    await testDriveService.schedule(data);
    await loadTestDrives();
  };

  const handleComplete = async (id: string, data: CompleteTestDriveRequest) => {
    await testDriveService.complete(id, data);
    await loadTestDrives();
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
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Data/Hora</TableHead>
                <TableHead>Lead</TableHead>
                <TableHead>Veículo</TableHead>
                <TableHead>Status</TableHead>
                <TableHead>Vendedor</TableHead>
                <TableHead className="text-right">Ações</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {testDrives.map((td) => (
                <TableRow key={td.id}>
                  <TableCell>
                    <div className="flex items-center">
                      <Calendar className="mr-2 h-4 w-4 text-muted-foreground" />
                      {format(new Date(td.scheduledAt), "dd/MM/yyyy HH:mm", { locale: ptBR })}
                    </div>
                  </TableCell>
                  <TableCell>{td.leadId === 'lead-1' ? 'João Silva' : 'Maria Santos'}</TableCell>
                  <TableCell>{td.vehicleId === 'vehicle-1' ? 'Honda Civic Touring' : 'Toyota Corolla Altis'}</TableCell>
                  <TableCell>{getStatusBadge(td.status)}</TableCell>
                  <TableCell>Vendedor Atual</TableCell>
                  <TableCell className="text-right">
                    {td.status === 'Scheduled' && (
                      <Button 
                        variant="outline" 
                        size="sm"
                        onClick={() => handleOpenExecution(td)}
                      >
                        Iniciar/Finalizar
                      </Button>
                    )}
                  </TableCell>
                </TableRow>
              ))}
              {testDrives.length === 0 && (
                <TableRow>
                  <TableCell colSpan={6} className="text-center py-8 text-muted-foreground">
                    Nenhum test-drive agendado.
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
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
