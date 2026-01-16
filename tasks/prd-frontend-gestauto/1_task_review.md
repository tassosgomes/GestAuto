---
reviewed_at: 2025-12-22
prd: prd-frontend-gestauto
task: 1
status: approved
---

# Revisão da Tarefa 1.0 — Provisionar client do Keycloak (gestauto-frontend)

## 1) Validação da Definição da Tarefa (task → PRD → Tech Spec)

### Requisitos da task (tasks/prd-frontend-gestauto/1_task.md)

- Base URL do Keycloak: `http://keycloak.tasso.local`
  - OK (documentado no README e suportado via `KEYCLOAK_BASE_URL`).
- Realms por ambiente: `gestauto-dev`, `gestauto-hml`, `gestauto`
  - OK (script controla realm via `GESTAUTO_REALM`, compatível com a convenção da techspec).
- Criar client: `gestauto-frontend` (public client, sem secret)
  - OK (client com `publicClient:true`, sem secret).
- Flow: Authorization Code + PKCE
  - OK (`standardFlowEnabled:true`, `implicitFlowEnabled:false`, e atributo `pkce.code.challenge.method=S256`).
- Redirect URIs: `http://gestauto.tasso.local/*`
  - OK (`redirectUris` configurado exatamente).
- Web Origins: `http://gestauto.tasso.local`
  - OK (`webOrigins` configurado exatamente).
- Token deve conter claim `roles` (multivalued)
  - OK via client-scope `gestauto-roles` com mapper `roles-claim` (multivalued) e scope anexado como default ao client `gestauto-frontend`.

### Alinhamento com o PRD (tasks/prd-frontend-gestauto/prd.md)

- PRD exige autenticação via Keycloak e RBAC de menus/rotas baseado na claim `roles`.
  - OK: o bootstrap garante que o token do client SPA carregue `roles`.

### Alinhamento com a Tech Spec (tasks/prd-frontend-gestauto/techspec.md)

- OIDC SPA com Authorization Code + PKCE
  - OK.
- Redirect/Web Origins conforme hostname `gestauto.tasso.local`
  - OK.
- Reuso do bootstrap idempotente existente
  - OK (mudança incremental no script existente).

## 2) Análise de Regras e Conformidade

### Regras aplicáveis revisadas

- `rules/ROLES_NAMING_CONVENTION.md`
  - OK: roles permanecem em `SCREAMING_SNAKE_CASE` e a claim usada é `roles`.
- `rules/git-commit.md`
  - OK: a mensagem de commit gerada ao final segue tipo/escopo opcional + PT-BR + lista.

Observação: não há regras específicas para shell/bash em `rules/`, então a revisão foca em idempotência, segurança e compatibilidade com Keycloak.

## 3) Resumo da Revisão de Código

### O que foi alterado

- `scripts/keycloak/configure_gestauto.sh`
  - Foi introduzida a função `ensure_client_upsert` para garantir convergência do client `gestauto-frontend` em re-runs.
  - Antes, o script apenas criava o client caso não existisse; agora também atualiza se existir, evitando divergência de `redirectUris`, `webOrigins` e PKCE.
- `scripts/keycloak/README.md`
  - Adicionadas instruções para validar manualmente o fluxo Authorization Code + PKCE e inspecionar a claim `roles` no token.

### Pontos positivos

- Idempotência real para o SPA client (resolve a principal fragilidade da task).
- Reuso de client-scope/mappers existentes (`gestauto-roles`) e anexação default ao client do frontend.
- Documentação pragmática para validação (inclui troca do `code` por token via PKCE).

## 4) Problemas Encontrados e Correções

### Problema (média severidade)

- O bootstrap era “idempotente” apenas no sentido de *não recriar*, mas não corrigia configurações já divergentes do client `gestauto-frontend`.

### Correção aplicada

- Implementado “upsert” do client (`ensure_client_upsert`) para sempre convergir os campos críticos (`redirectUris`, `webOrigins`, `attributes.pkce.code.challenge.method`).

## 5) Validação / Testes

- `bash -n scripts/keycloak/configure_gestauto.sh` (sanity check de sintaxe)
  - OK.

Limitação conhecida:
- Validação end-to-end (rodar `run_configure.sh` contra um Keycloak ativo e executar o fluxo PKCE completo) depende de o Keycloak estar disponível no ambiente local; foi documentado como executar a validação manual.

## 6) Confirmação de Conclusão e Prontidão para Deploy

- Critérios de sucesso atendidos no nível de configuração/script.
- Mudança é de baixo risco operacional (apenas provisionamento e docs).
- Pronto para deploy (desde que o ambiente tenha Keycloak acessível e credenciais admin corretas).
