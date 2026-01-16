---
status: completed
parallelizable: true
blocked_by: ["5.0"]
---

<task_context>
<domain>engine/checkin-checkout</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>medium</complexity>
<dependencies>http_server|database</dependencies>
<unblocks>"8.0, 9.0, 10.0"</unblocks>
</task_context>

# Tarefa 6.0: Fluxos de check-in/check-out

## Visão Geral
Implementar os fluxos de entrada (check-in) e saída (check-out) com histórico e atualização coerente de status, incluindo validações por categoria e motivos mínimos.

## Requisitos
- `POST /api/v1/vehicles/{vehicleId}/check-ins` (RF4.1–RF4.4).
- `POST /api/v1/vehicles/{vehicleId}/check-outs` (RF5.1–RF5.4).
- Check-in deve aplicar obrigatoriedade por categoria (RF1.5–RF1.8).
- Check-out por `venda` => status `vendido`; por `baixa_sinistro_perda_total` => `baixado`.
- Registrar responsável e timestamps.
- Publicar eventos de negócio (RF9.4, RF9.7).

## Subtarefas
- [x] 6.1 Criar DTOs para CheckIn/CheckOut
- [x] 6.2 Implementar `CreateCheckInCommand` + handler com validações por categoria
- [x] 6.3 Implementar `CreateCheckOutCommand` + handler com transições e motivos
- [x] 6.4 Ajustar status e persistir `CheckInRecord`/`CheckOutRecord`
- [x] 6.5 (Testes) Unit tests: check-in por categoria; check-out venda/baixa; bloqueios por status

## Sequenciamento
- Bloqueado por: 5.0
- Desbloqueia: 8.0, 9.0, 10.0
- Paralelizável: Sim (em paralelo com 7.0 após 5.0)

## Detalhes de Implementação
- Check-in deve registrar `CurrentOwnerUserId` (RF4.3).
- Garantir que check-out/test-drive não possam ocorrer em `vendido`/`baixado`.

## Critérios de Sucesso
- Check-ins/outs persistem histórico corretamente.
- Status do veículo fica consistente após cada operação.
- Eventos correspondentes entram no outbox e são publicados.

## Checklist de conclusão

- [x] 6.0 Fluxos de check-in/check-out ✅ CONCLUÍDA
	- [x] 6.1 Implementação completada
	- [x] 6.2 Definição da tarefa, PRD e tech spec validados
	- [x] 6.3 Análise de regras e conformidade verificadas
	- [x] 6.4 Revisão de código completada
	- [x] 6.5 Pronto para deploy
