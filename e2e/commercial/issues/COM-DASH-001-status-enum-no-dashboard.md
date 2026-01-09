# COM-DASH-001 — Status exibido como enum técnico no Dashboard

## Severidade
Média

## Ambiente
- URL: https://gestauto.tasso.local
- Usuário: `seller / 123456`

## Contexto (PRD)
- Dashboard deve exibir status em formato amigável (badge colorido) e consistente.

## Passos para reproduzir
1. Logar com `seller / 123456`
2. Ir em `Comercial → Dashboard` (`/commercial`)
3. Observar a lista “Leads Quentes”

## Resultado atual
- (Antes) Status aparecia como enum técnico (`InNegotiation`).

## Reteste (pós rebuild/redeploy)
- Data: 2025-12-30
- Resultado: ✅ **OK** — status exibido como label pt-BR

## Reteste (2026-01-09)

### Resultado
- ✅ **OK** — status segue exibido como label pt-BR, com `Badge` e cores consistentes.

## Resultado esperado
- Status deve aparecer como label pt-BR (ex.: “Em Negociação”), com `Badge` e cores padronizadas.

## Evidência
- Em “🔥 Leads Quentes” aparecem leads (ex.: `Lead Seller Success`) com status **“Em Negociação”**.
- URL validada: `https://gestauto.tasso.local/commercial`

## Critérios de aceite
- [x] Mapear enums → labels pt-BR.
- [x] Padronizar em Dashboard, Listagem e Detalhes.

## Sugestão de correção
- Criar um mapper único de status e reutilizar nos componentes.
