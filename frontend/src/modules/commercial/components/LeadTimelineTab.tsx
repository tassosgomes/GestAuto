import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { Button } from '@/components/ui/button';
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from '@/components/ui/form';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Textarea } from '@/components/ui/textarea';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import type { Lead } from '../types';
import { useRegisterInteraction } from '../hooks/useLeads';
import { useToast } from '@/hooks/use-toast';
import { format } from 'date-fns';
import { ptBR } from 'date-fns/locale';
import { MessageSquare, Phone, Mail, User, Calendar } from 'lucide-react';

const interactionSchema = z.object({
  type: z.string().min(1, 'Tipo é obrigatório'),
  description: z.string().min(1, 'Descrição é obrigatória'),
});

type InteractionFormValues = z.infer<typeof interactionSchema>;

interface LeadTimelineTabProps {
  lead: Lead;
}

export function LeadTimelineTab({ lead }: LeadTimelineTabProps) {
  const { toast } = useToast();
  const registerInteraction = useRegisterInteraction();

  const form = useForm<InteractionFormValues>({
    resolver: zodResolver(interactionSchema),
    defaultValues: {
      type: '',
      description: '',
    },
  });

  const onSubmit = (data: InteractionFormValues) => {
    registerInteraction.mutate(
      {
        id: lead.id,
        data: {
          type: data.type,
          description: data.description,
        },
      },
      {
        onSuccess: () => {
          toast({
            title: 'Interação registrada',
            description: 'A interação foi salva com sucesso.',
          });
          form.reset();
        },
        onError: () => {
          toast({
            title: 'Erro ao registrar',
            description: 'Não foi possível salvar a interação.',
            variant: 'destructive',
          });
        },
      }
    );
  };

  const getIcon = (type: string) => {
    switch (type) {
      case 'Call':
        return <Phone className="h-4 w-4" />;
      case 'Email':
        return <Mail className="h-4 w-4" />;
      case 'WhatsApp':
        return <MessageSquare className="h-4 w-4" />;
      case 'Visit':
        return <User className="h-4 w-4" />;
      default:
        return <Calendar className="h-4 w-4" />;
    }
  };

  const interactions = lead.interactions || [];

  return (
    <div className="grid gap-6 md:grid-cols-3">
      <div className="md:col-span-2 space-y-6">
        <Card>
          <CardHeader>
            <CardTitle>Histórico de Interações</CardTitle>
          </CardHeader>
          <CardContent>
            {interactions.length === 0 ? (
              <p className="text-muted-foreground text-center py-4">
                Nenhuma interação registrada.
              </p>
            ) : (
              <div className="space-y-8">
                {interactions.map((interaction) => (
                  <div key={interaction.id} className="flex gap-4">
                    <div className="mt-1 bg-primary/10 p-2 rounded-full h-fit">
                      {getIcon(interaction.type)}
                    </div>
                    <div className="space-y-1">
                      <div className="flex items-center gap-2">
                        <span className="font-semibold">{interaction.type}</span>
                        <span className="text-xs text-muted-foreground">
                          {format(
                            new Date(interaction.occurredAt),
                            "d 'de' MMMM 'às' HH:mm",
                            { locale: ptBR }
                          )}
                        </span>
                      </div>
                      <p className="text-sm text-gray-700">
                        {interaction.description}
                      </p>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </CardContent>
        </Card>
      </div>

      <div>
        <Card>
          <CardHeader>
            <CardTitle>Nova Interação</CardTitle>
          </CardHeader>
          <CardContent>
            <Form {...form}>
              <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
                <FormField
                  control={form.control}
                  name="type"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Tipo</FormLabel>
                      <Select
                        onValueChange={field.onChange}
                        defaultValue={field.value}
                      >
                        <FormControl>
                          <SelectTrigger>
                            <SelectValue placeholder="Selecione..." />
                          </SelectTrigger>
                        </FormControl>
                        <SelectContent>
                          <SelectItem value="Call">Ligação</SelectItem>
                          <SelectItem value="WhatsApp">WhatsApp</SelectItem>
                          <SelectItem value="Email">Email</SelectItem>
                          <SelectItem value="Visit">Visita</SelectItem>
                          <SelectItem value="Other">Outros</SelectItem>
                        </SelectContent>
                      </Select>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <FormField
                  control={form.control}
                  name="description"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Descrição</FormLabel>
                      <FormControl>
                        <Textarea
                          placeholder="Descreva o que foi conversado..."
                          className="min-h-[100px]"
                          {...field}
                        />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <Button
                  type="submit"
                  className="w-full"
                  disabled={registerInteraction.isPending}
                >
                  {registerInteraction.isPending
                    ? 'Salvando...'
                    : 'Registrar Interação'}
                </Button>
              </form>
            </Form>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
