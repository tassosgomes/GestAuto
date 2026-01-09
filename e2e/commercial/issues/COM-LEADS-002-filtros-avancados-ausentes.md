# COM-LEADS-002 — Filtros avançados ausentes / status não é multi-select

## Severidade
Alta

## Ambiente
- URL: https://gestauto.tasso.local/commercial/leads
- Usuário: `seller / 123456`

## Contexto (PRD)
- PRD exige filtros: Status (multi-select), Score, Data de Criação; e Vendedor (apenas gerente).
- Ordenação padrão: Score desc > Data Criação desc.

## Passos para reproduzir
1. Logar com `seller / 123456`
2. Acessar `/commercial/leads`
3. Observar filtros disponíveis

## Resultado atual
- Apenas busca por nome e um combobox “Filtrar por Status”.
- Não foi evidenciado multi-select.
- Ausentes filtros por Score, Data de Criação e Vendedor.

## Resultado esperado
- Filtros avançados conforme PRD, permitindo priorização e gestão do funil.

## Evidência
- Área de filtros contém somente “Buscar por nome...” e “Filtrar por Status”.

## Reteste (2026-01-09)

### Resultado
- Status com multi-select.
- Filtros por Score e intervalo de Data de Criação.
- Para perfil gerencial (`MANAGER`/equivalentes), filtro por Vendedor.
- Ordenação padrão aplicada na listagem.

## Critérios de aceite
- [x] Status suporta multi-select.
- [x] Existem filtros por Score e Data de Criação.
- [x] Para `MANAGER`, existe filtro por Vendedor.
- [x] Ordenação padrão aplicada.

## Sugestão de correção
- Implementar filtros via query params e refletir na UI.
