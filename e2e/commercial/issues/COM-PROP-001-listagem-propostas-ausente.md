# COM-PROP-001 — Listagem de Propostas não implementada

## Severidade
Alta

## Ambiente
- URL: https://gestauto.tasso.local/commercial/proposals
- Usuário: `seller / 123456`

## Contexto (PRD)
- PRD exige tabela com colunas: Nº Proposta, Cliente, Veículo, Valor Total, Status, Data.
- Filtros: Status.

## Passos para reproduzir
1. Logar com `seller / 123456`
2. Acessar `/commercial/proposals`

## Resultado atual
- Página exibe: “Listagem de propostas será implementada na próxima tarefa.”

## Resultado esperado
- Tabela e filtros conforme PRD.

## Evidência
- Mensagem de backlog na tela.

## Critérios de aceite
- [ ] Tabela com colunas do PRD.
- [ ] Filtro por status.
- [ ] Estados de loading (Skeleton) e erro (Toaster).

## Sugestão de correção
- Implementar listagem consumindo API REST do módulo commercial.
