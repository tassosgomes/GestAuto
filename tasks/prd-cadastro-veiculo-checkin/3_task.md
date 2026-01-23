---
status: pending
parallelizable: true
blocked_by: []
---

<task_context>
<domain>frontend/stock</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>medium</complexity>
<dependencies>react, shadcn-ui</dependencies>
<unblocks>"4.0", "5.0", "6.0"</unblocks>
</task_context>

# Tarefa 3.0: Criar Página VehicleCheckInPage (Estrutura Base)

## Visão Geral

Criar a estrutura base da nova página de cadastro de veículo com check-in unificado. Esta página será o container principal que orquestra os componentes de seleção de origem, categoria e formulário dinâmico. A página deve seguir o padrão de layout existente no módulo de estoque.

<requirements>
- Criar página em `frontend/src/modules/stock/pages/VehicleCheckInPage.tsx`
- Seguir padrão de layout existente (header, content area)
- Implementar estado para controlar fluxo: origem → categoria → formulário
- Utilizar componentes shadcn/ui existentes
- Preparar slots para componentes filhos (OriginSelector, CategorySelector, DynamicVehicleForm)
- Implementar feedback de loading e erro
</requirements>

## Subtarefas

- [ ] 3.1 Criar arquivo `VehicleCheckInPage.tsx` em `frontend/src/modules/stock/pages/`
- [ ] 3.2 Implementar estrutura de layout com header e área de conteúdo
- [ ] 3.3 Criar estado para controlar step atual (origin | category | form)
- [ ] 3.4 Criar estado para armazenar seleções (selectedOrigin, selectedCategory)
- [ ] 3.5 Implementar navegação entre steps (voltar/avançar)
- [ ] 3.6 Adicionar componente de loading skeleton
- [ ] 3.7 Adicionar tratamento de erro com alert/toast
- [ ] 3.8 Exportar página no index do módulo

## Sequenciamento

- **Bloqueado por**: Nenhum
- **Desbloqueia**: 4.0 (OriginSelector), 5.0 (CategorySelector), 6.0 (DynamicVehicleForm)
- **Paralelizável**: Sim — pode executar em paralelo com tarefas de backend

## Detalhes de Implementação

### Estrutura do Componente

```typescript
// frontend/src/modules/stock/pages/VehicleCheckInPage.tsx
import { useState } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { ArrowLeft } from 'lucide-react';

type Step = 'origin' | 'category' | 'form';

interface CheckInState {
  origin: CheckInSource | null;
  category: VehicleCategory | null;
}

export function VehicleCheckInPage() {
  const [step, setStep] = useState<Step>('origin');
  const [state, setState] = useState<CheckInState>({
    origin: null,
    category: null,
  });
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleOriginSelect = (origin: CheckInSource) => {
    setState(prev => ({ ...prev, origin }));
    setStep('category');
  };

  const handleCategorySelect = (category: VehicleCategory) => {
    setState(prev => ({ ...prev, category }));
    setStep('form');
  };

  const handleBack = () => {
    if (step === 'category') setStep('origin');
    if (step === 'form') setStep('category');
  };

  return (
    <div className="container mx-auto py-6 space-y-6">
      <div className="flex items-center gap-4">
        {step !== 'origin' && (
          <Button variant="ghost" size="icon" onClick={handleBack}>
            <ArrowLeft className="h-4 w-4" />
          </Button>
        )}
        <h1 className="text-2xl font-bold">Registrar Entrada de Veículo</h1>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>{getStepTitle(step)}</CardTitle>
        </CardHeader>
        <CardContent>
          {step === 'origin' && (
            <OriginSelector onSelect={handleOriginSelect} />
          )}
          {step === 'category' && (
            <CategorySelector 
              origin={state.origin!} 
              onSelect={handleCategorySelect} 
            />
          )}
          {step === 'form' && (
            <DynamicVehicleForm
              origin={state.origin!}
              category={state.category!}
              onSubmit={handleSubmit}
              isSubmitting={isSubmitting}
            />
          )}
        </CardContent>
      </Card>
    </div>
  );
}
```

### Estados do Fluxo

```
┌──────────┐    ┌────────────┐    ┌──────────┐
│  Origin  │───▶│  Category  │───▶│   Form   │
│ Selector │    │  Selector  │    │ (Submit) │
└──────────┘    └────────────┘    └──────────┘
     │                │                │
     └────────────────┴────────────────┘
                 Voltar
```

### Tipos Necessários

```typescript
// Já existentes no projeto ou a criar em types.ts
export enum CheckInSource {
  Manufacturer = 'Manufacturer',
  CustomerUsedPurchase = 'CustomerUsedPurchase',
  StoreTransfer = 'StoreTransfer',
  InternalFleet = 'InternalFleet',
}

export enum VehicleCategory {
  New = 'New',
  Used = 'Used',
  Demonstration = 'Demonstration',
}
```

## Critérios de Sucesso

- [ ] Página renderiza sem erros
- [ ] Navegação entre steps funciona corretamente
- [ ] Botão voltar aparece apenas quando não está no primeiro step
- [ ] Estado é preservado ao navegar entre steps
- [ ] Layout responsivo funciona em mobile e desktop
- [ ] Componente exportado e acessível para rotas
