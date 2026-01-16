---
status: completed
parallelizable: true
blocked_by: ["1.0", "2.0", "3.0"]
---

<task_context>
<domain>engine/http-api</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>high</complexity>
<dependencies>http_server|database</dependencies>
<unblocks>"6.0, 7.0"</unblocks>
</task_context>

# Tarefa 5.0: APIs de veículos (consulta/cadastro/status)

## Visão Geral
Implementar os endpoints base de veículos (cadastro, detalhe, listagem paginada e alteração manual de status), respeitando regras de segurança e padrões REST.

## Requisitos
- Endpoints `api/v1/vehicles` (POST/GET list) e `api/v1/vehicles/{id}` (GET).
- Listagem paginada (`page`, `pageSize`) e filtros simples (`status`, `category`, `q`).
- Alteração manual de status via `PATCH /api/v1/vehicles/{id}/status` (somente `MANAGER`/`ADMIN`).
- Erros via `application/problem+json`.

## Subtarefas
- [x] 5.1 Criar DTOs de request/response (VehicleCreate, VehicleResponse, VehicleListItem)
- [x] 5.2 Implementar `CreateVehicleCommand` + handler
- [x] 5.3 Implementar `GetVehicleQuery` e `ListVehiclesQuery` + handlers
- [x] 5.4 Implementar `ChangeVehicleStatusCommand` + handler (com RBAC e validação de transição)
- [x] 5.5 Criar `VehiclesController` com rotas e policies
- [x] 5.6 (Testes) Unit tests dos handlers e validações principais

## Sequenciamento
- Bloqueado por: 1.0, 2.0, 3.0
- Desbloqueia: 6.0, 7.0
- Paralelizável: Sim

## Detalhes de Implementação
- Seguir padrão de controllers/handlers do serviço Comercial (CQRS sem MediatR).
- Validar que ações proibidas em status `vendido`/`baixado` retornam erro de negócio.

## Critérios de Sucesso
- Endpoints funcionam e respeitam RBAC.
- Listagem performa com paginação e índices básicos.
- Alteração manual de status registra responsável e gera evento `vehicle.status-changed` (via outbox).

## Checklist de conclusão

- [x] 5.0 APIs de veículos (consulta/cadastro/status) ✅ CONCLUÍDA
	- [x] 5.1 Implementação completada
	- [x] 5.2 Definição da tarefa, PRD e tech spec validados
	- [x] 5.3 Análise de regras e conformidade verificadas
	- [x] 5.4 Revisão de código completada
	- [x] 5.5 Pronto para deploy
