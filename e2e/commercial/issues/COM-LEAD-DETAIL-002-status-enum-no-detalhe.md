# COM-LEAD-DETAIL-002 — Status exibido como enum técnico no Detalhe do Lead

## Severidade
Média

## Ambiente
- URL: https://gestauto.tasso.local/commercial/leads/{id}
- Usuário: `seller / 123456`

## Passos para reproduzir
1. Logar com `seller / 123456`
2. Abrir um lead em `/commercial/leads/{id}`
3. Observar o header

## Resultado atual
- Status aparece como `InNegotiation`.

## Resultado esperado
- Status em label pt-BR + badge.

## Evidência
- Heading inclui `InNegotiation`.

## Critérios de aceite
- [x] Status mapeado para label pt-BR no header.
- [x] Consistência com Dashboard/Listagem.

## Sugestão de correção
- Usar o mesmo mapper de status em todas as telas.
