---
status: pending
parallelizable: false
blocked_by: []
---

<task_context>
<domain>frontend/infra</domain>
<type>configuration</type>
<scope>configuration</scope>
<complexity>low</complexity>
<dependencies>npm</dependencies>
<unblocks>2.0</unblocks>
</task_context>

# Tarefa 1.0: Setup de Infraestrutura e Dependências

## Visão Geral
Instalação e configuração das bibliotecas necessárias para suportar o desenvolvimento do módulo comercial, incluindo gerenciamento de estado assíncrono, formulários e validação.

## Requisitos
- Instalar `axios` para requisições HTTP.
- Instalar `@tanstack/react-query` para gerenciamento de estado de servidor.
- Instalar `react-hook-form` para gerenciamento de formulários.
- Instalar `zod` e `@hookform/resolvers` para validação.
- Instalar `date-fns` para manipulação de datas.
- Configurar o `QueryClientProvider` no `App.tsx`.
- Configurar instância base do `axios` em `src/lib/api.ts` com interceptors para token.

## Subtarefas
- [x] 1.1 Instalar pacotes npm (`axios`, `@tanstack/react-query`, `react-hook-form`, `zod`, `@hookform/resolvers`, `date-fns`).
- [x] 1.2 Criar `src/lib/api.ts` com instância do Axios e interceptor de Authorization (usando token do Keycloak).
- [x] 1.3 Configurar `QueryClient` e envolver a aplicação com `QueryClientProvider` em `src/App.tsx` ou `src/main.tsx`.

## Detalhes de Implementação
- O interceptor do Axios deve ler o token do `AuthContext` ou `keycloak-js` instance.
- Configurar `QueryClient` com defaults sensatos (ex: `refetchOnWindowFocus: false`).

## Critérios de Sucesso
- Todas as dependências instaladas e listadas no `package.json`.
- Aplicação roda sem erros de console.
- `api.get` envia header `Authorization: Bearer ...` corretamente (verificável via network tab ou teste simples).

- [x] 1.0 Setup de Infraestrutura e Dependências ✅ CONCLUÍDA
  - [x] 1.1 Implementação completada
  - [x] 1.2 Definição da tarefa, PRD e tech spec validados
  - [x] 1.3 Análise de regras e conformidade verificadas
  - [x] 1.4 Revisão de código completada
  - [x] 1.5 Pronto para deploy
