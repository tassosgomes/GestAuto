---
status: pending
parallelizable: false
blocked_by: ["4.0"]
---

<task_context>
<domain>frontend/stock</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>low</complexity>
<dependencies>react, shadcn-ui</dependencies>
<unblocks>"6.0"</unblocks>
</task_context>

# Tarefa 5.0: Implementar Componente CategorySelector

## Visão Geral

Criar o componente de seleção de categoria do veículo (Novo, Seminovo, Demonstração). As opções disponíveis dependem da origem selecionada anteriormente. Se apenas uma categoria estiver disponível, ela deve ser auto-selecionada.

<requirements>
- Criar componente em `frontend/src/modules/stock/components/CategorySelector.tsx`
- Receber `origin` como prop para filtrar categorias disponíveis
- Exibir categorias como botões/tabs
- Auto-selecionar quando houver apenas uma opção
- Categoria selecionada deve ter destaque visual
- Emitir evento `onSelect` ao clicar
- Ser acessível (labels, focus states)
</requirements>

## Subtarefas

- [ ] 5.1 Criar arquivo `CategorySelector.tsx` em `frontend/src/modules/stock/components/`
- [ ] 5.2 Importar mapeamento `ORIGIN_CATEGORY_MAP` para filtrar opções
- [ ] 5.3 Implementar lógica de auto-seleção quando apenas uma categoria disponível
- [ ] 5.4 Implementar UI com botões/tabs para seleção
- [ ] 5.5 Adicionar estados de hover e focus
- [ ] 5.6 Implementar seleção com destaque visual
- [ ] 5.7 Adicionar suporte a navegação por teclado
- [ ] 5.8 Adicionar atributos de acessibilidade
- [ ] 5.9 Exportar componente no index do módulo

## Sequenciamento

- **Bloqueado por**: 4.0 (OriginSelector)
- **Desbloqueia**: 6.0 (DynamicVehicleForm)
- **Paralelizável**: Não — depende da implementação anterior

## Detalhes de Implementação

### Estrutura do Componente

```typescript
// frontend/src/modules/stock/components/CategorySelector.tsx
import { useEffect } from 'react';
import { Button } from '@/components/ui/button';
import { cn } from '@/lib/utils';
import { CheckInSource, VehicleCategory } from '../types';
import { ORIGIN_CATEGORY_MAP } from '../constants';

interface CategoryOption {
  value: VehicleCategory;
  label: string;
  description: string;
}

const CATEGORY_OPTIONS: CategoryOption[] = [
  {
    value: VehicleCategory.New,
    label: 'Novo',
    description: '0 km, direto da fábrica',
  },
  {
    value: VehicleCategory.Used,
    label: 'Seminovo',
    description: 'Usado, com placa e quilometragem',
  },
  {
    value: VehicleCategory.Demonstration,
    label: 'Demonstração',
    description: 'Para test-drive ou exposição',
  },
];

interface CategorySelectorProps {
  origin: CheckInSource;
  onSelect: (category: VehicleCategory) => void;
  selected?: VehicleCategory;
}

export function CategorySelector({ 
  origin, 
  onSelect, 
  selected 
}: CategorySelectorProps) {
  // Filtrar categorias disponíveis baseado na origem
  const availableCategories = CATEGORY_OPTIONS.filter(
    (cat) => ORIGIN_CATEGORY_MAP[origin].includes(cat.value)
  );

  // Auto-selecionar se apenas uma opção
  useEffect(() => {
    if (availableCategories.length === 1 && !selected) {
      onSelect(availableCategories[0].value);
    }
  }, [availableCategories, selected, onSelect]);

  // Se apenas uma categoria, mostrar mensagem informativa
  if (availableCategories.length === 1) {
    const category = availableCategories[0];
    return (
      <div className="text-center py-4">
        <p className="text-muted-foreground mb-2">
          Categoria definida automaticamente:
        </p>
        <div className="inline-flex items-center gap-2 px-4 py-2 bg-primary/10 rounded-lg">
          <span className="font-semibold">{category.label}</span>
          <span className="text-sm text-muted-foreground">
            ({category.description})
          </span>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      <p className="text-muted-foreground">
        Selecione a categoria do veículo:
      </p>
      <div 
        className="flex flex-wrap gap-3"
        role="radiogroup"
        aria-label="Selecione a categoria do veículo"
      >
        {availableCategories.map((option) => {
          const isSelected = selected === option.value;
          
          return (
            <Button
              key={option.value}
              variant={isSelected ? 'default' : 'outline'}
              className={cn(
                'flex-1 min-w-[150px] h-auto py-4 flex-col',
                isSelected && 'ring-2 ring-primary ring-offset-2'
              )}
              onClick={() => onSelect(option.value)}
              role="radio"
              aria-checked={isSelected}
            >
              <span className="font-semibold">{option.label}</span>
              <span className="text-xs opacity-80">{option.description}</span>
            </Button>
          );
        })}
      </div>
    </div>
  );
}
```

### Regras de Mapeamento

| Origem | Categorias Disponíveis |
|--------|----------------------|
| Montadora | Novo (auto-select) |
| Compra Cliente/Seminovo | Seminovo (auto-select) |
| Transferência | Novo, Seminovo, Demonstração |
| Frota Interna | Demonstração (auto-select) |

### Comportamento de Auto-Seleção

```
┌─────────────────────────────────────────────────────────┐
│ Origem: Montadora                                       │
│                                                         │
│ Categoria definida automaticamente:                     │
│ ┌─────────────────────────────────────────────────┐   │
│ │  Novo  (0 km, direto da fábrica)                │   │
│ └─────────────────────────────────────────────────┘   │
│                                                         │
│ [Avançar automaticamente para o formulário]            │
└─────────────────────────────────────────────────────────┘
```

## Critérios de Sucesso

- [ ] Categorias filtradas corretamente baseado na origem
- [ ] Auto-seleção funciona quando apenas uma categoria disponível
- [ ] Mensagem informativa exibida em caso de auto-seleção
- [ ] Click em botão dispara `onSelect` com valor correto
- [ ] Botão selecionado tem destaque visual claro
- [ ] Navegação por teclado funciona
- [ ] Screen readers anunciam corretamente as opções
