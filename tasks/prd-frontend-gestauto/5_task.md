---
status: completed
parallelizable: false
blocked_by: ["4.0"]
---

## markdown

## status: completed # Opções: pending, in-progress, completed, excluded

<task_context>
<domain>engine/ui/frontend</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>medium</complexity>
<dependencies>http_server</dependencies>
</task_context>

# Tarefa 5.0: Implementar RBAC (menus + guards) + páginas placeholder + testes

## Visão Geral

Implementar a lógica de autorização no frontend baseada nas roles (`roles` claim), garantindo:

- Menus visíveis por role (Comercial, Avaliações, Admin)
- Proteção de rotas (guard) contra acesso via URL
- Páginas placeholder para os módulos
- Testes unitários da lógica de RBAC

<requirements>
- Menus por role:
  - Comercial: `SALES_PERSON`, `SALES_MANAGER`, `MANAGER`, `ADMIN`
  - Avaliações: `VEHICLE_EVALUATOR`, `EVALUATION_MANAGER`, `MANAGER`, `VIEWER`, `ADMIN`
  - Admin: somente `ADMIN`
- Guard de rota deve bloquear acesso e mostrar “Acesso negado”
- UI inspirada em `model-ui/code.html` (sem inventar novos padrões)
</requirements>

## Subtarefas

- [x] 5.1 Implementar `getVisibleMenus(roles)` (ou equivalente) e componente de navegação
- [x] 5.2 Implementar guards de rotas por permissão
- [x] 5.3 Implementar páginas placeholder (Home, Comercial, Avaliações, Admin, Acesso negado)
- [x] 5.4 Criar testes unitários para RBAC (menus e guards)
- [x] 5.5 Documentar como validar manualmente com usuários do Keycloak (seller/evaluator/admin/viewer)

## Detalhes de Implementação

- Ver seções “RBAC helpers” e “Abordagem de Testes” em `tasks/prd-frontend-gestauto/techspec.md`.

## Critérios de Sucesso

- Usuários veem menus corretos:
  - `seller` → Comercial
  - `evaluator` → Avaliações
  - `admin` → Comercial + Avaliações + Admin
  - `viewer` → somente Avaliações
- Acesso direto por URL a rota não autorizada exibe “Acesso negado”.
- Testes unitários do RBAC passam.
