---
status: pending
parallelizable: true
blocked_by: ["2.0"]
---

<task_context>
<domain>frontend/commercial</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>medium</complexity>
<dependencies>swagger</dependencies>
<unblocks>4.0, 5.0, 6.0</unblocks>
</task_context>

# Tarefa 3.0: Implementação de Serviços e Tipos (API)

## Visão Geral
Mapeamento dos endpoints da API REST (`commercial`) para funções TypeScript e definição de interfaces (DTOs) baseadas no Swagger.

## Requisitos
- Criar interfaces TypeScript para `Lead`, `Proposal`, `TestDrive` e seus DTOs de criação/atualização.
- Implementar serviços (`leadService`, `proposalService`) usando a instância do `axios`.

## Subtarefas
- [x] 3.1 Criar `src/modules/commercial/types/index.ts` com interfaces extraídas do Swagger.
- [x] 3.2 Criar `src/modules/commercial/services/leadService.ts` (getAll, getById, create, qualify).
- [x] 3.3 Criar `src/modules/commercial/services/proposalService.ts` (getAll, getById, create, update).
- [x] 3.4 Criar `src/modules/commercial/services/testDriveService.ts`.

## Detalhes de Implementação
- Usar tipos estritos.
- Mapear Enums (Status, Score) corretamente.

## Critérios de Sucesso
- Funções de serviço tipadas corretamente (entrada e saída).
- Teste unitário simples ou verificação manual de chamada.
