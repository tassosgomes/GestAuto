# COM-LEAD-NEW-002 — Modal “Novo Lead” incompleto vs PRD (campos/opções)

## Severidade
Média

## Ambiente
- URL: https://gestauto.tasso.local/commercial/leads
- Usuário: `seller / 123456`

## Contexto (PRD)
- Obrigatórios: Nome Completo, Telefone (com máscara), Email, Origem.
- Opcionais: Modelo de Interesse, Versão/Cor.
- Origem deve cobrir: instagram, indicacao, google, loja, telefone, showroom, portal_classificados, outros.

## Passos para reproduzir
1. Logar com `seller / 123456`
2. Acessar `/commercial/leads`
3. Clicar em “Novo Lead”

## Resultado atual
- Campos: Nome, Email, Telefone, Origem, Modelo de Interesse.
- Não há campo “Versão/Cor”.
- Origem não inclui “showroom” nem “portal_classificados” (e labels/values não estão 1:1 com PRD).

## Resultado esperado
- Modal com todos os campos e opções do PRD.

## Evidência
- Listbox de origem não inclui “showroom” nem “portal_classificados”; não há campo Versão/Cor.

## Critérios de aceite
## Reteste (2026-01-09)

### Resultado
- Campo “Versão/Cor” presente.
- Opções de “Origem” cobrem o PRD (com labels amigáveis).
- Máscara/normalização de telefone aplicada no input.

## Critérios de aceite
- [x] Campo “Versão/Cor” presente.
- [x] Lista de “Origem” cobre todas as opções do PRD (com labels amigáveis).
- [x] Máscara de telefone aplicada.

## Sugestão de correção
- Ajustar enum/lista de origem e adicionar campo “Versão/Cor”.
