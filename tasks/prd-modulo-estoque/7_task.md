---
status: completed
parallelizable: true
blocked_by: ["5.0", "3.0"]
---

<task_context>
<domain>engine/reservations</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>high</complexity>
<dependencies>http_server|database|temporal</dependencies>
<unblocks>"8.0, 9.0, 10.0"</unblocks>
</task_context>

# Tarefa 7.0: Reservas (criar/cancelar/prorrogar/expirar)

## Visão Geral
Implementar reservas de veículos com exclusividade comercial, regras de expiração e RBAC conforme PRD, incluindo rotina automática de expiração.

## Requisitos
- Tipos: `padrao`, `entrada_paga`, `aguardando_banco` (RF6.7).
- Expiração: 48h para `padrao`; sem expiração para `entrada_paga`; prazo manual do vendedor para `aguardando_banco` (RF6.8–RF6.10).
- Cancelamento: vendedor só a própria; gerente cancela de outro (RF6.6).
- Prorrogação: gerente comercial (RF6.11).
- Expiração automática: registrar histórico + tornar veículo disponível (RF6.12).
- Garantia técnica: 1 reserva ativa por veículo (constraint + tratamento de concorrência).

## Subtarefas
- [x] 7.1 Criar DTOs/commands: create/cancel/extend
- [x] 7.2 Implementar `CreateReservationCommand` + handler (setar `expiresAt` e validar `bankDeadlineAt`)
- [x] 7.3 Implementar `CancelReservationCommand` + handler (enforce "própria reserva")
- [x] 7.4 Implementar `ExtendReservationCommand` + handler (somente `SALES_MANAGER`/`MANAGER`/`ADMIN`)
- [x] 7.5 Implementar job/background service de expiração automática
- [x] 7.6 (Testes) Unit tests: regras de expiração; RBAC no handler; concorrência (violação de constraint)

## Sequenciamento
- Bloqueado por: 5.0, 3.0
- Desbloqueia: 8.0, 9.0, 10.0
- Paralelizável: Sim (em paralelo com 6.0)

## Detalhes de Implementação
- Expiração automática deve publicar `reservation.expired` e `vehicle.status-changed`.
- Em caso de disputa de reserva, retornar erro de negócio (ideal 409) via ProblemDetails.

## Critérios de Sucesso
- Reserva ativa bloqueia nova reserva por outro vendedor.
- Expiração automática acontece e libera veículo.
- Cancelamento/prorrogação respeitam RBAC e registram histórico/eventos.

## Checklist de conclusão

- [x] 7.0 Reservas (criar/cancelar/prorrogar/expirar) ✅ CONCLUÍDA
	- [x] 7.1 Implementação completada
	- [x] 7.2 Definição da tarefa, PRD e tech spec validados
	- [x] 7.3 Análise de regras e conformidade verificadas
	- [x] 7.4 Revisão de código completada
	- [x] 7.5 Pronto para deploy
