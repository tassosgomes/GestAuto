---
status: pending
parallelizable: true
blocked_by: ["1.0"]
---

<task_context>
<domain>backend/stock</domain>
<type>testing</type>
<scope>core_feature</scope>
<complexity>medium</complexity>
<dependencies>database, http_server</dependencies>
<unblocks>""</unblocks>
</task_context>

# Tarefa 2.0: Implementar Testes de Integração para Fluxo Completo

## Visão Geral

Criar testes de integração que validam o fluxo completo de criação de veículo seguido de check-in, utilizando banco de dados real (PostgreSQL via Testcontainers ou fixture). O foco principal é o cenário de seminovo, que possui mais regras de negócio.

<requirements>
- Testar fluxo completo: POST /vehicles → POST /vehicles/{id}/check-ins
- Validar que veículo é criado com dados corretos
- Validar que check-in atualiza status para `InStock`
- Testar cenário de erro: VIN duplicado retorna 400 com ProblemDetails
- Usar PostgresFixture para banco de dados real
- Autenticação deve estar configurada no cliente HTTP
</requirements>

## Subtarefas

- [ ] 2.1 Configurar `PostgresFixture` se não existir
- [ ] 2.2 Criar arquivo `VehiclesControllerIntegrationTests.cs` em `GestAuto.Stock.IntegrationTest/Controllers/`
- [ ] 2.3 Implementar teste `CreateAndCheckIn_UsedVehicle_FullFlow_ReturnsSuccess`
- [ ] 2.4 Implementar teste `CreateVehicle_DuplicateVin_Returns400WithProblemDetails`
- [ ] 2.5 Implementar teste `CreateVehicle_NewCategory_WithManufacturerOrigin_ReturnsSuccess`
- [ ] 2.6 Implementar teste `CheckIn_WithInvalidVehicleId_Returns404`
- [ ] 2.7 Executar todos os testes de integração e garantir aprovação

## Sequenciamento

- **Bloqueado por**: 1.0 (Testes Unitários)
- **Desbloqueia**: Nenhum
- **Paralelizável**: Sim — pode executar em paralelo com tarefas de frontend

## Detalhes de Implementação

### Estrutura do Teste de Integração

```csharp
// GestAuto.Stock.IntegrationTest/Controllers/VehiclesControllerIntegrationTests.cs
namespace GestAuto.Stock.IntegrationTest.Controllers;

public class VehiclesControllerIntegrationTests : IClassFixture<PostgresFixture>
{
    private readonly HttpClient _client;
    private readonly PostgresFixture _fixture;

    public VehiclesControllerIntegrationTests(PostgresFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.CreateAuthenticatedClient();
    }
}
```

### Cenário Principal: Seminovo Completo

```csharp
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
        EvaluationId: Guid.NewGuid(),
        DemoPurpose: null,
        IsRegistered: false);

    // Act - Criar veículo
    var createResponse = await _client.PostAsJsonAsync("/api/v1/vehicles", createRequest);
    
    // Assert - Criação
    createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
    var vehicle = await createResponse.Content.ReadFromJsonAsync<VehicleResponse>();
    vehicle.Should().NotBeNull();
    vehicle!.Category.Should().Be(VehicleCategory.Used);

    // Act - Check-in
    var checkInRequest = new CheckInCreateRequest(
        Source: CheckInSource.CustomerUsedPurchase,
        OccurredAt: DateTime.UtcNow,
        Notes: "Veículo em bom estado");

    var checkInResponse = await _client.PostAsJsonAsync(
        $"/api/v1/vehicles/{vehicle.Id}/check-ins", 
        checkInRequest);

    // Assert - Check-in
    checkInResponse.StatusCode.Should().Be(HttpStatusCode.Created);
    var checkIn = await checkInResponse.Content.ReadFromJsonAsync<CheckInResponse>();
    checkIn.Should().NotBeNull();
    checkIn!.CurrentStatus.Should().Be(VehicleStatus.InStock);
}
```

### Configuração de Autenticação

O cliente HTTP deve incluir token JWT válido com roles necessárias (`STOCK_PERSON`).

## Critérios de Sucesso

- [ ] Todos os 4+ testes de integração passando
- [ ] Banco de dados PostgreSQL real utilizado
- [ ] Testes isolados (cada um com seu próprio estado)
- [ ] Tempo de execução < 30 segundos por teste
- [ ] ProblemDetails validado em cenários de erro
