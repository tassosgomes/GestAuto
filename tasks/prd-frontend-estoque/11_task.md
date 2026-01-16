---
status: pending # Opções: pending, in-progress, completed, excluded
parallelizable: true
blocked_by: ["2.0","5.0"]
---

<task_context>
<domain>frontend/stock/pages-operational</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>medium</complexity>
<dependencies>external_apis</dependencies>
<unblocks>"12.0"</unblocks>
</task_context>

# Tarefa 11.0: Implementar telas Preparação, Financeiro e Baixas/Exceções

## Visão Geral
Implementar páginas operacionais do MVP:
- `/stock/preparation`
- `/stock/finance`
- `/stock/write-offs`

Com foco em listagem por status + ações permitidas (ex.: change-status e/ou check-out para baixas).

## Requisitos
- Cada tela deve ser uma listagem filtrada (reuso de componentes de tabela/filtros quando possível).
- Ações sensíveis e visibilidade dessas telas restritas a: `STOCK_MANAGER`, `MANAGER`, `ADMIN`.
- Preparação: permitir alterar status quando aplicável.
- Financeiro: listagem e ações limitadas por RBAC.
- Baixas/Exceções: registrar baixa via check-out com motivo apropriado (quando permitido).

## Subtarefas
- [ ] 11.1 Implementar `/stock/preparation` (filtro status “em preparação” e ação change-status).
- [ ] 11.2 Implementar `/stock/finance` (filtros de status relevantes).
- [ ] 11.3 Implementar `/stock/write-offs` (listagem e ação de check-out para baixa).
- [ ] 11.4 Validar ocultação de menu/rotas/ações conforme RBAC.

## Sequenciamento
- Bloqueado por: 2.0, 5.0
- Desbloqueia: 12.0
- Paralelizável: Sim

## Detalhes de Implementação
- PRD: seções “Preparação/Oficina”, “Financeiro/Vendas”, “Baixas/Exceções”.
- Tech Spec: restrição de ações sensíveis por role.

## Critérios de Sucesso
- Telas renderizam listas filtradas e respeitam RBAC por ocultação.
- Ações (quando permitidas) atualizam os dados sem refresh manual.
