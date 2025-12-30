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
- Status aparece como enum técnico (`InNegotiation`).

## Resultado esperado
- Status deve aparecer como label pt-BR (ex.: “Em Negociação”), com `Badge` e cores padronizadas.

## Evidência
- Em “Leads Quentes” aparece `Lead Seller Success` com status `InNegotiation`.

## Critérios de aceite
- [ ] Mapear enums → labels pt-BR.
- [ ] Padronizar em Dashboard, Listagem e Detalhes.

## Sugestão de correção
- Criar um mapper único de status e reutilizar nos componentes.
