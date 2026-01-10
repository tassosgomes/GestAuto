# RBAC-001 — Seller vê menu/rota “Aprovações” (gerencial)

## Severidade
Blocker

## Ambiente
- URL: https://gestauto.tasso.local
- Realm/SSO: Keycloak (gestauto)
- Usuário: `seller / 123456`

## Contexto (PRD)
- PRD: `tasks/prd-frontend-comercial/prd.md`
- Seção relevante: **5.1 Aprovação de Descontos** (acesso `MANAGER`)

## Passos para reproduzir
1. Acessar `https://gestauto.tasso.local`
2. Logar com `seller / 123456`
3. Expandir o menu “Comercial”

## Resultado atual
- O item “Aprovações” aparece no menu do usuário seller.
- Usuário consegue navegar para `/commercial/approvals`.

## Resultado esperado
- Seller não deve visualizar o item “Aprovações”.
- A rota `/commercial/approvals` deve ser bloqueada para perfis não-`MANAGER`.

## Evidência
- URL: `/` (pós-login)
- Menu “Comercial” expandido contém: `Dashboard`, `Leads`, `Propostas`, `Test-Drives`, `Aprovações`.
- Trecho: `link "Aprovações"  /url: /commercial/approvals`

## Critérios de aceite
## Reteste (2026-01-09)

### Resultado
- Item “Aprovações” ainda aparece para `seller` no submenu de “Comercial”.
- `seller` consegue navegar para `/commercial/approvals` (não cai em “Acesso negado”).
- Console registra erro ao buscar aprovações pendentes (HTTP 404 no endpoint de approvals).

## Critérios de aceite
- [ ] Menu: “Aprovações” aparece apenas para role `MANAGER` (ou equivalente).
- [ ] Guard: acesso direto a `/commercial/approvals` retorna “Acesso negado” para roles não autorizadas.
- [ ] Teste automatizado: adicionar/ajustar teste de RBAC garantindo o comportamento (menu + guard).

## Sugestão de correção
- Revisar cálculo de menus baseado nas roles do token.
- Aplicar guard de rota em `/commercial/approvals` (e/ou no subtree `/commercial/*`) exigindo role `MANAGER`.
