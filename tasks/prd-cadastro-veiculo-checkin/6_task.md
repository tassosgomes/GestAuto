---
status: pending
parallelizable: false
blocked_by: ["5.0"]
---

<task_context>
<domain>frontend/stock</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>high</complexity>
<dependencies>react, shadcn-ui, react-hook-form, zod</dependencies>
<unblocks>"7.0"</unblocks>
</task_context>

# Tarefa 6.0: Implementar DynamicVehicleForm com Campos por Categoria

## Visão Geral

Criar o formulário dinâmico de cadastro de veículo que exibe campos diferentes baseado na categoria selecionada. Este é o componente mais complexo, pois deve validar campos obrigatórios por categoria e preparar os dados para submissão.

<requirements>
- Criar componente em `frontend/src/modules/stock/components/DynamicVehicleForm.tsx`
- Campos variam conforme categoria (novo, seminovo, demonstração)
- Validação com Zod schema dinâmico por categoria
- Usar react-hook-form para gerenciamento de estado
- Campo de observações sempre visível
- Botão submit desabilitado enquanto formulário inválido
- Feedback visual de validação inline
- Nenhum campo de preço no formulário
</requirements>

## Subtarefas

- [ ] 6.1 Criar arquivo `DynamicVehicleForm.tsx` em `frontend/src/modules/stock/components/`
- [ ] 6.2 Definir schema Zod base com campos comuns (VIN, marca, modelo, ano, cor)
- [ ] 6.3 Criar schema Zod para categoria Novo (VIN obrigatório)
- [ ] 6.4 Criar schema Zod para categoria Seminovo (placa, km, evaluationId obrigatórios)
- [ ] 6.5 Criar schema Zod para categoria Demonstração (demoPurpose, isRegistered)
- [ ] 6.6 Implementar lógica de schema dinâmico baseado em categoria
- [ ] 6.7 Implementar campos de input com react-hook-form
- [ ] 6.8 Adicionar campo de observações (textarea)
- [ ] 6.9 Implementar validação inline com mensagens de erro
- [ ] 6.10 Adicionar botão submit com estado de loading
- [ ] 6.11 Exportar componente no index do módulo

## Sequenciamento

- **Bloqueado por**: 5.0 (CategorySelector)
- **Desbloqueia**: 7.0 (Serviço de Orquestração)
- **Paralelizável**: Não — depende da definição de categorias

## Detalhes de Implementação

### Schemas Zod por Categoria

```typescript
// frontend/src/modules/stock/schemas/vehicleSchemas.ts
import { z } from 'zod';
import { VehicleCategory, DemoPurpose } from '../types';

// Schema base comum a todas as categorias
const baseVehicleSchema = z.object({
  vin: z.string().min(1, 'VIN é obrigatório').max(17, 'VIN deve ter no máximo 17 caracteres'),
  make: z.string().min(1, 'Marca é obrigatória'),
  model: z.string().min(1, 'Modelo é obrigatório'),
  yearModel: z.number().min(1900).max(new Date().getFullYear() + 2),
  color: z.string().min(1, 'Cor é obrigatória'),
  trim: z.string().optional(),
  notes: z.string().optional(),
});

// Schema para veículo NOVO
export const newVehicleSchema = baseVehicleSchema.extend({
  category: z.literal(VehicleCategory.New),
  // Placa não obrigatória para novo
  plate: z.string().optional(),
});

// Schema para veículo SEMINOVO
export const usedVehicleSchema = baseVehicleSchema.extend({
  category: z.literal(VehicleCategory.Used),
  plate: z.string().min(7, 'Placa é obrigatória para seminovos'),
  mileageKm: z.number().min(1, 'Quilometragem é obrigatória para seminovos'),
  evaluationId: z.string().uuid('ID de avaliação inválido'),
});

// Schema para veículo DEMONSTRAÇÃO
export const demonstrationVehicleSchema = baseVehicleSchema.extend({
  category: z.literal(VehicleCategory.Demonstration),
  demoPurpose: z.nativeEnum(DemoPurpose, { 
    errorMap: () => ({ message: 'Finalidade é obrigatória para demonstração' }) 
  }),
  isRegistered: z.boolean(),
  plate: z.string().optional().refine(
    (val, ctx) => {
      // Se isRegistered = true, plate é obrigatória
      // Isso será validado no componente
      return true;
    }
  ),
});

// Função para obter schema por categoria
export function getSchemaByCategory(category: VehicleCategory) {
  switch (category) {
    case VehicleCategory.New:
      return newVehicleSchema;
    case VehicleCategory.Used:
      return usedVehicleSchema;
    case VehicleCategory.Demonstration:
      return demonstrationVehicleSchema;
    default:
      return baseVehicleSchema;
  }
}
```

### Estrutura do Componente

```typescript
// frontend/src/modules/stock/components/DynamicVehicleForm.tsx
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { 
  Form, FormControl, FormField, FormItem, FormLabel, FormMessage 
} from '@/components/ui/form';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { Button } from '@/components/ui/button';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Checkbox } from '@/components/ui/checkbox';
import { Loader2 } from 'lucide-react';
import { VehicleCategory, CheckInSource, DemoPurpose } from '../types';
import { getSchemaByCategory } from '../schemas/vehicleSchemas';

interface DynamicVehicleFormProps {
  origin: CheckInSource;
  category: VehicleCategory;
  onSubmit: (data: VehicleFormData) => Promise<void>;
  isSubmitting: boolean;
}

export function DynamicVehicleForm({ 
  origin, 
  category, 
  onSubmit, 
  isSubmitting 
}: DynamicVehicleFormProps) {
  const schema = getSchemaByCategory(category);
  
  const form = useForm({
    resolver: zodResolver(schema),
    defaultValues: {
      category,
      vin: '',
      make: '',
      model: '',
      yearModel: new Date().getFullYear(),
      color: '',
      trim: '',
      plate: '',
      mileageKm: undefined,
      evaluationId: '',
      demoPurpose: undefined,
      isRegistered: false,
      notes: '',
    },
  });

  const isUsed = category === VehicleCategory.Used;
  const isDemonstration = category === VehicleCategory.Demonstration;
  const isRegistered = form.watch('isRegistered');

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
        {/* Campos comuns */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <FormField name="vin" control={form.control} render={...} />
          <FormField name="make" control={form.control} render={...} />
          <FormField name="model" control={form.control} render={...} />
          <FormField name="yearModel" control={form.control} render={...} />
          <FormField name="color" control={form.control} render={...} />
          <FormField name="trim" control={form.control} render={...} />
        </div>

        {/* Campos específicos de Seminovo */}
        {isUsed && (
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <FormField name="plate" control={form.control} render={...} />
            <FormField name="mileageKm" control={form.control} render={...} />
            <FormField name="evaluationId" control={form.control} render={...} />
          </div>
        )}

        {/* Campos específicos de Demonstração */}
        {isDemonstration && (
          <div className="space-y-4">
            <FormField name="demoPurpose" control={form.control} render={...} />
            <FormField name="isRegistered" control={form.control} render={...} />
            {isRegistered && (
              <FormField name="plate" control={form.control} render={...} />
            )}
          </div>
        )}

        {/* Observações */}
        <FormField name="notes" control={form.control} render={...} />

        {/* Botão Submit */}
        <Button type="submit" disabled={isSubmitting} className="w-full">
          {isSubmitting ? (
            <>
              <Loader2 className="mr-2 h-4 w-4 animate-spin" />
              Registrando...
            </>
          ) : (
            'Registrar Entrada'
          )}
        </Button>
      </form>
    </Form>
  );
}
```

### Campos por Categoria

| Campo | Novo | Seminovo | Demonstração |
|-------|------|----------|--------------|
| VIN | ✅ Obrigatório | ✅ Obrigatório | ✅ Obrigatório |
| Marca | ✅ Obrigatório | ✅ Obrigatório | ✅ Obrigatório |
| Modelo | ✅ Obrigatório | ✅ Obrigatório | ✅ Obrigatório |
| Ano | ✅ Obrigatório | ✅ Obrigatório | ✅ Obrigatório |
| Cor | ✅ Obrigatório | ✅ Obrigatório | ✅ Obrigatório |
| Acabamento | Opcional | Opcional | Opcional |
| Placa | Opcional | ✅ Obrigatório | Condicional* |
| KM | ❌ | ✅ Obrigatório | ❌ |
| ID Avaliação | ❌ | ✅ Obrigatório | ❌ |
| Finalidade | ❌ | ❌ | ✅ Obrigatório |
| Emplacado? | ❌ | ❌ | ✅ Obrigatório |
| Observações | Opcional | Opcional | Opcional |

*Condicional: obrigatório se `isRegistered = true`

## Critérios de Sucesso

- [ ] Formulário renderiza campos corretos por categoria
- [ ] Validação Zod funciona para todos os cenários
- [ ] Mensagens de erro aparecem inline nos campos
- [ ] Botão submit desabilitado enquanto formulário inválido
- [ ] Estado de loading exibido durante submissão
- [ ] Campos condicionais aparecem/desaparecem corretamente
- [ ] Nenhum campo de preço presente no formulário
