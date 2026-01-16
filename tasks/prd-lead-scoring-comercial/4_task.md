---
status: completed
parallelizable: true
blocked_by: ["1.0"]
---

<task_context>
<domain>frontend/commercial/pages</domain>
<type>integration</type>
<scope>core_feature</scope>
<complexity>low</complexity>
<dependencies>none</dependencies>
<unblocks>["5.0"]</unblocks>
</task_context>

# Tarefa 4.0: Integrar na Listagem de Leads

## Visão Geral
Atualizar a listagem de leads para exibir o badge de score, permitindo identificação rápida de oportunidades.

## Requisitos
- Exibir `LeadScoreBadge` (versão compacta/sem SLA extenso) nos cards da lista de leads.

## Subtarefas
- [x] 4.1 Localizar componente de card/item da lista em `LeadListPage` (ou componente filho). ✅ CONCLUÍDA
- [x] 4.2 Inserir `LeadScoreBadge` no layout do card. ✅ CONCLUÍDA
- [x] 4.3 Ajustar estilos para garantir boa visualização na lista. ✅ CONCLUÍDA

## Status da Tarefa
- [x] 4.0 Integrar na Listagem de Leads ✅ CONCLUÍDA
  - [x] 4.1 Implementação completada
  - [x] 4.2 Definição da tarefa, PRD e tech spec validados
  - [x] 4.3 Análise de regras e conformidade verificadas
  - [x] 4.4 Revisão de código completada
  - [x] 4.5 Pronto para deploy

## Sequenciamento
- Bloqueado por: 1.0
- Desbloqueia: 5.0
- Paralelizável: Sim (após 1.0)

## Detalhes de Implementação
- Verificar se o DTO de listagem já traz o `score`. Se não, pode ser necessário ajuste no backend (fora do escopo desta task, mas deve ser reportado). Assumindo que sim conforme PRD.

## Critérios de Sucesso
- Leads na lista exibem seus respectivos badges de score.
