import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import {
  Tabs,
  TabsContent,
  TabsList,
  TabsTrigger,
} from '@/components/ui/tabs';
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from '@/components/ui/form';
import { Label } from '@/components/ui/label';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { Button } from '@/components/ui/button';
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { useCheckInVehicle, useCheckOutVehicle, useVehiclesList } from '../hooks/useVehicles';
import {
  CheckInSource,
  CheckOutReason,
  VehicleStatus,
} from '../types';
import { useToast } from '@/hooks/use-toast';
import { useAuth } from '@/auth/useAuth';
import { cn } from '@/lib/utils';
import { Loader2 } from 'lucide-react';

const checkInSchema = z.object({
  vehicleId: z.string().min(1, 'Veículo é obrigatório'),
  source: z.number().int().min(1, 'Origem é obrigatória'),
  occurredAt: z.string().optional(),
  notes: z.string().optional(),
});

const checkOutSchema = z.object({
  vehicleId: z.string().min(1, 'Veículo é obrigatório'),
  reason: z.number().int().min(1, 'Motivo é obrigatório'),
  occurredAt: z.string().optional(),
  notes: z.string().optional(),
});

type CheckInFormValues = z.infer<typeof checkInSchema>;
type CheckOutFormValues = z.infer<typeof checkOutSchema>;

const CHECK_IN_SOURCES = [
  { value: CheckInSource.Manufacturer, label: 'Montadora', description: 'Veículos novos da montadora' },
  {
    value: CheckInSource.CustomerUsedPurchase,
    label: 'Compra cliente/seminovo',
    description: 'Veículos usados adquiridos de clientes',
  },
  {
    value: CheckInSource.StoreTransfer,
    label: 'Transferência entre lojas',
    description: 'Veículos transferidos de outra loja',
  },
  { value: CheckInSource.InternalFleet, label: 'Frota interna', description: 'Veículos da frota da empresa' },
];

const CHECK_OUT_REASONS = [
  { value: CheckOutReason.Sale, label: 'Venda', description: 'Veículo vendido para cliente' },
  { value: CheckOutReason.TestDrive, label: 'Test-drive', description: 'Veículo saiu para test-drive' },
  {
    value: CheckOutReason.Transfer,
    label: 'Transferência',
    description: 'Veículo transferido para outra loja',
  },
  {
    value: CheckOutReason.TotalLoss,
    label: 'Baixa sinistro/perda total',
    description: 'Veículo baixado por sinistro ou perda total',
    requiresRole: true,
  },
];

export function StockMovementsPage() {
  const [activeTab, setActiveTab] = useState('checkin');
  const { toast } = useToast();
  const authState = useAuth();

  const userRoles = authState.status === 'ready' ? authState.session.roles : [];
  const canProcessTotalLoss = userRoles.includes('ADMIN') || userRoles.includes('MANAGER');

  // Fetch vehicles for check-in (exclude Sold, WrittenOff)
  const {
    data: checkInVehicles,
    isLoading: isLoadingCheckInVehicles,
    isError: isErrorCheckInVehicles,
  } = useVehiclesList({
    page: 1,
    size: 100,
  });

  // Fetch vehicles for check-out (only InStock, Reserved)
  const {
    data: checkOutVehicles,
    isLoading: isLoadingCheckOutVehicles,
    isError: isErrorCheckOutVehicles,
  } = useVehiclesList({
    page: 1,
    size: 100,
  });

  const checkInForm = useForm<CheckInFormValues>({
    resolver: zodResolver(checkInSchema),
    defaultValues: {
      vehicleId: '',
      source: 0,
      occurredAt: new Date().toISOString().slice(0, 16),
      notes: '',
    },
  });

  const checkOutForm = useForm<CheckOutFormValues>({
    resolver: zodResolver(checkOutSchema),
    defaultValues: {
      vehicleId: '',
      reason: 0,
      occurredAt: new Date().toISOString().slice(0, 16),
      notes: '',
    },
  });

  const [isSubmittingCheckIn, setIsSubmittingCheckIn] = useState(false);
  const [isSubmittingCheckOut, setIsSubmittingCheckOut] = useState(false);

  const checkInMutation = useCheckInVehicle();
  const checkOutMutation = useCheckOutVehicle();

  const onSubmitCheckIn = async (data: CheckInFormValues) => {
    setIsSubmittingCheckIn(true);

    try {
      const occurredAtISO = data.occurredAt ? new Date(data.occurredAt).toISOString() : undefined;

      await checkInMutation.mutateAsync(
        {
          id: data.vehicleId,
          data: {
            source: data.source as CheckInSource,
            occurredAt: occurredAtISO,
            notes: data.notes || null,
          },
        },
        {
          onSuccess: () => {
            toast({
              title: 'Entrada registrada com sucesso! 🚗',
              description: 'O veículo foi registrado no estoque.',
            });
            checkInForm.reset();
          },
          onError: (error: Error) => {
            toast({
              title: 'Erro ao registrar entrada',
              description: error.message || 'Não foi possível registrar a entrada do veículo.',
              variant: 'destructive',
            });
          },
          onSettled: () => {
            setIsSubmittingCheckIn(false);
          },
        }
      );
    } catch (error) {
      setIsSubmittingCheckIn(false);
      toast({
        title: 'Erro ao registrar entrada',
        description: 'Ocorreu um erro inesperado.',
        variant: 'destructive',
      });
    }
  };

  const onSubmitCheckOut = async (data: CheckOutFormValues) => {
    setIsSubmittingCheckOut(true);

    try {
      const occurredAtISO = data.occurredAt ? new Date(data.occurredAt).toISOString() : undefined;

      await checkOutMutation.mutateAsync(
        {
          id: data.vehicleId,
          data: {
            reason: data.reason as CheckOutReason,
            occurredAt: occurredAtISO,
            notes: data.notes || null,
          },
        },
        {
          onSuccess: () => {
            toast({
              title: 'Saída registrada com sucesso! 🚗',
              description: 'A saída do veículo foi registrada.',
            });
            checkOutForm.reset();
          },
          onError: (error: Error) => {
            toast({
              title: 'Erro ao registrar saída',
              description: error.message || 'Não foi possível registrar a saída do veículo.',
              variant: 'destructive',
            });
          },
          onSettled: () => {
            setIsSubmittingCheckOut(false);
          },
        }
      );
    } catch (error) {
      setIsSubmittingCheckOut(false);
      toast({
        title: 'Erro ao registrar saída',
        description: 'Ocorreu um erro inesperado.',
        variant: 'destructive',
      });
    }
  };

  const selectedSource = checkInForm.watch('source');
  const selectedReason = checkOutForm.watch('reason');

  return (
    <div className="p-6 space-y-6">
      <div>
        <h1 className="text-2xl font-semibold">Movimentações</h1>
        <p className="text-muted-foreground">Registre a entrada e saída de veículos no estoque</p>
      </div>

      <Tabs value={activeTab} onValueChange={setActiveTab} className="w-full">
        <TabsList>
          <TabsTrigger value="checkin">Registrar Entrada</TabsTrigger>
          <TabsTrigger value="checkout">Registrar Saída</TabsTrigger>
        </TabsList>

        <TabsContent value="checkin" className="mt-6">
          <div className="max-w-3xl">
            <Form {...checkInForm}>
              <form onSubmit={checkInForm.handleSubmit(onSubmitCheckIn)} className="space-y-6">
                {/* Vehicle Selection */}
                <FormField
                  control={checkInForm.control}
                  name="vehicleId"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Veículo *</FormLabel>
                      <Select onValueChange={field.onChange} value={field.value} disabled={isSubmittingCheckIn}>
                        <FormControl>
                          <SelectTrigger>
                            <SelectValue placeholder="Selecione o veículo" />
                          </SelectTrigger>
                        </FormControl>
                        <SelectContent>
                          {isLoadingCheckInVehicles ? (
                            <div className="flex items-center justify-center p-4">
                              <Loader2 className="h-4 w-4 animate-spin" />
                            </div>
                          ) : isErrorCheckInVehicles ? (
                            <div className="p-4 text-sm text-red-500">Erro ao carregar veículos</div>
                          ) : (
                            checkInVehicles?.data
                              .filter((v) => v.currentStatus !== VehicleStatus.Sold && v.currentStatus !== VehicleStatus.WrittenOff)
                              .map((vehicle) => (
                                <SelectItem key={vehicle.id} value={vehicle.id}>
                                  {vehicle.plate || 'Sem placa'} - {vehicle.make} {vehicle.model} {vehicle.yearModel}
                                </SelectItem>
                              ))
                          )}
                        </SelectContent>
                      </Select>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                {/* Source Selection */}
                <div>
                  <Label>Origem *</Label>
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mt-2">
                    {CHECK_IN_SOURCES.map((source) => (
                      <Card
                        key={source.value}
                        className={cn(
                          'cursor-pointer border-2 hover:border-primary transition-colors',
                          selectedSource === source.value && 'border-primary bg-primary/5'
                        )}
                        onClick={() => !isSubmittingCheckIn && checkInForm.setValue('source', source.value)}
                      >
                        <CardHeader className="pb-3">
                          <CardTitle className="text-lg">{source.label}</CardTitle>
                        </CardHeader>
                        <CardContent>
                          <p className="text-sm text-muted-foreground">{source.description}</p>
                        </CardContent>
                      </Card>
                    ))}
                  </div>
                  {checkInForm.formState.errors.source && (
                    <p className="text-sm font-medium text-destructive mt-2">
                      {checkInForm.formState.errors.source.message}
                    </p>
                  )}
                </div>

                {/* Date/Time */}
                <FormField
                  control={checkInForm.control}
                  name="occurredAt"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Data e Hora</FormLabel>
                      <FormControl>
                        <Input type="datetime-local" {...field} disabled={isSubmittingCheckIn} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                {/* Notes */}
                <FormField
                  control={checkInForm.control}
                  name="notes"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Observações</FormLabel>
                      <FormControl>
                        <Textarea
                          placeholder="Informações adicionais sobre a entrada..."
                          className="min-h-[100px]"
                          {...field}
                          disabled={isSubmittingCheckIn}
                        />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                {/* Submit Button */}
                <Button type="submit" disabled={isSubmittingCheckIn} className="w-full">
                  {isSubmittingCheckIn ? (
                    <>
                      <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                      Registrando entrada...
                    </>
                  ) : (
                    'Registrar Entrada'
                  )}
                </Button>
              </form>
            </Form>
          </div>
        </TabsContent>

        <TabsContent value="checkout" className="mt-6">
          <div className="max-w-3xl">
            <Form {...checkOutForm}>
              <form onSubmit={checkOutForm.handleSubmit(onSubmitCheckOut)} className="space-y-6">
                {/* Vehicle Selection */}
                <FormField
                  control={checkOutForm.control}
                  name="vehicleId"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Veículo *</FormLabel>
                      <Select onValueChange={field.onChange} value={field.value} disabled={isSubmittingCheckOut}>
                        <FormControl>
                          <SelectTrigger>
                            <SelectValue placeholder="Selecione o veículo" />
                          </SelectTrigger>
                        </FormControl>
                        <SelectContent>
                          {isLoadingCheckOutVehicles ? (
                            <div className="flex items-center justify-center p-4">
                              <Loader2 className="h-4 w-4 animate-spin" />
                            </div>
                          ) : isErrorCheckOutVehicles ? (
                            <div className="p-4 text-sm text-red-500">Erro ao carregar veículos</div>
                          ) : (
                            checkOutVehicles?.data
                              .filter(
                                (v) =>
                                  v.currentStatus === VehicleStatus.InStock || v.currentStatus === VehicleStatus.Reserved
                              )
                              .map((vehicle) => (
                                <SelectItem key={vehicle.id} value={vehicle.id}>
                                  {vehicle.plate || 'Sem placa'} - {vehicle.make} {vehicle.model} {vehicle.yearModel}
                                </SelectItem>
                              ))
                          )}
                        </SelectContent>
                      </Select>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                {/* Reason Selection */}
                <div>
                  <Label>Motivo *</Label>
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mt-2">
                    {CHECK_OUT_REASONS.filter(
                      (reason) => !reason.requiresRole || (reason.requiresRole && canProcessTotalLoss)
                    ).map((reason) => (
                      <Card
                        key={reason.value}
                        className={cn(
                          'cursor-pointer border-2 hover:border-primary transition-colors',
                          selectedReason === reason.value && 'border-primary bg-primary/5'
                        )}
                        onClick={() => !isSubmittingCheckOut && checkOutForm.setValue('reason', reason.value)}
                      >
                        <CardHeader className="pb-3">
                          <CardTitle className="text-lg">{reason.label}</CardTitle>
                        </CardHeader>
                        <CardContent>
                          <p className="text-sm text-muted-foreground">{reason.description}</p>
                        </CardContent>
                      </Card>
                    ))}
                  </div>
                  {checkOutForm.formState.errors.reason && (
                    <p className="text-sm font-medium text-destructive mt-2">
                      {checkOutForm.formState.errors.reason.message}
                    </p>
                  )}
                </div>

                {/* Date/Time */}
                <FormField
                  control={checkOutForm.control}
                  name="occurredAt"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Data e Hora</FormLabel>
                      <FormControl>
                        <Input type="datetime-local" {...field} disabled={isSubmittingCheckOut} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                {/* Notes */}
                <FormField
                  control={checkOutForm.control}
                  name="notes"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Observações</FormLabel>
                      <FormControl>
                        <Textarea
                          placeholder="Informações adicionais sobre a saída..."
                          className="min-h-[100px]"
                          {...field}
                          disabled={isSubmittingCheckOut}
                        />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                {/* Submit Button */}
                <Button type="submit" disabled={isSubmittingCheckOut} className="w-full">
                  {isSubmittingCheckOut ? (
                    <>
                      <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                      Registrando saída...
                    </>
                  ) : (
                    'Registrar Saída'
                  )}
                </Button>
              </form>
            </Form>
          </div>
        </TabsContent>
      </Tabs>
    </div>
  );
}
