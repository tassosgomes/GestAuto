---
status: pending # Opções: pending, in-progress, completed, excluded
parallelizable: false
blocked_by: ["2.0","4.0"]
---

<task_context>
<domain>frontend/stock/hooks</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>medium</complexity>
<dependencies>external_apis</dependencies>
<unblocks>"6.0,7.0,8.0,9.0,10.0,11.0,12.0"</unblocks>
</task_context>

# Tarefa 5.0: Implementar hooks TanStack Query (queries/mutations)

## Visão Geral
Criar hooks do módulo Stock usando TanStack Query para leitura e mutações, com `queryKey` padronizado e invalidations consistentes (listas/detalhes/histórico).

## Requisitos
- Queries:
  - `useVehiclesList(params)` → `['stock-vehicles', params]`
  - `useVehicle(id)` → `['stock-vehicle', id]`
  - `useVehicleHistory(id)` → `['stock-vehicle-history', id]`
- Mutations (ex.: reserva, cancelamento, status, check-in/out, test-drive) devem invalidar as chaves corretas.
- Estados de loading/error expostos de forma consistente para as páginas.

## Subtarefas
- [ ] 5.1 Implementar hooks de query para veículos/detalhe/histórico.
- [ ] 5.2 Implementar hooks de mutation para ações (reservas, status, movimentos, test-drive).
- [ ] 5.3 Definir estratégia de invalidation após mutações (lista + detalhe + histórico do veículo afetado).

## Sequenciamento
- Bloqueado por: 2.0, 4.0
- Desbloqueia: 6.0–11.0 e 12.0
- Paralelizável: Não (centraliza padrão de cache/invalidation)

## Detalhes de Implementação
- Tech Spec: seção “Frontend (hooks TanStack Query)” e “Sequenciamento de Desenvolvimento”.

## Critérios de Sucesso
- Hooks reutilizáveis e usados pelas páginas Stock.
- Após mutações, dados exibidos são atualizados sem refresh manual.
