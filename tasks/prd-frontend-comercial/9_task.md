---
status: done
parallelizable: false
blocked_by: ["4.0", "6.0", "8.0"]
---

<task_context>
<domain>frontend/commercial</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>medium</complexity>
<dependencies>charts</dependencies>
<unblocks>[]</unblocks>
</task_context>

# Tarefa 9.0: Funcionalidade: Dashboard e Telas Gerenciais

## Visão Geral
Implementação do Dashboard principal e telas de aprovação para gerentes.

## Requisitos
- Dashboard com KPIs reais.
- Tela de Aprovação de Descontos.

## Subtarefas
- [x] 9.1 Implementar Widgets do Dashboard consumindo APIs.
- [x] 9.2 Implementar Tela de Aprovação de Descontos (Lista + Ação Aprovar/Rejeitar).
- [x] 9.3 Refinar permissões (apenas Gerente vê aprovações).

## Critérios de Sucesso
- Dashboard exibe números reais do usuário logado.
- Gerente consegue aprovar propostas pendentes.
