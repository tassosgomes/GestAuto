---
status: pending # Opções: pending, in-progress, completed, excluded
parallelizable: true
blocked_by: ["2.0","5.0"]
---

<task_context>
<domain>frontend/stock/pages-dashboard</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>high</complexity>
<dependencies>external_apis</dependencies>
<unblocks>"12.0"</unblocks>
</task_context>

# Tarefa 6.0: Implementar telas de Dashboard e Listagem de Veículos

## Visão Geral
Implementar `/stock` (visão geral) e `/stock/vehicles` (listagem dedicada), com filtros, paginação e ações rápidas conforme RBAC.

## Requisitos
- Dashboard `/stock` com KPIs (mínimo) e tabela de veículos.
- Filtros: `q` (VIN/Modelo/Placa), `category`, `status`.
- Paginação com `_page/_size` e UI consistente.
- Ações na linha conforme RBAC (ex.: abrir detalhe; reservar; iniciar test-drive; alterar status quando permitido).
- Loading com skeleton e empty-state.

## Subtarefas
- [ ] 6.1 Implementar layout da página `/stock` com cards de KPI (derivados da lista ou endpoint disponível).
- [ ] 6.2 Implementar tabela de veículos com badges (status/categoria) e colunas do PRD.
- [ ] 6.3 Implementar página `/stock/vehicles` reutilizando componentes e adicionando paginação/ações.
- [ ] 6.4 Garantir que ações não autorizadas ficam ocultas (não apenas desabilitadas).

## Sequenciamento
- Bloqueado por: 2.0, 5.0
- Desbloqueia: 12.0
- Paralelizável: Sim (em paralelo com 7.0–11.0 depois que hooks existirem)

## Detalhes de Implementação
- PRD: “Visão Geral do Estoque” e “Veículos (Listagem)”.

## Critérios de Sucesso
- Usuário autorizado consegue filtrar/paginar e navegar ao detalhe.
- KPIs e tabela renderizam com estados de loading/erro/empty consistentes.
