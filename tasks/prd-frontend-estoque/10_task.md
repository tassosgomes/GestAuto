---
status: completed # Opções: pending, in-progress, completed, excluded
parallelizable: true
blocked_by: ["1.0","2.0","5.0"]
---

<task_context>
<domain>frontend/stock/pages-test-drives</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>high</complexity>
<dependencies>external_apis</dependencies>
<unblocks>"12.0"</unblocks>
</task_context>

# Tarefa 10.0: Implementar Monitor de Test-drive + Finalização

## Visão Geral
Implementar `/stock/test-drives` para monitorar test-drives em andamento e permitir finalização (quando autorizada), incluindo campo opcional de observações (`notes`) se suportado pelo backend.

## Requisitos
- Lista de test-drives em andamento (ou derivada do contrato disponível) com tempo decorrido.
- Destaque visual para “atrasados” (SLA inicial simples, configurável depois).
- Ação “Finalizar test-drive” com resultado e observações opcionais.
- Se o backend não suportar `notes`/endpoint, UI deve ocultar o campo/ação ou degradar conforme decisão na Tarefa 1.0.

## Subtarefas
- [x] 10.1 Implementar página `/stock/test-drives` (KPIs + lista).
- [x] 10.2 Implementar ação de iniciar test-drive a partir do veículo quando aplicável.
- [x] 10.3 Implementar modal/drawer de finalização com validações e campo `notes` opcional.
- [x] 10.4 Garantir invalidation de listas/detalhe/histórico após start/complete.

## Sequenciamento
- Bloqueado por: 1.0, 2.0, 5.0
- Desbloqueia: 12.0
- Paralelizável: Sim

## Detalhes de Implementação
- PRD: “Controle de Test-drive (Monitoramento)” e “Questões em Aberto”.
- Tech Spec: dependência do backend para `notes` em `CompleteTestDriveRequest`.

## Critérios de Sucesso
- Usuário autorizado consegue finalizar um test-drive e ver atualização refletida.
- Caso `notes` não exista, comportamento de fallback está aplicado conforme 1.0.

## Checklist de conclusão

- [x] 10.0 Implementar Monitor de Test-drive + Finalização ✅ CONCLUÍDA
	- [x] 10.1 Implementação completada
	- [x] 10.2 Definição da tarefa, PRD e tech spec validados
	- [x] 10.3 Análise de regras e conformidade verificadas
	- [x] 10.4 Revisão de código completada
	- [x] 10.5 Pronto para deploy
