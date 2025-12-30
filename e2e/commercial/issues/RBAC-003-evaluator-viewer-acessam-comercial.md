# RBAC-003 — evaluator e viewer enxergam e acessam Comercial

## Severidade
Blocker

## Ambiente
- URL: https://gestauto.tasso.local
- Usuários:
  - `evaluator / 123456`
  - `viewer / 123456`

## Contexto (README)
- evaluator → deve ver menu **Avaliações**
- viewer → deve ver **somente Avaliações**

## Passos para reproduzir
1. Logar com `evaluator / 123456` (ou `viewer / 123456`)
2. Observar o menu lateral
3. Expandir “Comercial”

## Resultado atual
- evaluator e viewer visualizam o menu “Comercial”.
- Ao expandir, aparecem `Dashboard`, `Leads`, `Propostas`, `Test-Drives`, `Aprovações`.

## Resultado esperado
- evaluator: ver apenas “Avaliações” (conforme README).
- viewer: ver somente “Avaliações” (conforme README).
- Mesmo se o menu estiver incorreto, o guard deve impedir acesso a `/commercial/*`.

## Evidência
- evaluator pós-login mostra menu com `Comercial`, `Avaliações`, `Configurações`.
- viewer pós-login mostra `Comercial` e submenu completo ao expandir.

## Critérios de aceite
- [ ] evaluator e viewer não veem o menu “Comercial”.
- [ ] evaluator e viewer recebem “Acesso negado” ao navegar direto para `/commercial/*`.
- [ ] Testes automatizados cobrindo matriz de acesso (menus + guards).

## Sugestão de correção
- Corrigir cálculo de menus baseado nas roles do token.
- Aplicar guard por subtree (`/commercial/*`) para bloquear acesso sem role comercial.
