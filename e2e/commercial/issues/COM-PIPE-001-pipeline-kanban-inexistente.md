# COM-PIPE-001 — Tela de Pipeline (Kanban gerencial) inexistente

## Severidade
Alta

## Ambiente
- URL: https://gestauto.tasso.local/commercial/pipeline
- Usuário: `admin / admin`

## Contexto (PRD)
- PRD (5.2) exige visão de pipeline em kanban, com colunas por status e filtro por vendedor.

## Passos para reproduzir
1. Logar com `admin / admin`
2. Acessar `/commercial/pipeline`

## Resultado atual
- Página “Página não encontrada”.

## Resultado esperado
- Kanban de leads por status, com filtro por vendedor.

## Evidência
- Heading “Página não encontrada”.

## Critérios de aceite
## Reteste (2026-01-09)

### Resultado
- Rota `/commercial/pipeline` implementada e restrita a perfis gerenciais.
- Kanban renderiza colunas por status e cards.
- Filtro por vendedor disponível.

## Critérios de aceite
- [x] Rota/tela implementada e acessível apenas para `MANAGER` (ou perfil gerencial).
- [x] Kanban renderiza colunas e cards.
- [x] Filtro por vendedor disponível.

## Sugestão de correção
- Implementar rota e UI conforme PRD ou atualizar PRD/rotas oficiais se houve mudança.
