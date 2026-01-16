---
status: completed # Opções: pending, in-progress, completed, excluded
parallelizable: true
blocked_by: ["1.0","3.0"]
---

<task_context>
<domain>frontend/stock/services</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>medium</complexity>
<dependencies>external_apis|http_server</dependencies>
<unblocks>"5.0,6.0,7.0,8.0,9.0,10.0,11.0,12.0"</unblocks>
</task_context>

# Tarefa 4.0: Implementar camada de serviços HTTP do Stock (axios)

## Visão Geral
Implementar os serviços HTTP do módulo Stock (veículos, reservas e test-drives) usando o cliente axios central existente (`frontend/src/lib/api.ts`), garantindo paginação `_page/_size`, filtros e coerência com `/api/v1`.

## Requisitos
- Implementar métodos conforme Tech Spec:
  - Vehicles: list, getById, getHistory, changeStatus, checkIn, checkOut, startTestDrive.
  - Reservations: create, cancel, extend.
  - Test-drives: complete.
- Params de listagem devem usar `_page`/`_size` e suportar filtros (`q`, `status`, `category`).
- Tratar erros `ProblemDetails` (extrair `title/detail/status`) para a camada de UI/toast.

## Subtarefas
- [ ] 4.1 Criar `vehicleService` (ex.: `frontend/src/modules/stock/services/vehicleService.ts`).
- [ ] 4.2 Criar `reservationService`.
- [ ] 4.3 Criar `testDriveService`.
- [ ] 4.4 Padronizar serialização de query params (incluindo compatibilidade opcional com `page/pageSize` se necessário).

## Sequenciamento
- Bloqueado por: 1.0, 3.0
- Desbloqueia: 5.0 e telas 6.0–11.0
- Paralelizável: Sim (em paralelo com 2.0)

## Detalhes de Implementação
- Tech Spec: “Interfaces Principais”, “Endpoints de API”, “Erros: ProblemDetails”.
- Regra REST: paginação `_page`/`_size` e ProblemDetails em `rules/restful.md` (consumo/expectativa).

## Critérios de Sucesso
- Serviços chamam endpoints corretos sob `/api/v1`.
- Filtros/paginação funcionam e não quebram quando params opcionais estão ausentes.
- Erros do backend são convertidos em objeto consumível pela UI.

## Checklist de conclusão

- [x] 4.0 Implementar camada de serviços HTTP do Stock (axios) ✅ CONCLUÍDA
  - [x] 4.1 Implementação completada
  - [x] 4.2 Definição da tarefa, PRD e tech spec validados
  - [x] 4.3 Análise de regras e conformidade verificadas
  - [x] 4.4 Revisão de código completada
  - [x] 4.5 Pronto para deploy
