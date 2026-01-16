---
status: completed # Opções: pending, in-progress, completed, excluded
parallelizable: true
blocked_by: ["1.0"]
---

<task_context>
<domain>frontend/stock/routing-rbac</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>medium</complexity>
<dependencies>configuration|http_server</dependencies>
<unblocks>"5.0,6.0,7.0,8.0,9.0,10.0,11.0"</unblocks>
</task_context>

# Tarefa 2.0: Criar infraestrutura do módulo Stock (rotas, menu, RBAC)

## Visão Geral
Criar o esqueleto do módulo `stock` no frontend (estrutura de pastas, rotas em `useRoutes`, layouts e placeholders de páginas) e integrar na navegação/Sidebar com RBAC por ocultação.

## Requisitos
- Criar rotas base `/stock` e sub-rotas do MVP.
- Integrar no `App.tsx` (roteamento via `useRoutes`) e aplicar guardas já existentes (`RequireAuth`/`RequireMenuAccess` quando aplicável).
- Adicionar item de menu “Estoque” visível para roles: `STOCK_PERSON`, `STOCK_MANAGER`, `SALES_PERSON`, `SALES_MANAGER`, `MANAGER`, `ADMIN`.
- Garantir ocultação: usuários sem role não veem menu nem páginas (redirect/negação conforme padrão atual).
- Adicionar as novas roles ao tipo/enum de roles do frontend, seguindo SCREAMING_SNAKE_CASE.

## Subtarefas
- [x] 2.1 Criar pasta `frontend/src/modules/stock` com `routes.tsx` (ou padrão equivalente do repo) e páginas placeholder.
- [x] 2.2 Registrar rotas no roteamento global (`frontend/src/App.tsx`).
- [x] 2.3 Adicionar nav item em `frontend/src/config/navigation.tsx` com RBAC.
- [x] 2.4 Atualizar RBAC/roles do frontend para incluir `STOCK_PERSON` e `STOCK_MANAGER`.
- [x] 2.5 Garantir que a Sidebar oculta corretamente o item quando não autorizado.

## Sequenciamento
- Bloqueado por: 1.0
- Desbloqueia: 5.0–11.0
- Paralelizável: Sim (pode ocorrer em paralelo com 3.0/4.0)

## Detalhes de Implementação
- Tech Spec: “Visão Geral dos Componentes” (App.tsx, navigation, Sidebar, RBAC).
- Regra: roles devem seguir `rules/ROLES_NAMING_CONVENTION.md`.

## Critérios de Sucesso
- Rotas `/stock/*` navegáveis (mesmo que placeholders) e protegidas conforme RBAC.
- Menu “Estoque” aparece apenas para roles autorizadas.
- Build/testes existentes do frontend continuam passando (ou são ajustados na tarefa 12.0).

## Checklist de conclusão

- [x] 2.0 Criar infraestrutura do módulo Stock (rotas, menu, RBAC) ✅ CONCLUÍDA
	- [x] 2.1 Implementação completada
	- [x] 2.2 Definição da tarefa, PRD e tech spec validados
	- [x] 2.3 Análise de regras e conformidade verificadas
	- [x] 2.4 Revisão de código completada
	- [x] 2.5 Pronto para deploy
