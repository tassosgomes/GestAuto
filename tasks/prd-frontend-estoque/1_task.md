---
status: completed # Opções: pending, in-progress, completed, excluded
parallelizable: true
blocked_by: []
---

<task_context>
<domain>frontend/stock/integration</domain>
<type>integration</type>
<scope>core_feature</scope>
<complexity>low</complexity>
<dependencies>external_apis|http_server</dependencies>
<unblocks>"2.0,3.0,4.0"</unblocks>
</task_context>

# Tarefa 1.0: Validar contrato da Stock API (Swagger) e gaps

## Visão Geral
Validar (no Swagger do ambiente) os endpoints e modelos necessários para o módulo de Estoque, registrando gaps e decisões de fallback no frontend (ex.: ocultar campos/ações não suportadas).

## Requisitos
- Identificar base URL e confirmar que endpoints estão sob `/api/v1`.
- Confirmar paginação `_page`/`_size` em endpoints de coleção.
- Confirmar formato de erro `ProblemDetails` (RFC 9457) e campos comuns (`title`, `detail`, `status`).
- Validar existência do endpoint `POST /api/v1/test-drives/{testDriveId}/complete` e presença do campo opcional `notes` (ou definir fallback de UI).

## Subtarefas
- [x] 1.1 Acessar Swagger (ex.: `http://localhost:8089/swagger/v1/swagger.json`) e listar endpoints relevantes do MVP.
- [x] 1.2 Mapear modelos usados no frontend (vehicles, history, reservations, test-drives) e anotar enum values numéricos.
- [x] 1.3 Registrar gaps de contrato (campos ausentes como preço/localização; ausência de `notes`; endpoint faltando no swagger).
- [x] 1.4 Definir fallback do frontend para cada gap (ocultar campo/ação; exibir placeholder; feature flag simples por detecção de contrato se aplicável).

## Sequenciamento
- Bloqueado por: -
- Desbloqueia: 2.0, 3.0, 4.0
- Paralelizável: Sim (não exige mudanças de código)

## Detalhes de Implementação
- Tech Spec: “Endpoints de API”, “Riscos conhecidos”, “Dependências Técnicas”.
- PRD: “Integração com API” e “Questões em Aberto”.

## Critérios de Sucesso
- Lista dos endpoints/modelos confirmados e gaps documentados no próprio arquivo da tarefa.
- Decisão explícita de como a UI deve se comportar quando `notes`/endpoint não estiver disponível.

## Resultado da validação (Swagger do ambiente)

Data da validação: 2026-01-16

Swagger consultado em: `http://localhost:8089/swagger/v1/swagger.json`

### Base URL / versionamento
- Endpoints do Stock estão **majoritariamente** sob `/api/v1`.

### Endpoints confirmados (MVP)

**Veículos**
- `GET /api/v1/vehicles` (lista + filtros + paginação)
- `GET /api/v1/vehicles/{id}` (detalhe)
- `GET /api/v1/vehicles/{id}/history` (auditoria/histórico)
- `PATCH /api/v1/vehicles/{id}/status` (alterar status)
- `POST /api/v1/vehicles/{id}/check-ins`
- `POST /api/v1/vehicles/{id}/check-outs`
- `POST /api/v1/vehicles/{id}/test-drives/start`

**Reservas**
- `POST /api/v1/vehicles/{vehicleId}/reservations`
- `POST /api/v1/reservations/{reservationId}/cancel`
- `POST /api/v1/reservations/{reservationId}/extend`

**Test-drive**
- **GAP**: Swagger expõe `POST /test-drives/{testDriveId}/complete` **sem** prefixo `/api/v1`.

### Paginação
- `GET /api/v1/vehicles` aceita `_page`/`_size` (defaults `1` e `10`).
- Também aceita `page`/`pageSize` (compat) e o backend prioriza `page/pageSize` quando informado.

### Erros (ProblemDetails)
- Existe schema `ProblemDetails` no Swagger.
- Campos observados: `title`, `detail`, `status` (e também `type`, `instance`).

### Modelos (schemas) relevantes para o frontend

**Vehicles**
- `VehicleResponse` (detalhe)
- `VehicleHistoryResponse` + `VehicleHistoryItemResponse` (timeline)
- **Observação**: o response schema do `GET /api/v1/vehicles` não está descrito no Swagger.

**Reservations**
- `CreateReservationRequest` (inclui `bankDeadlineAtUtc`)
- `ReservationResponse`
- `CancelReservationRequest`
- `ExtendReservationRequest`

**Test-drive**
- `StartTestDriveRequest` / `StartTestDriveResponse`
- `CompleteTestDriveRequest` / `CompleteTestDriveResponse`

### Enums numéricos (valores do backend)

**VehicleStatus**: `1` InTransit, `2` InStock, `3` Reserved, `4` InTestDrive, `5` InPreparation, `6` Sold, `7` WrittenOff

**VehicleCategory**: `1` New, `2` Used, `3` Demonstration

**ReservationType**: `1` Standard, `2` PaidDeposit, `3` WaitingBank

**ReservationStatus**: `1` Active, `2` Cancelled, `3` Completed, `4` Expired

**TestDriveOutcome**: `1` ReturnedToStock, `2` ConvertedToReservation

### Gaps identificados e fallback/decisões de UI

1) **Finalização de test-drive fora de `/api/v1`**
- Gap: endpoint esperado é `POST /api/v1/test-drives/{testDriveId}/complete`, mas Swagger atual expõe `POST /test-drives/{testDriveId}/complete`.
- Fallback (decisão): no frontend, implementar o client de finalização com fallback de rota: tentar `/api/v1/test-drives/{id}/complete` e, em caso de `404`, tentar `/test-drives/{id}/complete`. Se ambos falharem, ocultar/desabilitar a ação “Finalizar test-drive” com mensagem amigável.

2) **Campo `notes` na finalização de test-drive**
- Gap: `CompleteTestDriveRequest` não contém `notes` no contrato atual.
- Fallback (decisão): UI de finalização deve ocultar o campo de observações (ou exibir como “não suportado”, sem enviar para API). Manter suporte a `notes` apenas quando o backend adicionar o campo no contrato.

3) **Schema ausente na listagem de veículos**
- Gap: o Swagger retorna `200 OK` sem `content/schema` para `GET /api/v1/vehicles`, então não dá para validar o shape via contrato.
- Fallback (decisão): assumir retorno real como `PagedResponse<VehicleListItem>` com `data: VehicleListItem[]` e `pagination: { page, size, total, totalPages }` (conforme o tipo do backend).

4) **Campos de preço/localização não existem no modelo atual**
- Gap: `VehicleResponse` não expõe `price` e nem `location`.
- Fallback (decisão): ocultar esses campos na UI (sem placeholder) até o backend expor. “Dias no estoque” pode ser derivado de `createdAt`.

5) **Filtros `status` e `category` como string**
- Observação: query params são `string`; backend faz parse por nome do enum.
- Decisão: ao filtrar, enviar nomes compatíveis com o enum (ex.: `InStock`, `Reserved`, `Demonstration`) em vez de valores numéricos.

## Checklist de conclusão

- [x] 1.0 Validar contrato da Stock API (Swagger) e gaps ✅ CONCLUÍDA
	- [x] 1.1 Implementação completada
	- [x] 1.2 Definição da tarefa, PRD e tech spec validados
	- [x] 1.3 Análise de regras e conformidade verificadas
	- [x] 1.4 Revisão de código completada
	- [x] 1.5 Pronto para deploy
