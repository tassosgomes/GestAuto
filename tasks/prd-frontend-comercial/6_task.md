---
status: completed
parallelizable: true
blocked_by: ["3.0"]
---

<task_context>
<domain>frontend/commercial</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>high</complexity>
<dependencies>shadcn-ui</dependencies>
<unblocks>7.0</unblocks>
</task_context>

# Tarefa 6.0: Funcionalidade: Editor de Propostas (Básico)

## Visão Geral
Implementação da estrutura do editor de propostas e seções básicas (Veículo e Pagamento).

## Requisitos
- Tela `/commercial/proposals/new` (ou via Lead).
- Seleção de Veículo.
- Definição de condições de pagamento.
- Resumo lateral com totais.

## Subtarefas
- [x] 6.1 Criar layout do Editor de Proposta (Grid com Sidebar).
- [x] 6.2 Implementar Seção de Seleção de Veículo.
- [x] 6.3 Implementar Seção de Pagamento (Entrada, Parcelas).
- [x] 6.4 Implementar Resumo de Valores (Cálculo em tempo real).
- [x] 6.5 Salvar Rascunho (`useCreateProposal`).

## Detalhes de Implementação
- Usar `react-hook-form` com `useWatch` para cálculos em tempo real no resumo.

## Critérios de Sucesso
- Usuário consegue criar uma proposta básica selecionando veículo e forma de pagamento.
- Proposta é salva no backend.
