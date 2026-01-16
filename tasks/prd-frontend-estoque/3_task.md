---
status: completed # Opções: pending, in-progress, completed, excluded
parallelizable: true
blocked_by: ["1.0"]
---

<task_context>
<domain>frontend/stock/types</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>medium</complexity>
<dependencies>external_apis</dependencies>
<unblocks>"4.0,5.0,6.0,7.0,8.0,9.0,10.0,11.0,12.0"</unblocks>
</task_context>

# Tarefa 3.0: Implementar tipos + helpers (labels PT-BR e datas)

## Visão Geral
Criar os tipos do domínio Stock no frontend e helpers reutilizáveis (labels PT-BR para enums numéricos e utilitário da regra de “prazo do banco” date-only + envio 18:00 local → UTC).

## Requisitos
- Tipos para veículos, histórico, reservas e test-drive (conforme Swagger e Tech Spec).
- Mapas/funcões para rótulos em PT-BR: status, categoria, tipos de reserva, motivos.
- Utilitário para converter data (YYYY-MM-DD) em `bankDeadlineAtUtc` aplicando 18:00 local e convertendo para ISO UTC.
- Tipos devem tolerar campos opcionais (ex.: preço/localização) sem quebrar a UI.

## Subtarefas
- [x] 3.1 Criar `frontend/src/modules/stock/types.ts` (ou estrutura por feature) com DTOs base.
- [x] 3.2 Implementar mappers `mapVehicleStatusLabel`, `mapVehicleCategoryLabel` (e outros necessários) com fallback “Desconhecido”.
- [x] 3.3 Implementar helper `toBankDeadlineAtUtc(dateOnly: string): string` (ou similar) seguindo a regra 18:00 local → UTC.
- [x] 3.4 Exportar helpers para uso em páginas e componentes.

## Conclusão
- [x] 3.0 Implementar tipos + helpers (labels PT-BR e datas) ✅ CONCLUÍDA
	- [x] 3.1 Implementação completada
	- [x] 3.2 Definição da tarefa, PRD e tech spec validados
	- [x] 3.3 Análise de regras e conformidade verificadas
	- [x] 3.4 Revisão de código completada
	- [x] 3.5 Pronto para deploy

## Sequenciamento
- Bloqueado por: 1.0
- Desbloqueia: 4.0, 5.0 e telas (6.0–11.0) + testes (12.0)
- Paralelizável: Sim (pode ocorrer em paralelo com 2.0)

## Detalhes de Implementação
- PRD: seção “Convenção: Prazo do banco (date-only)”.
- Tech Spec: seção “Modelos de Dados” e “Convenção de data”.

## Critérios de Sucesso
- Qualquer enum numérico exibido na UI aparece como label PT-BR.
- Conversão do prazo do banco produz ISO UTC coerente (com testes em 12.0).
