---
status: pending # Opções: pending, in-progress, completed, excluded
parallelizable: false
blocked_by: ["2.0","3.0","4.0","5.0","6.0","7.0","8.0","9.0","10.0","11.0"]
---

<task_context>
<domain>frontend/stock/testing</domain>
<type>testing</type>
<scope>core_feature</scope>
<complexity>high</complexity>
<dependencies>http_server|external_apis</dependencies>
<unblocks>""</unblocks>
</task_context>

# Tarefa 12.0: Adicionar testes e validação de regressão do módulo Stock

## Visão Geral
Adicionar/ajustar testes (Vitest + Testing Library) para cobrir RBAC, mapeamento de labels, regras de datas (prazo do banco) e fluxos principais do módulo Stock, garantindo que o resto do frontend não regressa.

## Requisitos
- Teste de RBAC: usuário sem roles não vê menu/rotas do Stock.
- Testes de helpers: mappers de enum → label PT-BR e helper do prazo do banco (18:00 local → UTC).
- Testes de páginas principais com mocks (ex.: hooks/services mockados ou MSW se já existir padrão no repo).
- Rodar suíte de testes do frontend e corrigir falhas relacionadas às mudanças.

## Subtarefas
- [ ] 12.1 Criar testes unitários para `types/helpers` (labels e `toBankDeadlineAtUtc`).
- [ ] 12.2 Criar teste de navegação/RBAC (menu “Estoque” oculto quando não autorizado).
- [ ] 12.3 Criar testes smoke para pelo menos: Dashboard/lista, Detalhe/histórico, Reservas.
- [ ] 12.4 Executar `npm test` (ou comando padrão do repo) e garantir verde.

## Sequenciamento
- Bloqueado por: 2.0–11.0
- Desbloqueia: -
- Paralelizável: Não (consolida regressão/estabilidade)

## Detalhes de Implementação
- Tech Spec: “Abordagem de Testes”.

## Critérios de Sucesso
- Testes do frontend passam localmente.
- Cobertura mínima dos pontos críticos: RBAC, datas, labels e fluxos de navegação.
