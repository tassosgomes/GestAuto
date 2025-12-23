---
status: pending
parallelizable: false
blocked_by: ["4.0"]
---

<task_context>
<domain>frontend/commercial</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>high</complexity>
<dependencies>shadcn-ui</dependencies>
<unblocks>6.0</unblocks>
</task_context>

# Tarefa 5.0: Funcionalidade: Detalhes do Lead (Abas e Qualificação)

## Visão Geral
Implementação da tela de detalhes do lead, incluindo visão 360º, timeline de interações e formulário de qualificação.

## Requisitos
- Tela `/commercial/leads/:id`.
- Cabeçalho com informações principais e ações.
- Abas: Visão Geral, Timeline, Propostas.
- Formulário de Qualificação (Lead Scoring) funcional.
- Registro de Interações (Timeline).

## Subtarefas
- [x] 5.1 Criar hook `useLead` (getById).
- [x] 5.2 Implementar layout da página de detalhes (Header + Tabs).
- [x] 5.3 Implementar Aba Visão Geral com formulário de Qualificação (`useQualifyLead`).
- [x] 5.4 Implementar Aba Timeline com lista de interações e form de nova interação.

## Detalhes de Implementação
- Atualização otimista ou refetch após qualificação para atualizar o Score no cabeçalho.

## Critérios de Sucesso
- Usuário consegue acessar detalhes de um lead.
- Usuário consegue qualificar o lead e ver o Score mudar.
- Usuário consegue registrar uma interação (ex: "Liguei para o cliente").
