---
status: completed
parallelizable: true
blocked_by: ["6.0", "7.0", "8.0"]
---

<task_context>
<domain>engine/history-audit</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>medium</complexity>
<dependencies>database|http_server</dependencies>
<unblocks>"10.0"</unblocks>
</task_context>

# Tarefa 9.0: Histórico (timeline) e auditoria

## Visão Geral
Implementar a consulta do histórico completo do veículo (timeline cronológica) combinando check-ins, check-outs, reservas, test-drives e mudanças de status, para atender auditoria e rastreabilidade.

## Requisitos
- `GET /api/v1/vehicles/{vehicleId}/history` (RF9.2).
- Deve incluir: quem fez, quando, o que mudou (RF9.1).
- Operações críticas sem usuário autenticado devem ser bloqueadas (RF9.3) — garantir que o histórico reflita sempre um responsável.

## Subtarefas
- [x] 9.1 Definir modelo de resposta da timeline (tipo do evento, data/hora, userId, detalhes)
- [x] 9.2 Implementar query `GetVehicleHistoryQuery` + handler (ordenação por data)
- [x] 9.3 Implementar endpoint no controller
- [x] 9.4 (Opcional) Criar tabela `audit_entries` se necessário para “status-changed” manual
- [x] 9.5 (Testes) Unit/integration: timeline retorna eventos na ordem correta

## Sequenciamento
- Bloqueado por: 6.0, 7.0, 8.0
- Desbloqueia: 10.0
- Paralelizável: Sim

## Detalhes de Implementação
- Se mudanças de status forem sempre registradas por histórico próprio, a timeline pode ser derivada sem tabela de auditoria.

## Critérios de Sucesso
- Timeline mostra operações relevantes em ordem cronológica.
- Cada item contém responsável e dados mínimos para auditoria.

## Checklist de conclusão

- [x] 9.0 Histórico (timeline) e auditoria ✅ CONCLUÍDA
	- [x] 9.1 Implementação completada
	- [x] 9.2 Definição da tarefa, PRD e tech spec validados
	- [x] 9.3 Análise de regras e conformidade verificadas
	- [x] 9.4 Revisão de código completada
	- [x] 9.5 Pronto para deploy
