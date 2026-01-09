# UX-001 — Página Home exibe “Configuração Runtime” (conteúdo técnico) para usuários finais

## Severidade
Média

## Ambiente
- URL: https://gestauto.tasso.local
- Usuário: qualquer (seller/evaluator/viewer/admin)

## Passos para reproduzir
1. Logar
2. Acessar `/`

## Resultado atual
- A Home exibe um card “Configuração Runtime” com JSON técnico (Keycloak base URL, realm, clientId, etc.).

## Resultado esperado
- A Home não deve expor informações técnicas em ambiente de uso normal.

## Evidência
- Card “Configuração Runtime” renderiza JSON de configuração.

## Critérios de aceite
## Reteste (2026-01-09)

### Resultado
- Card “Configuração Runtime” não é exibido fora de ambiente de desenvolvimento.

## Critérios de aceite
- [x] Em produção, o card não aparece para usuários finais.
- [x] Se necessário, conteúdo técnico fica restrito a ambiente dev/rota interna (ex.: `/design`) ou feature flag.

## Sugestão de correção
- Remover da Home ou proteger via env/feature-flag (ex.: somente `import.meta.env.DEV`).
