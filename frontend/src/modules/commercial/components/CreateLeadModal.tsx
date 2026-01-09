import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from '@/components/ui/form';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { useCreateLead } from '../hooks/useLeads';
import { useToast } from '@/hooks/use-toast';

const createLeadSchema = z.object({
  name: z.string().min(3, 'Nome deve ter pelo menos 3 caracteres'),
  email: z.string().email('Email inválido'),
  phone: z
    .string()
    .min(14, 'Telefone inválido')
    .regex(/^\(\d{2}\) \d{4,5}-\d{4}$/, 'Telefone inválido'),
  source: z.string().min(1, 'Origem é obrigatória'),
  interestedModel: z.string().optional(),
  interestedTrim: z.string().optional(),
  interestedColor: z.string().optional(),
});

type CreateLeadFormValues = z.infer<typeof createLeadSchema>;

interface CreateLeadModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function CreateLeadModal({ open, onOpenChange }: CreateLeadModalProps) {
  const { toast } = useToast();
  const createLead = useCreateLead();

  const formatPhone = (value: string) => {
    const digits = value.replace(/\D/g, '').slice(0, 11);
    if (digits.length === 0) return '';

    const ddd = digits.slice(0, 2);
    const rest = digits.slice(2);
    if (rest.length <= 4) return `(${ddd}) ${rest}`;
    if (rest.length <= 8) return `(${ddd}) ${rest.slice(0, 4)}-${rest.slice(4)}`;
    return `(${ddd}) ${rest.slice(0, 5)}-${rest.slice(5)}`;
  };

  const form = useForm<CreateLeadFormValues>({
    resolver: zodResolver(createLeadSchema),
    defaultValues: {
      name: '',
      email: '',
      phone: '',
      source: '',
      interestedModel: '',
      interestedTrim: '',
      interestedColor: '',
    },
  });

  const onSubmit = (data: CreateLeadFormValues) => {
    createLead.mutate(data, {
      onSuccess: () => {
        toast({
          title: 'Lead criado com sucesso',
          description: `O lead ${data.name} foi cadastrado.`,
        });
        onOpenChange(false);
        form.reset();
      },
      onError: (error) => {
        toast({
          title: 'Erro ao criar lead',
          description: 'Ocorreu um erro ao tentar cadastrar o lead.',
          variant: 'destructive',
        });
        console.error(error);
      },
    });
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[425px]">
        <DialogHeader>
          <DialogTitle>Novo Lead</DialogTitle>
          <DialogDescription>
            Preencha os dados abaixo para cadastrar um novo lead.
          </DialogDescription>
        </DialogHeader>
        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
            <FormField
              control={form.control}
              name="name"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Nome</FormLabel>
                  <FormControl>
                    <Input placeholder="João Silva" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
            <FormField
              control={form.control}
              name="email"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Email</FormLabel>
                  <FormControl>
                    <Input placeholder="joao@exemplo.com" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
            <FormField
              control={form.control}
              name="phone"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Telefone</FormLabel>
                  <FormControl>
                    <Input
                      placeholder="(11) 99999-9999"
                      {...field}
                      inputMode="tel"
                      onChange={(e) => field.onChange(formatPhone(e.target.value))}
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
            <FormField
              control={form.control}
              name="source"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Origem</FormLabel>
                  <Select onValueChange={field.onChange} defaultValue={field.value}>
                    <FormControl>
                      <SelectTrigger>
                        <SelectValue placeholder="Selecione a origem" />
                      </SelectTrigger>
                    </FormControl>
                    <SelectContent>
                        <SelectItem value="Instagram">Instagram</SelectItem>
                        <SelectItem value="Referral">Indicação</SelectItem>
                        <SelectItem value="Google">Site / Google</SelectItem>
                        <SelectItem value="Store">Loja</SelectItem>
                        <SelectItem value="Phone">Telefone</SelectItem>
                        <SelectItem value="Showroom">Showroom</SelectItem>
                        <SelectItem value="ClassifiedsPortal">Portal de Classificados</SelectItem>
                        <SelectItem value="Other">Outros</SelectItem>
                    </SelectContent>
                  </Select>
                  <FormMessage />
                </FormItem>
              )}
            />
            <FormField
              control={form.control}
              name="interestedModel"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Modelo de Interesse (Opcional)</FormLabel>
                  <FormControl>
                    <Input placeholder="Ex: Civic, Corolla" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="interestedTrim"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Versão (Opcional)</FormLabel>
                  <FormControl>
                    <Input placeholder="Ex: Touring, Altis" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="interestedColor"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Cor (Opcional)</FormLabel>
                  <FormControl>
                    <Input placeholder="Ex: Prata, Preto" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
            <DialogFooter>
              <Button type="submit" disabled={createLead.isPending}>
                {createLead.isPending ? 'Salvando...' : 'Salvar'}
              </Button>
            </DialogFooter>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  );
}
