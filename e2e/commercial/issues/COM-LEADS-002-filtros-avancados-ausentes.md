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
- Filtros continuam limitados a: busca por nome e combobox “Filtrar por Status”.
- “Filtrar por Status” aparenta ser **single-select** (lista simples com opções como “Todos”, “Novo”, “Contatado”, etc.).
- Não foi evidenciado filtro por Score, intervalo de Data de Criação, nem filtro por Vendedor.

## Critérios de aceite
- [ ] Status suporta multi-select.
- [ ] Existem filtros por Score e Data de Criação.
- [ ] Para `MANAGER`, existe filtro por Vendedor.
- [ ] Ordenação padrão aplicada.

## Sugestão de correção
- Implementar filtros via query params e refletir na UI.
