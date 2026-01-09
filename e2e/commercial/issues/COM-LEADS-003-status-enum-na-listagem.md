# COM-LEADS-003 — Status exibido como enum técnico na listagem (ProposalSent/InNegotiation)

## Severidade
Média

## Ambiente
- URL: https://gestauto.tasso.local/commercial/leads
- Usuário: `seller / 123456`

## Passos para reproduzir
1. Logar com `seller / 123456`
2. Acessar `/commercial/leads`
3. Observar a coluna “Status”

## Resultado atual
- Status aparece como `ProposalSent`, `InNegotiation`.

## Resultado esperado
- Status em pt-BR, com badges (ex.: `Novo`, `Em Negociação`, etc.).

## Evidência
- Coluna Status mostra `ProposalSent` e `InNegotiation`.

## Critérios de aceite
## Reteste (2026-01-09)

### Resultado
- Status é exibido como label pt-BR (ex.: “Em Negociação”, “Proposta Enviada”).
- Badge consistente com Dashboard e Detalhes.

## Critérios de aceite
- [x] Mapeamento de enums para labels pt-BR.
- [x] Badge colorido consistente.

## Sugestão de correção
- Centralizar mapeamento (mesmo usado no dashboard e detalhes).
