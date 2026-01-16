---
status: pending
parallelizable: true
blocked_by: []
---

<task_context>
<domain>frontend/commercial/components</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>medium</complexity>
<dependencies>react-hook-form|zod</dependencies>
<unblocks>["3.0"]</unblocks>
</task_context>

# Tarefa 2.0: Implementar Formulário de Qualificação

## Visão Geral
Desenvolver o formulário para entrada de dados de qualificação do lead, utilizando `react-hook-form` e `zod` para validação.

## Requisitos
- Formulário deve validar campos obrigatórios.
- Campos de veículo de troca devem aparecer apenas se "Veículo na Troca" for marcado.
- Deve chamar o serviço `leadService.qualify` ao submeter.

## Subtarefas
- [x] 2.1 Definir schema Zod para validação (incluindo condicional de troca). ✅
- [x] 2.2 Criar componente `LeadQualificationForm` com `react-hook-form`. ✅
- [x] 2.3 Implementar campos condicionais para dados do veículo de troca. ✅
- [x] 2.4 Integrar chamada ao `leadService.qualify` com tratamento de erro/sucesso (Toast). ✅
- [x] 2.5 Criar testes unitários de validação e submissão. ✅

## Status da Revisão
- [x] 2.0 [Implementar Formulário de Qualificação] ✅ CONCLUÍDA
  - [x] Implementação completada ✅
  - [x] Definição da tarefa, PRD e tech spec validados ✅
  - [x] Análise de regras e conformidade verificadas ✅
  - [x] Revisão de código completada ✅
  - [x] Testes unitários passando (8/8) ✅
  - [x] Build de produção validado ✅
  - [x] Pronto para deploy ✅

## Sequenciamento
- Bloqueado por: N/A
- Desbloqueia: 3.0
- Paralelizável: Sim

## Detalhes de Implementação
- **Schema:**
    - `paymentMethod`: Obrigatório.
    - `tradeInVehicle`: Obrigatório se `hasTradeInVehicle` for true.
- **UI:** Usar componentes `Form`, `Input`, `Select`, `Checkbox` do shadcn/ui.

## Critérios de Sucesso
- Validação impede submissão de dados incompletos.
- Campos condicionais funcionam corretamente.
- Submissão chama a API corretamente.
