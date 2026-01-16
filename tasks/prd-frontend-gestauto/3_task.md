---
status: completed
parallelizable: true
blocked_by: []
---

## markdown

## status: completed # Opções: pending, in-progress, completed, excluded

<task_context>
<domain>engine/ui/frontend</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>medium</complexity>
<dependencies>http_server</dependencies>
<unblocks>"4.0"</unblocks>
</task_context>

# Tarefa 3.0: Scaffold do frontend + config runtime

## Visão Geral

Criar a base do projeto do frontend (SPA) dentro do monorepo, incluindo build, estrutura de pastas e um mecanismo de **configuração por ambiente em runtime** (base URL do Keycloak, realm, clientId, appBaseUrl) sem hardcode.

<requirements>
- Criar um novo pacote de frontend no repo (sugestão: `frontend/`)
- Config runtime com:
  - `keycloakBaseUrl`: `http://keycloak.tasso.local`
  - `keycloakRealm`: `gestauto-dev` | `gestauto-hml` | `gestauto`
  - `keycloakClientId`: `gestauto-frontend`
  - `appBaseUrl`: `http://gestauto.tasso.local`
- Preparar páginas placeholder e roteamento base (sem auth ainda)
</requirements>

## Subtarefas

- [x] 3.1 Criar estrutura do pacote `frontend/` (build dev/prod)
- [x] 3.2 Implementar leitura de config runtime (ex.: `window.__APP_CONFIG__` ou arquivo JSON servido)
- [x] 3.3 Criar roteamento base com páginas placeholder (Home, Comercial, Avaliações, Admin, Acesso negado)
- [x] 3.4 Documentar como rodar o frontend em modo dev

## Detalhes de Implementação

- Ver seção “Modelos de Dados / Config de runtime do frontend” em `tasks/prd-frontend-gestauto/techspec.md`.

## Critérios de Sucesso

- O frontend sobe localmente em modo dev e renderiza páginas placeholder.
- Config runtime é carregada sem rebuild (alteração de config não exige recompilar).
- Estrutura está pronta para integrar Keycloak na Tarefa 4.0.

## Checklist de conclusão

- [x] 3.0 Scaffold do frontend + config runtime ✅ CONCLUÍDA
  - [x] 3.1 Implementação completada
  - [x] 3.2 Definição da tarefa, PRD e tech spec validados
  - [x] 3.3 Análise de regras e conformidade verificadas
  - [x] 3.4 Revisão de código completada
  - [x] 3.5 Pronto para deploy
