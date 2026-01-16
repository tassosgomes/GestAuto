---
status: completed # Opções: pending, in-progress, completed, excluded
parallelizable: true
blocked_by: ["2.0","5.0"]
---

<task_context>
<domain>frontend/stock/pages-vehicle-detail</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>high</complexity>
<dependencies>external_apis</dependencies>
<unblocks>"12.0"</unblocks>
</task_context>

# Tarefa 7.0: Implementar tela de Detalhe do Veículo + Histórico

## Visão Geral
Implementar `/stock/vehicles/{id}` com cabeçalho do veículo, ficha técnica (com ocultação de campos inexistentes no contrato) e aba/área de histórico em timeline.

## Requisitos
- Carregar `GET /vehicles/{id}` e `GET /vehicles/{id}/history`.
- Renderizar timeline cronológica por `occurredAtUtc` com tipo e resumo.
- Ocultar campos não disponíveis no contrato (ex.: preço/localização) sem quebrar layout.
- Ações principais no cabeçalho conforme RBAC (status, reserva, test-drive etc. conforme disponível).

## Subtarefas
- [x] 7.1 Implementar página de detalhe com estados loading/erro.
- [x] 7.2 Implementar componente de timeline/histórico (ordenável se necessário).
- [x] 7.3 Implementar ocultação de campos opcionais e fallback para valores ausentes.
- [x] 7.4 Conectar ações principais às mutations e garantir invalidation.

## Sequenciamento
- Bloqueado por: 2.0, 5.0
- Desbloqueia: 12.0
- Paralelizável: Sim

## Detalhes de Implementação
- PRD: “Detalhe do Veículo + Auditoria”.
- Tech Spec: “History” e invalidations pós-mutation.

## Critérios de Sucesso
- Detalhe e histórico carregam corretamente e se atualizam após ações.
- Timeline exibe eventos com labels PT-BR e datas coerentes.

## Checklist de conclusão

- [x] 7.0 Implementar tela de Detalhe do Veículo + Histórico ✅ CONCLUÍDA
	- [x] 7.1 Implementação completada
	- [x] 7.2 Definição da tarefa, PRD e tech spec validados
	- [x] 7.3 Análise de regras e conformidade verificadas
	- [x] 7.4 Revisão de código completada
	- [x] 7.5 Pronto para deploy
