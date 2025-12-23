---
status: completed
parallelizable: false
blocked_by: ["3.0"]
---

<task_context>
<domain>frontend/commercial</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>medium</complexity>
<dependencies>shadcn-ui</dependencies>
<unblocks>5.0</unblocks>
</task_context>

# Tarefa 4.0: Funcionalidade: Gestão de Leads (Listagem e Cadastro)

## Visão Geral
Implementação das telas de listagem de leads (DataGrid) e modal de cadastro de novos leads.

## Requisitos
- Tela `/commercial/leads` com tabela responsiva.
- Colunas: Nome, Status, Score, Interesse, Data.
- Filtros básicos (Status, Texto).
- Botão "Novo Lead" abrindo Modal.
- Formulário de cadastro com validação (Zod).

- [x] 4.0 Funcionalidade: Gestão de Leads (Listagem e Cadastro) ✅ CONCLUÍDA
  - [x] 4.1 Implementação completada
  - [x] 4.2 Definição da tarefa, PRD e tech spec validados
  - [x] 4.3 Análise de regras e conformidade verificadas
  - [x] 4.4 Revisão de código completada
  - [x] 4.5 Pronto para deploy

## Subtarefas
- [x] 4.1 Criar hook `useLeads` (React Query) para buscar lista.
- [x] 4.2 Implementar `LeadListPage` usando componente `Table` (Shadcn).
- [x] 4.3 Implementar `CreateLeadModal` com `react-hook-form` e `zod`.
- [x] 4.4 Integrar mutação `useCreateLead` para salvar e atualizar a lista.

## Detalhes de Implementação
- Validar campos obrigatórios: Nome, Telefone, Email, Origem.
- Feedback de sucesso/erro via Toast.

## Critérios de Sucesso
- Usuário consegue visualizar lista de leads paginada (ou scroll infinito).
- Usuário consegue cadastrar novo lead e vê-lo na lista imediatamente.
