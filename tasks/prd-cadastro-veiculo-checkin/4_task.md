---
status: pending
parallelizable: false
blocked_by: ["3.0"]
---

<task_context>
<domain>frontend/stock</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>low</complexity>
<dependencies>react, shadcn-ui</dependencies>
<unblocks>"5.0"</unblocks>
</task_context>

# Tarefa 4.0: Implementar Componente OriginSelector

## Visão Geral

Criar o componente de seleção de origem do veículo usando cards clicáveis. A origem determina de onde o veículo está chegando (montadora, compra de cliente, transferência entre lojas, frota interna) e influencia quais categorias estarão disponíveis no próximo passo.

<requirements>
- Criar componente em `frontend/src/modules/stock/components/OriginSelector.tsx`
- Exibir 4 cards clicáveis: Montadora, Compra Cliente/Seminovo, Transferência, Frota Interna
- Cada card deve ter ícone, título e descrição
- Card selecionado deve ter destaque visual
- Emitir evento `onSelect` ao clicar
- Suportar navegação por teclado
- Ser acessível (labels, focus states)
</requirements>

## Subtarefas

- [ ] 4.1 Criar arquivo `OriginSelector.tsx` em `frontend/src/modules/stock/components/`
- [ ] 4.2 Definir array de opções de origem com ícone, título, descrição
- [ ] 4.3 Implementar grid de cards responsivo (2 colunas em mobile, 4 em desktop)
- [ ] 4.4 Adicionar estados de hover e focus
- [ ] 4.5 Implementar seleção com destaque visual (borda/background)
- [ ] 4.6 Adicionar suporte a navegação por teclado (Enter/Space para selecionar)
- [ ] 4.7 Adicionar atributos de acessibilidade (role, aria-label)
- [ ] 4.8 Exportar componente no index do módulo

## Sequenciamento

- **Bloqueado por**: 3.0 (VehicleCheckInPage)
- **Desbloqueia**: 5.0 (CategorySelector)
- **Paralelizável**: Não — depende da estrutura da página

## Detalhes de Implementação

### Estrutura do Componente

```typescript
// frontend/src/modules/stock/components/OriginSelector.tsx
import { Card, CardContent } from '@/components/ui/card';
import { Factory, Car, ArrowLeftRight, Users } from 'lucide-react';
import { cn } from '@/lib/utils';
import { CheckInSource } from '../types';

interface OriginOption {
  value: CheckInSource;
  title: string;
  description: string;
  icon: React.ComponentType<{ className?: string }>;
}

const ORIGIN_OPTIONS: OriginOption[] = [
  {
    value: CheckInSource.Manufacturer,
    title: 'Montadora',
    description: 'Veículo vindo direto da fábrica',
    icon: Factory,
  },
  {
    value: CheckInSource.CustomerUsedPurchase,
    title: 'Compra Cliente/Seminovo',
    description: 'Veículo adquirido de cliente',
    icon: Car,
  },
  {
    value: CheckInSource.StoreTransfer,
    title: 'Transferência',
    description: 'Vindo de outra loja da rede',
    icon: ArrowLeftRight,
  },
  {
    value: CheckInSource.InternalFleet,
    title: 'Frota Interna',
    description: 'Retorno de veículo de demonstração',
    icon: Users,
  },
];

interface OriginSelectorProps {
  onSelect: (origin: CheckInSource) => void;
  selected?: CheckInSource;
}

export function OriginSelector({ onSelect, selected }: OriginSelectorProps) {
  return (
    <div 
      className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4"
      role="radiogroup"
      aria-label="Selecione a origem do veículo"
    >
      {ORIGIN_OPTIONS.map((option) => {
        const Icon = option.icon;
        const isSelected = selected === option.value;
        
        return (
          <Card
            key={option.value}
            className={cn(
              'cursor-pointer transition-all hover:border-primary hover:shadow-md',
              isSelected && 'border-primary bg-primary/5 ring-2 ring-primary'
            )}
            onClick={() => onSelect(option.value)}
            onKeyDown={(e) => {
              if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault();
                onSelect(option.value);
              }
            }}
            tabIndex={0}
            role="radio"
            aria-checked={isSelected}
            aria-label={option.title}
          >
            <CardContent className="flex flex-col items-center p-6 text-center">
              <Icon className="h-10 w-10 mb-3 text-muted-foreground" />
              <h3 className="font-semibold mb-1">{option.title}</h3>
              <p className="text-sm text-muted-foreground">
                {option.description}
              </p>
            </CardContent>
          </Card>
        );
      })}
    </div>
  );
}
```

### Mapeamento Origem → Categorias Permitidas

Esta informação será usada pelo CategorySelector:

```typescript
export const ORIGIN_CATEGORY_MAP: Record<CheckInSource, VehicleCategory[]> = {
  [CheckInSource.Manufacturer]: [VehicleCategory.New],
  [CheckInSource.CustomerUsedPurchase]: [VehicleCategory.Used],
  [CheckInSource.StoreTransfer]: [VehicleCategory.New, VehicleCategory.Used, VehicleCategory.Demonstration],
  [CheckInSource.InternalFleet]: [VehicleCategory.Demonstration],
};
```

### Estilos e Estados

| Estado | Visual |
|--------|--------|
| Default | Borda cinza, fundo branco |
| Hover | Borda primária, sombra |
| Focus | Ring primário, outline |
| Selected | Borda primária, fundo primário/5, ring |

## Critérios de Sucesso

- [ ] 4 cards renderizam corretamente
- [ ] Click em card dispara `onSelect` com valor correto
- [ ] Card selecionado tem destaque visual claro
- [ ] Navegação por Tab funciona entre cards
- [ ] Enter/Space seleciona o card focado
- [ ] Layout responsivo funciona em diferentes tamanhos de tela
- [ ] Screen readers anunciam corretamente as opções
