---
status: completed # Opções: pending, in-progress, completed, excluded
parallelizable: true
blocked_by: ["2.0","5.0","3.0"]
---

<task_context>
<domain>frontend/stock/pages-reservations</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>high</complexity>
<dependencies>external_apis</dependencies>
<unblocks>"12.0"</unblocks>
</task_context>

# Tarefa 8.0: Implementar Gestão de Reservas (listagem + ações)

## Visão Geral
Implementar `/stock/reservations` (listagem) e fluxos de criação/cancelamento/prorrogação de reserva, incluindo a regra de UX do “prazo do banco” (date-only → 18:00 local → UTC).

## Requisitos
- Listagem de reservas (ativas e recentes) com campos do PRD.
- Ações RBAC:
  - Cancelar própria reserva (vendedor)
  - Cancelar reserva de outro vendedor (gerente)
  - Prorrogar reserva (gerente)
- Criação de reserva no contexto do veículo (via modal/drawer a partir de listagens/detalhe).
- Campo “prazo do banco” deve ser input de data (sem hora) e convertido pelo helper (Tarefa 3.0).

## Subtarefas
- [x] 8.1 Implementar página `/stock/reservations` com tabela, filtros simples e empty-state.
- [x] 8.2 Implementar modal/drawer de criação de reserva a partir do veículo (reuso entre páginas).
- [x] 8.3 Implementar ações de cancelar/prorrogar com confirmação e feedback.
- [x] 8.4 Garantir ocultação de botões por RBAC (não renderizar quando não autorizado).

## Sequenciamento
- Bloqueado por: 2.0, 3.0, 5.0
- Desbloqueia: 12.0
- Paralelizável: Sim

## Detalhes de Implementação
- PRD: “Gestão de Reservas” e regra “Prazo do banco (date-only)”.
- Tech Spec: “Reservations” e convenção de datas.

## Critérios de Sucesso
- Usuários com roles adequadas conseguem criar/cancelar/prorrogar conforme regras.
- Payload enviado para `bankDeadlineAtUtc` está em UTC e representa 18:00 do dia escolhido no fuso local.

## Checklist de conclusão

- [x] 8.0 Implementar Gestão de Reservas (listagem + ações) ✅ CONCLUÍDA
  - [x] 8.1 Implementação completada
  - [x] 8.2 Definição da tarefa, PRD e tech spec validados
  - [x] 8.3 Análise de regras e conformidade verificadas
  - [x] 8.4 Revisão de código completada
  - [x] 8.5 Pronto para deploy
