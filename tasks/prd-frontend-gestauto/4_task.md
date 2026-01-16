---
status: completed
parallelizable: false
blocked_by: ["1.0", "3.0"]
---

## markdown

## status: completed # Opções: pending, in-progress, completed, excluded

<task_context>
<domain>engine/ui/frontend</domain>
<type>integration</type>
<scope>core_feature</scope>
<complexity>high</complexity>
<dependencies>external_apis</dependencies>
<unblocks>"5.0"</unblocks>
</task_context>

# Tarefa 4.0: Implementar login/logout via Keycloak (PKCE)

## Visão Geral

Integrar o frontend ao Keycloak usando Authorization Code + PKCE, incluindo fluxo de login, logout e controle de sessão no browser, de forma segura (sem logar/persistir tokens desnecessariamente).

<requirements>
- Keycloak base URL: `http://keycloak.tasso.local`
- Client: `gestauto-frontend`
- Redirect: `http://gestauto.tasso.local/*`
- Roles devem ser lidas da claim `roles`
- Não persistir tokens além do necessário (preferência: memória)
</requirements>

## Subtarefas

- [x] 4.1 Adicionar biblioteca de OIDC/Keycloak (preferência: `keycloak-js`) e encapsular em `AuthService`
- [x] 4.2 Implementar `login()` e `logout()` integrados ao Keycloak
- [x] 4.3 Implementar inicialização `init()` (restauração/checagem de sessão)
- [x] 4.4 Extrair `roles` do token e expor via `UserSession`
- [x] 4.5 Tratar token expirado/ausente redirecionando para login

## Detalhes de Implementação

- Ver “Pontos de Integração / Keycloak (OIDC)” e “Decisões Principais” em `tasks/prd-frontend-gestauto/techspec.md`.
- Validar com usuários de teste do Keycloak provisionados pelos scripts em `scripts/keycloak/`.

## Critérios de Sucesso

- Usuário consegue logar e deslogar no Keycloak.
- Após login, a sessão do frontend identifica o usuário e suas roles via claim `roles`.
- Não há logs de token/dados sensíveis no console por padrão.
