# Tech Spec — Cadastro de Veículo + Check-in (MVP)

## Resumo Executivo

Esta especificação técnica detalha a implementação do fluxo unificado de cadastro de veículo com check-in no módulo de estoque. A abordagem utiliza os endpoints existentes (`POST /vehicles` + `POST /vehicles/{id}/check-ins`) em sequência orquestrada pelo frontend, com uma nova página dedicada que oferece formulário dinâmico baseado em categoria/origem. O backend já possui toda a lógica de domínio necessária; as principais entregas são: (1) nova página de frontend com UX unificada, (2) validação opcional de `EvaluationId` via chamada ao serviço `vehicle-evaluation`, e (3) testes de integração focados no cenário de seminovo.

A arquitetura segue o padrão Clean Architecture já estabelecido no projeto, com CQRS nativo (sem MediatR), garantindo separação clara entre comandos, queries e domínio.

## Arquitetura do Sistema

### Visão Geral dos Componentes

```
┌─────────────────────────────────────────────────────────────────────────┐
│                              FRONTEND                                    │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │           VehicleCheckInPage (Nova Página)                       │   │
│  │  ┌──────────────┐  ┌──────────────┐  ┌────────────────────┐    │   │
│  │  │OriginSelector│→│CategorySelect│→│DynamicVehicleForm  │    │   │
│  │  └──────────────┘  └──────────────┘  └────────────────────┘    │   │
│  │                              ↓                                   │   │
│  │                    vehicleService.ts                             │   │
│  └─────────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────────┘
                                    │
                    ┌───────────────┴───────────────┐
                    ▼                               ▼
┌──────────────────────────────┐    ┌──────────────────────────────┐
│      GestAuto.Stock.API      │    │   vehicle-evaluation (Java)  │
│  ┌────────────────────────┐  │    │  ┌────────────────────────┐  │
│  │  VehiclesController    │  │    │  │ VehicleEvaluationCtrl  │  │
│  │  POST /vehicles        │  │    │  │ GET /evaluations/{id}  │  │
│  │  POST /{id}/check-ins  │  │    │  └────────────────────────┘  │
│  └────────────────────────┘  │    └──────────────────────────────┘
│            │                  │
│            ▼                  │
│  ┌────────────────────────┐  │
│  │ CreateVehicleHandler   │  │
│  │ CreateCheckInHandler   │  │
│  └────────────────────────┘  │
│            │                  │
│            ▼                  │
│  ┌────────────────────────┐  │
│  │     Vehicle (Domain)   │  │
│  │  - ValidateCategoryReq │  │
│  │  - CheckIn()           │  │
│  └────────────────────────┘  │
└──────────────────────────────┘
```

**Componentes e Responsabilidades:**

| Componente | Responsabilidade |
|------------|------------------|
| `VehicleCheckInPage` | Nova página de frontend - fluxo unificado de cadastro + check-in |
| `OriginSelector` | Seleção de origem via cards (montadora, compra, transferência, frota) |
| `CategorySelector` | Seleção de categoria (novo, seminovo, demonstração) |
| `DynamicVehicleForm` | Formulário com campos dinâmicos baseados em categoria |
| `vehicleService.ts` | Orquestração de chamadas: create → check-in |
| `CreateVehicleHandler` | Handler existente - criação de veículo |
| `CreateCheckInHandler` | Handler existente - registro de entrada |
| `Vehicle` | Entidade de domínio com validações por categoria |

## Design de Implementação

### Interfaces Principais

#### Frontend - Novo Service Method

```typescript
// frontend/src/modules/stock/services/vehicleService.ts
export interface CreateVehicleWithCheckInRequest {
  // Dados do veículo
  category: VehicleCategory;
  vin: string;
  make: string;
  model: string;
  yearModel: number;
  color: string;
  plate?: string;
  trim?: string;
  mileageKm?: number;
  evaluationId?: string;
  demoPurpose?: DemoPurpose;
  isRegistered?: boolean;
  // Dados do check-in
  source: CheckInSource;
  notes?: string;
}

export interface CreateVehicleWithCheckInResponse {
  vehicle: VehicleResponse;
  checkIn: CheckInResponse;
}
```

#### Backend - Validação de Avaliação (Opcional)

```csharp
// GestAuto.Stock.Domain/Interfaces/IEvaluationValidator.cs
public interface IEvaluationValidator
{
    Task<bool> ExistsAsync(Guid evaluationId, CancellationToken cancellationToken);
}
```

### Modelos de Dados

#### DTOs Existentes (Sem Alteração)

O modelo `VehicleCreate` já suporta todos os campos necessários:

```csharp
// Existente em GestAuto.Stock.Application/Vehicles/Dto/VehicleCreate.cs
public sealed record VehicleCreate(
    VehicleCategory Category,
    string Vin,
    string Make,
    string Model,
    int YearModel,
    string Color,
    string? Plate,
    string? Trim,
    int? MileageKm,
    Guid? EvaluationId,      // Usado para seminovos
    DemoPurpose? DemoPurpose, // Usado para demonstração
    bool IsRegistered);       // Usado para demonstração
```

#### Mapeamento Origem → Categoria (Frontend)

```typescript
// Regras de negócio para auto-seleção
const ORIGIN_CATEGORY_MAP: Record<CheckInSource, VehicleCategory[]> = {
  [CheckInSource.Manufacturer]: [VehicleCategory.New],
  [CheckInSource.CustomerUsedPurchase]: [VehicleCategory.Used],
  [CheckInSource.StoreTransfer]: [VehicleCategory.New, VehicleCategory.Used, VehicleCategory.Demonstration],
  [CheckInSource.InternalFleet]: [VehicleCategory.Demonstration],
};
```

### Endpoints de API

#### Endpoints Existentes (Reutilizados)

| Método | Path | Descrição |
|--------|------|-----------|
| `POST` | `/api/v1/vehicles` | Cadastro de veículo |
| `POST` | `/api/v1/vehicles/{id}/check-ins` | Registro de check-in |
| `GET` | `/api/v1/vehicles/{id}` | Consulta de veículo (confirmação) |

#### Endpoint Externo (vehicle-evaluation)

| Método | Path | Descrição |
|--------|------|-----------|
| `GET` | `/api/v1/evaluations/{id}` | Validação de existência de avaliação |

**Nota**: A validação de `EvaluationId` para seminovos é feita no frontend antes de submeter, consultando o serviço de avaliações. Isso evita acoplamento direto entre os serviços de backend.

### Fluxo de Orquestração (Frontend)

```typescript
// frontend/src/modules/stock/services/vehicleService.ts
async createVehicleWithCheckIn(
  request: CreateVehicleWithCheckInRequest
): Promise<CreateVehicleWithCheckInResponse> {
  // 1. Se seminovo, validar EvaluationId existe
  if (request.category === VehicleCategory.Used && request.evaluationId) {
    const evaluationExists = await this.validateEvaluationExists(request.evaluationId);
    if (!evaluationExists) {
      throw new Error('Avaliação não encontrada. Verifique o ID informado.');
    }
  }

  // 2. Criar veículo
  const vehicle = await this.create(request);

  // 3. Registrar check-in
  const checkIn = await this.checkIn(vehicle.id, {
    source: request.source,
    occurredAt: new Date().toISOString(),
    notes: request.notes,
  });

  return { vehicle, checkIn };
}

private async validateEvaluationExists(evaluationId: string): Promise<boolean> {
  try {
    await evaluationApi.get(`/evaluations/${evaluationId}`);
    return true;
  } catch (error) {
    if (isAxiosError(error) && error.response?.status === 404) {
      return false;
    }
    throw error;
  }
}
```

## Pontos de Integração

### Integração com vehicle-evaluation

| Aspecto | Especificação |
|---------|---------------|
| Serviço | `vehicle-evaluation` (Java/Spring Boot) |
| Endpoint | `GET /api/v1/evaluations/{id}` |
| Autenticação | Bearer Token (mesmo Keycloak) |
| Tratamento de Erro | 404 → avaliação inexistente; outros → propagar erro |
| Timeout | 5 segundos (fail-fast) |
| Fallback | Permitir cadastro sem validação (log warning) |

**Importante**: A validação de avaliação é feita no **frontend** para manter o desacoplamento entre serviços de backend. Se o serviço de avaliação estiver indisponível, o fluxo pode prosseguir com um warning ao usuário.

## Análise de Impacto

| Componente Afetado | Tipo de Impacto | Descrição & Nível de Risco | Ação Requerida |
|--------------------|-----------------|----------------------------|----------------|
| `VehiclesController` | Nenhum | Endpoints existentes são reutilizados. Sem risco. | Nenhuma |
| `CreateVehicleCommandHandler` | Nenhum | Validações por categoria já implementadas. Sem risco. | Nenhuma |
| `Vehicle` (Domain) | Nenhum | Validações de check-in por categoria já existem. Sem risco. | Nenhuma |
| `vehicleService.ts` | Adição | Novo método `createVehicleWithCheckIn`. Baixo risco. | Adicionar método |
| `stock/pages/` | Adição | Nova página `VehicleCheckInPage`. Baixo risco. | Criar página |
| `stock/routes.tsx` | Modificação | Adicionar rota `/check-in`. Baixo risco. | Adicionar rota |
| `lib/api.ts` | Possível adição | Instância axios para `vehicle-evaluation`. Baixo risco. | Verificar/adicionar |

## Abordagem de Testes

### Testes Unitários

**Foco: Cenário de seminovo (mais regras de negócio)**

```csharp
// GestAuto.Stock.UnitTest/Application/CreateVehicleCommandHandlerTests.cs
public class CreateVehicleCommandHandlerTests
{
    [Fact]
    public async Task Handle_UsedVehicleWithoutPlate_ThrowsDomainException()
    {
        // Arrange
        var request = new VehicleCreate(
            Category: VehicleCategory.Used,
            Vin: "ABC123",
            Make: "Honda",
            Model: "Civic",
            YearModel: 2023,
            Color: "Preto",
            Plate: null,  // Faltando
            Trim: null,
            MileageKm: 50000,
            EvaluationId: Guid.NewGuid(),
            DemoPurpose: null,
            IsRegistered: false);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(() => 
            _handler.HandleAsync(new CreateVehicleCommand(userId, request), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_UsedVehicleWithoutEvaluationId_ThrowsDomainException()
    {
        // Similar - EvaluationId obrigatório para seminovos
    }

    [Fact]
    public async Task Handle_UsedVehicleWithAllRequiredFields_CreatesVehicleSuccessfully()
    {
        // Cenário positivo completo
    }
}
```

**Cenários críticos a testar:**

| Categoria | Cenário | Resultado Esperado |
|-----------|---------|-------------------|
| Novo | VIN ausente | `DomainException` |
| Novo | Origem diferente de Montadora | `DomainException` no check-in |
| Seminovo | Placa ausente | `DomainException` |
| Seminovo | MileageKm ausente | `DomainException` |
| Seminovo | EvaluationId ausente | `DomainException` |
| Seminovo | Todos campos válidos | Sucesso |
| Demonstração | DemoPurpose ausente | `DomainException` |
| Demonstração | IsRegistered=true sem Plate | `DomainException` |
| Qualquer | VIN duplicado | `DomainException` |

### Testes de Integração

**Foco: Fluxo completo de criação + check-in para seminovo**

```csharp
// GestAuto.Stock.IntegrationTest/Controllers/VehiclesControllerIntegrationTests.cs
public class VehiclesControllerIntegrationTests : IClassFixture<PostgresFixture>
{
    [Fact]
    public async Task CreateAndCheckIn_UsedVehicle_FullFlow_ReturnsSuccess()
    {
        // Arrange
        var createRequest = new VehicleCreate(
            Category: VehicleCategory.Used,
            Vin: $"VIN{Guid.NewGuid():N}".Substring(0, 17),
            Make: "Toyota",
            Model: "Corolla",
            YearModel: 2022,
            Color: "Prata",
            Plate: "ABC1234",
            Trim: "XEi",
            MileageKm: 35000,
            EvaluationId: Guid.NewGuid(), // Assumindo avaliação existe
            DemoPurpose: null,
            IsRegistered: false);

        // Act - Criar veículo
        var createResponse = await _client.PostAsJsonAsync("/api/v1/vehicles", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var vehicle = await createResponse.Content.ReadFromJsonAsync<VehicleResponse>();

        // Act - Check-in
        var checkInRequest = new CheckInCreateRequest(
            Source: CheckInSource.CustomerUsedPurchase,
            OccurredAt: DateTime.UtcNow,
            Notes: "Veículo em bom estado");

        var checkInResponse = await _client.PostAsJsonAsync(
            $"/api/v1/vehicles/{vehicle!.Id}/check-ins", 
            checkInRequest);

        // Assert
        checkInResponse.EnsureSuccessStatusCode();
        var checkIn = await checkInResponse.Content.ReadFromJsonAsync<CheckInResponse>();

        checkIn.Should().NotBeNull();
        checkIn!.CurrentStatus.Should().Be(VehicleStatus.InStock);
    }

    [Fact]
    public async Task CreateVehicle_DuplicateVin_Returns400WithProblemDetails()
    {
        // Teste de idempotência/duplicidade
    }
}
```

### Testes de Frontend (Vitest)

```typescript
// frontend/tests/vehicle-checkin-page.test.tsx
describe('VehicleCheckInPage', () => {
  it('should disable submit when required fields are missing for Used category', async () => {
    render(<VehicleCheckInPage />);
    
    // Selecionar origem e categoria
    await userEvent.click(screen.getByText('Compra cliente/seminovo'));
    
    // Tentar submeter sem preencher campos obrigatórios
    const submitButton = screen.getByRole('button', { name: /registrar entrada/i });
    expect(submitButton).toBeDisabled();
  });

  it('should show evaluation validation error when evaluationId is invalid', async () => {
    // Mock da API de avaliação retornando 404
    server.use(
      rest.get('/api/v1/evaluations/:id', (req, res, ctx) => {
        return res(ctx.status(404));
      })
    );

    // ... preencher formulário com evaluationId inválido
    // ... verificar mensagem de erro
  });
});
```

## Sequenciamento de Desenvolvimento

### Ordem de Construção

| Fase | Entrega | Justificativa | Estimativa |
|------|---------|---------------|------------|
| 1 | Testes unitários para cenário seminovo | Garantir validações antes de UI | 2h |
| 2 | Nova página `VehicleCheckInPage` (estrutura) | Base para desenvolvimento frontend | 3h |
| 3 | Componentes: `OriginSelector`, `CategorySelector` | Seleção de contexto | 2h |
| 4 | `DynamicVehicleForm` com campos por categoria | Formulário principal | 4h |
| 5 | Integração com `vehicleService.createVehicleWithCheckIn` | Orquestração de chamadas | 2h |
| 6 | Validação de `EvaluationId` (chamada ao vehicle-evaluation) | Regra de seminovo | 1h |
| 7 | Testes de integração E2E | Validação completa | 3h |
| 8 | Acessibilidade e refinamentos de UX | Conformidade WCAG | 2h |

**Total estimado: ~19h de desenvolvimento**

### Dependências Técnicas

| Dependência | Status | Ação |
|-------------|--------|------|
| `POST /vehicles` | ✅ Disponível | Nenhuma |
| `POST /vehicles/{id}/check-ins` | ✅ Disponível | Nenhuma |
| `GET /evaluations/{id}` | ✅ Disponível | Nenhuma |
| Axios instance para vehicle-evaluation | ⚠️ Verificar | Criar se não existir |
| shadcn/ui components | ✅ Disponível | Nenhuma |

## Monitoramento e Observabilidade

### Métricas a Expor

| Métrica | Tipo | Labels |
|---------|------|--------|
| `gestauto_vehicle_created_total` | Counter | `category`, `origin` |
| `gestauto_checkin_completed_total` | Counter | `category`, `source` |
| `gestauto_checkin_duration_seconds` | Histogram | `category` |
| `gestauto_evaluation_validation_total` | Counter | `result` (success/not_found/error) |

### Logs Principais

```json
{
  "level": "info",
  "message": "Vehicle created with check-in",
  "vehicleId": "uuid",
  "category": "Used",
  "source": "CustomerUsedPurchase",
  "userId": "uuid",
  "durationMs": 245
}
```

### Alertas Sugeridos

| Alerta | Condição | Severidade |
|--------|----------|------------|
| Check-in sem avaliação (seminovo) | Rate > 0 em 5min | Warning |
| Falha na validação de avaliação | Error rate > 10% em 5min | Warning |
| Duplicidade de VIN | Ocorrências > 5 em 1h | Info |

## Considerações Técnicas

### Decisões Principais

| Decisão | Justificativa | Alternativas Rejeitadas |
|---------|---------------|------------------------|
| Orquestração no frontend | Mantém backend desacoplado; endpoints já existem | Endpoint atômico `/vehicles/with-checkin` (mais complexo, menos flexível) |
| Validação de avaliação no frontend | Evita dependência síncrona entre serviços de backend | Chamada backend-to-backend (acoplamento desnecessário) |
| Nova página dedicada | UX mais clara; não sobrecarrega página existente | Extensão da `StockMovementsPage` (UI complexa demais) |
| Formulário dinâmico por categoria | Evita campos desnecessários; reduz erros | Formulário único com todos os campos (confuso) |

### Riscos Conhecidos

| Risco | Probabilidade | Impacto | Mitigação |
|-------|---------------|---------|-----------|
| Serviço de avaliação indisponível | Baixa | Médio | Fallback com warning; permitir prosseguir |
| Inconsistência em caso de falha no check-in | Baixa | Alto | Retry automático; UI mostra estado parcial |
| Latência na validação de avaliação | Média | Baixo | Timeout de 5s; feedback visual de loading |

### Requisitos Especiais

**Performance:**
- Tempo total de cadastro + check-in: ≤ 2 segundos (excluindo input do usuário)
- Validação de duplicidade de VIN: ≤ 100ms

**Segurança:**
- Todos os endpoints requerem autenticação
- RBAC: `STOCK_PERSON`, `STOCK_MANAGER`, `MANAGER`, `ADMIN`

**Acessibilidade:**
- Labels em todos os campos
- Mensagens de erro com `aria-live="polite"`
- Navegação por teclado completa
- Contraste WCAG 2.1 AA

### Conformidade com Padrões

| Padrão | Conformidade | Notas |
|--------|--------------|-------|
| `dotnet-architecture.md` | ✅ | Clean Architecture, CQRS nativo |
| `dotnet-coding-standards.md` | ✅ | Records imutáveis, async/await |
| `dotnet-testing.md` | ✅ | xUnit, AwesomeAssertions, AAA pattern |
| `restful.md` | ✅ | Verbos HTTP corretos, ProblemDetails |

---

## Tarefas de Implementação

### Backend (GestAuto.Stock)

- [ ] **TASK-BE-01**: Adicionar testes unitários para cenário de seminovo em `CreateVehicleCommandHandlerTests`
- [ ] **TASK-BE-02**: Adicionar testes de integração para fluxo completo de criação + check-in
- [ ] **TASK-BE-03**: (Opcional) Implementar `IEvaluationValidator` para validação backend-to-backend futura

### Frontend

- [ ] **TASK-FE-01**: Criar página `VehicleCheckInPage` em `frontend/src/modules/stock/pages/`
- [ ] **TASK-FE-02**: Implementar componente `OriginSelector` (cards clicáveis)
- [ ] **TASK-FE-03**: Implementar componente `CategorySelector` (tabs/botões)
- [ ] **TASK-FE-04**: Implementar `DynamicVehicleForm` com campos condicionais por categoria
- [ ] **TASK-FE-05**: Adicionar método `createVehicleWithCheckIn` em `vehicleService.ts`
- [ ] **TASK-FE-06**: Adicionar validação de `EvaluationId` via chamada ao vehicle-evaluation
- [ ] **TASK-FE-07**: Configurar instância axios para API de avaliações (se não existir)
- [ ] **TASK-FE-08**: Adicionar rota `/stock/check-in` em `routes.tsx`
- [ ] **TASK-FE-09**: Criar testes Vitest para `VehicleCheckInPage`
- [ ] **TASK-FE-10**: Garantir acessibilidade (labels, aria-live, navegação por teclado)

### Documentação

- [ ] **TASK-DOC-01**: Atualizar README do módulo de estoque com novo fluxo

---

## Questões Abertas

| # | Questão | Responsável | Status |
|---|---------|-------------|--------|
| 1 | Timeout padrão para chamada ao vehicle-evaluation (sugestão: 5s) | Tech Lead | Pendente |
| 2 | Comportamento quando avaliação não existe: bloquear ou warning? | PO | **Assumido: warning + permitir** |
| 3 | Necessidade de endpoint `HEAD /evaluations/{id}` para otimização | Tech Lead | Não prioritário |
