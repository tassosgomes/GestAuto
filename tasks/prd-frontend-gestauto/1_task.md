---
status: completed
parallelizable: true
blocked_by: []
---

## markdown

## status: completed # Opções: pending, in-progress, completed, excluded

<task_context>
<domain>engine/infra/auth</domain>
<type>integration</type>
<scope>configuration</scope>
<complexity>medium</complexity>
<dependencies>external_apis</dependencies>
<unblocks>"4.0"</unblocks>
</task_context>

# Tarefa 1.0: Provisionar client do Keycloak (gestauto-frontend)

## Visão Geral

Provisionar, de forma **idempotente**, o client SPA do frontend no Keycloak para permitir autenticação via **Authorization Code + PKCE**.

Referências:
- PRD: `tasks/prd-frontend-gestauto/prd.md`
- Tech Spec: `tasks/prd-frontend-gestauto/techspec.md`

<requirements>
- Base URL do Keycloak: `http://keycloak.tasso.local`
- Realms por ambiente: `gestauto-dev`, `gestauto-hml`, `gestauto`
- Criar client: `gestauto-frontend` (public client, sem secret)
- Flow: Authorization Code + PKCE
- Redirect URIs: `http://gestauto.tasso.local/*`
- Web Origins: `http://gestauto.tasso.local`
- Token deve conter claim `roles` (multivalued)
</requirements>

## Subtarefas

- [x] 1.1 Estender `scripts/keycloak/configure_gestauto.sh` para criar o client `gestauto-frontend` (idempotente)
- [x] 1.2 Configurar Redirect URIs e Web Origins para `http://gestauto.tasso.local`
- [x] 1.3 Garantir que o client do frontend receba a claim `roles` (via client-scope/mappers existentes)
- [x] 1.4 Atualizar `scripts/keycloak/README.md` com a validação do client do frontend (como testar login)

## Sequenciamento

- Bloqueado por: nenhum
- Desbloqueia: 4.0
- Paralelizável: Sim

## Detalhes de Implementação

- Ver seção “Pontos de Integração / Keycloak (OIDC)” em `tasks/prd-frontend-gestauto/techspec.md`.
- Reusar a abordagem de bootstrap idempotente já existente em `scripts/keycloak/`.

## Critérios de Sucesso

- O client `gestauto-frontend` existe no realm do ambiente e está configurado como public client.
- Login via Authorization Code + PKCE funciona com redirect para `http://gestauto.tasso.local/*`.
- Tokens emitidos para o frontend incluem a claim `roles` com as roles do usuário.

## Checklist de conclusão

- [x] 1.0 Provisionar client do Keycloak (gestauto-frontend) ✅ CONCLUÍDA
	- [x] 1.1 Implementação completada
	- [x] 1.2 Definição da tarefa, PRD e tech spec validados
	- [x] 1.3 Análise de regras e conformidade verificadas
	- [x] 1.4 Revisão de código completada
	- [x] 1.5 Pronto para deploy
