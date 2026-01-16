# Tech Spec - Frontend Módulo de Estoque

## Resumo Executivo

Esta Tech Spec descreve como implementar o **Módulo de Estoque** no frontend do GestAuto sob a rota **`/stock`**, aderindo aos padrões já existentes no repo: **React + Vite**, **React Router (useRoutes)**, **TanStack Query** para dados, **axios** centralizado em `frontend/src/lib/api.ts` com token Keycloak, e **RBAC por roles** com ocultação de menus/ações.

A implementação será feita como um novo módulo (análogo ao `modules/commercial`), com rotas próprias e layouts, consumindo a **GestAuto Stock API** versionada em **`/api/v1`**. A UI seguirá os mockups fornecidos no PRD e manterá consistência com o design system e padrões atuais de navegação. Principais decisões: (1) separar `services/` (HTTP) de `hooks/` (TanStack Query) por domínio, (2) esconder itens/ações não autorizados (não apenas desabilitar), (3) padronizar paginação para `_page/_size` (backend) e permitir compatibilidade com `page/pageSize` quando aplicável, (4) implementar convenção de “prazo do banco” como date-only na UI, enviando `18:00` local convertido para UTC.

## Arquitetura do Sistema

### Visão Geral dos Componentes

- **Frontend (React/Vite)**
  - **Roteamento**: `frontend/src/App.tsx` usa `useRoutes` com guardas `RequireAuth` e `RequireMenuAccess`.
  - **Navegação/Sidebar**: `frontend/src/config/navigation.tsx` define `navItems`; `frontend/src/components/layout/Sidebar.tsx` aplica filtro por `menu` + `permission` (roles).
  - **Auth/RBAC**:
    - Keycloak via `frontend/src/auth/keycloakAuthService.ts`, com roles extraídas da claim `roles` (fallback `realm_access.roles`).
    - Menus visíveis via `frontend/src/rbac/rbac.ts`.
  - **Dados**:
    - Cliente HTTP central: `frontend/src/lib/api.ts` com `Authorization: Bearer <token>`.
    - `appBaseUrl` carregado runtime de `frontend/public/app-config.json` via `AppConfigProvider`.
    - Fetching/cache via TanStack Query (ex.: módulo commercial em `frontend/src/modules/commercial/hooks/*`).

- **Stock API (.NET)**
  - Endpoints previstos no PRD sob `/api/v1`.
  - Padrão de erro: `ProblemDetails` (RFC 9457) — refletido em controllers com `[ProducesResponseType(typeof(ProblemDetails), ...)]`.

Fluxo típico:
1) Usuário autentica via Keycloak (token contém `roles`).
2) UI decide visibilidade de menu/ações via roles.
3) Páginas de estoque usam hooks (`useQuery/useMutation`) para chamar `api` com baseURL `.../api/v1`.
4) Erros `ProblemDetails` são convertidos em mensagens amigáveis via toasts.

## Design de Implementação

### Interfaces Principais

**Frontend (serviços HTTP)** — padrão já usado (ex.: `proposalService`, `leadService`, `testDriveService`). Para o estoque, seguir o mesmo:

```ts
export const vehicleService = {
  list: async (params?: { page?: number; size?: number; status?: string; category?: string; q?: string }) => {
    // chama GET /vehicles com _page/_size
  },
  getById: async (id: string) => {
    // GET /vehicles/{id}
  },
  getHistory: async (id: string) => {
    // GET /vehicles/{id}/history
  },
  changeStatus: async (id: string, data: ChangeVehicleStatusRequest) => {
    // PATCH /vehicles/{id}/status
  },
  checkIn: async (id: string, data: CheckInCreateRequest) => {
    // POST /vehicles/{id}/check-ins
  },
  checkOut: async (id: string, data: CheckOutCreateRequest) => {
    // POST /vehicles/{id}/check-outs
  },
  startTestDrive: async (id: string, data: StartTestDriveRequest) => {
    // POST /vehicles/{id}/test-drives/start
  },
}
```

**Frontend (hooks TanStack Query)**
- `useVehiclesList(params)` → `queryKey: ['stock-vehicles', params]`
- `useVehicle(id)` → `['stock-vehicle', id]`
- `useVehicleHistory(id)` → `['stock-vehicle-history', id]`
- mutations invalidam listas/detalhes/histórico conforme necessário.

### Modelos de Dados

Criar tipos do domínio Stock no frontend, preferencialmente em `frontend/src/modules/stock/types.ts` (ou organizado por feature):

- **Vehicles**
  - `VehicleListItem`
  - `VehicleResponse`
  - `VehicleStatus` (enum numérico vindo do backend) + `mapVehicleStatusLabel(status)`
  - `VehicleCategory` (enum numérico) + `mapVehicleCategoryLabel(category)`

- **History**
  - `VehicleHistoryResponse` com lista de eventos ordenáveis por `occurredAtUtc`
  - Tipos de evento: check-in/out, reserva, test-drive, status-change.

- **Reservations**
  - `CreateReservationRequest`, `ReservationResponse`
  - `CancelReservationRequest`, `ExtendReservationRequest`
  - Campos relevantes do PRD: tipo/status, `bankDeadlineAtUtc` (se existir) e datas.

- **Test Drive**
  - `StartTestDriveRequest/Response`
  - `CompleteTestDriveRequest`:
    - incluir resultado/flag conforme contrato
    - incluir `notes?: string` (depende do backend; ver “Dependências Técnicas”)

**Convenção de data**
- Campos `...AtUtc` são tratados como ISO string UTC.
- Para “prazo do banco”: UI solicita apenas data e envia `YYYY-MM-DDT18:00:00` no fuso local convertido para UTC.

### Endpoints de API

Base URL: `appBaseUrl` vindo de `frontend/public/app-config.json` (ambiente atual: `https://gestauto.tasso.local/api/v1`).

**Veículos**
- `GET /vehicles?_page=&_size=&status=&category=&q=`
- `GET /vehicles/{id}`
- `GET /vehicles/{id}/history`
- `PATCH /vehicles/{id}/status`
- `POST /vehicles/{id}/check-ins`
- `POST /vehicles/{id}/check-outs`
- `POST /vehicles/{id}/test-drives/start`

**Reservas**
- `POST /vehicles/{vehicleId}/reservations`
- `POST /reservations/{reservationId}/cancel`
- `POST /reservations/{reservationId}/extend`

**Test-drives**
- `POST /test-drives/{testDriveId}/complete`

Erros:
- Backend retorna `ProblemDetails`. Frontend deve extrair `title/detail/status` quando presentes.

## Pontos de Integração

- **Keycloak**
  - Requer token com claim `roles` (mapper `User Realm Role`).
  - Novas roles para estoque:
    - `STOCK_PERSON`
    - `STOCK_MANAGER`

- **GestAuto Stock API**
  - Deve expor endpoints sob `/api/v1`.
  - Deve manter compatibilidade com padrões de paginação `_page/_size`.

## Análise de Impacto

| Componente Afetado | Tipo de Impacto | Descrição & Nível de Risco | Ação Requerida |
| --- | --- | --- | --- |
| Frontend RBAC | Mudança de tipos | Adiciona roles `STOCK_PERSON`/`STOCK_MANAGER` ao tipo `Role`. Baixo risco. | Ajustar e rodar testes frontend |
| Frontend Navegação | Nova feature | Adicionar rotas/pages do módulo Stock e itens de menu. Risco médio (roteamento). | Implementar módulo + testes |
| Stock API - Vehicles | Mudança de roteamento | Padronizar `/api/v1/vehicles` e manter compatibilidade. Risco médio (clientes). | Validar swagger e testes |
| Stock API - Test-drive complete | Mudança de contrato (planejada) | Campo `notes` opcional precisa existir/persistir/expor em histórico. Risco médio. | Implementar backend + atualizar swagger |

## Abordagem de Testes

### Testes Unitários

Frontend (Vitest + Testing Library, conforme pasta `frontend/tests`):
- **RBAC/menu**: garantir que usuários sem roles não veem menu/itens de estoque.
- **Helpers de labels**: mapping de enums para PT-BR.
- **Forms**: validação de inputs (ex.: “prazo do banco” date-only, conversão para UTC).

Backend (xUnit existente em `services/stock/5-Tests`):
- Testes de roteamento e status codes para `/api/v1/vehicles` e `/api/v1/test-drives/{id}/complete`.

### Testes de Integração

- Opcional: smoke de endpoints via swagger/curl em ambiente local (quando docker/infra disponíveis).
- Para frontend: testes de integração de páginas com MSW (se o repo já usar) ou mocks de `api`.

## Sequenciamento de Desenvolvimento

### Ordem de Construção

1. **Infra frontend do módulo**: criar estrutura `frontend/src/modules/stock` (routes/layout/pages placeholders) e integrar no `App.tsx`.
2. **Tipos + services + hooks**: implementar camada de dados (`vehicleService`, `reservationService`, `testDriveService` do stock).
3. **Páginas MVP**: 
   - `/stock` visão geral (lista + KPIs básicos)
   - `/stock/vehicles` listagem
   - `/stock/vehicles/{id}` detalhe + histórico
4. **Ações**: reserva, check-in/out, start/complete test-drive, change-status com RBAC.
5. **Tratamento de erros + toasts + estados de loading**.
6. **Testes**: cobrir RBAC, navegação e formulários críticos.

### Dependências Técnicas

- Backend precisa garantir versionamento consistente em `/api/v1` (especialmente `vehicles`).
- Backend precisa implementar `notes` em `CompleteTestDriveRequest` (contrato + persistência + exposição em histórico) para completar a UI de finalização de test-drive.

## Monitoramento e Observabilidade

- Frontend:
  - Logar (console) apenas em modo dev; em produção, preferir mensagens de erro amigáveis.
  - Telemetria não está definida no repo; manter escopo MVP.

- Backend Stock API:
  - Seguir `rules/dotnet-observability.md` (quando aplicável): logs estruturados e status codes consistentes.

## Considerações Técnicas

### Decisões Principais

- **Novo módulo `modules/stock`** em vez de misturar com `commercial` para manter coesão e escalabilidade.
- **RBAC por ocultação** via `navItems` + `Sidebar` + `RequireMenuAccess` no roteamento.
- **TanStack Query** para cache/invalidations e UX responsiva.
- **Compatibilidade de rotas** no backend quando necessário para reduzir risco de quebra.

### Riscos Conhecidos

- **Inconsistência de versionamento**: controllers do Stock API precisam padronizar `/api/v1` (risco de 404 em produção).
- **Enums numéricos**: mapeamento no frontend pode divergir se o backend mudar valores.
- **Campo `notes`**: dependência backend; se não estiver disponível, UI deve ocultar/ignorar campo na finalização.

### Requisitos Especiais

- UX do “prazo do banco” (date-only) com envio `18:00` local → UTC.
- Ações sensíveis (baixas/status manual/finance/preparation) somente para `STOCK_MANAGER`/`MANAGER`/`ADMIN`.

### Conformidade com Padrões

- **Roles**: segue `rules/ROLES_NAMING_CONVENTION.md` (SCREAMING_SNAKE_CASE; claim `roles`).
- **REST**: segue `rules/restful.md` (versionamento via path; paginação `_page/_size`; erro ProblemDetails).
- **Mudanças minimamente invasivas**: reaproveita padrões existentes do frontend (services/hooks/routes) e evita dependências novas.
