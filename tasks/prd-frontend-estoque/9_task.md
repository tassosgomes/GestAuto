---
status: pending # Opções: pending, in-progress, completed, excluded
parallelizable: true
blocked_by: ["2.0","5.0"]
---

<task_context>
<domain>frontend/stock/pages-movements</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>high</complexity>
<dependencies>external_apis</dependencies>
<unblocks>"12.0"</unblocks>
</task_context>

# Tarefa 9.0: Implementar Movimentações (check-in/check-out)

## Visão Geral
Implementar `/stock/movements` com abas “Registrar Entrada” e “Registrar Saída”, formulários e integração com endpoints de check-in/check-out.

## Requisitos
- UI inspirada nos mockups (abas, cards de origem/motivo).
- Integração:
  - `POST /vehicles/{id}/check-ins`
  - `POST /vehicles/{id}/check-outs`
- Feedback de sucesso/erro e prevenção de envio duplo.
- Ações e acesso conforme RBAC (ocultar telas/ações sensíveis se necessário).

## Subtarefas
- [ ] 9.1 Implementar página `/stock/movements` com abas e seleção de origem/motivo.
- [ ] 9.2 Implementar formulário de check-in (campos mínimos alinhados ao contrato).
- [ ] 9.3 Implementar formulário de check-out (motivo/observações, conforme contrato).
- [ ] 9.4 Garantir invalidation do veículo/histórico após submissão.

## Sequenciamento
- Bloqueado por: 2.0, 5.0
- Desbloqueia: 12.0
- Paralelizável: Sim

## Detalhes de Implementação
- PRD: “Movimentações (Entrada/Saída)”.
- Tech Spec: `vehicleService.checkIn/checkOut` e invalidations.

## Critérios de Sucesso
- Check-in/out executam com feedback e atualizam histórico do veículo.
- UI mantém consistência de loading/erro com o resto do app.
