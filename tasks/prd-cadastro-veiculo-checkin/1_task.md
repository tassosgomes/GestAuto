---
status: pending
parallelizable: true
blocked_by: []
---

<task_context>
<domain>backend/stock</domain>
<type>testing</type>
<scope>core_feature</scope>
<complexity>medium</complexity>
<dependencies>database</dependencies>
<unblocks>"2.0"</unblocks>
</task_context>

# Tarefa 1.0: Implementar Testes Unitários para Cenário de Seminovo

## Visão Geral

Criar testes unitários focados no cenário de cadastro de veículo seminovo, que possui as regras de negócio mais complexas (placa obrigatória, quilometragem obrigatória, avaliação vinculada). Os testes garantem que as validações de domínio funcionam corretamente antes de desenvolver a UI.

<requirements>
- Testar validação de campos obrigatórios por categoria (novo, seminovo, demonstração)
- Testar cenário de seminovo sem placa → DomainException
- Testar cenário de seminovo sem MileageKm → DomainException
- Testar cenário de seminovo sem EvaluationId → DomainException
- Testar cenário de seminovo com todos os campos → Sucesso
- Testar duplicidade de VIN → DomainException
- Seguir padrão AAA (Arrange-Act-Assert)
</requirements>

## Subtarefas

- [ ] 1.1 Criar arquivo `CreateVehicleCommandHandlerTests.cs` em `GestAuto.Stock.UnitTest/Application/`
- [ ] 1.2 Implementar teste `Handle_UsedVehicleWithoutPlate_ThrowsDomainException`
- [ ] 1.3 Implementar teste `Handle_UsedVehicleWithoutMileageKm_ThrowsDomainException`
- [ ] 1.4 Implementar teste `Handle_UsedVehicleWithoutEvaluationId_ThrowsDomainException`
- [ ] 1.5 Implementar teste `Handle_UsedVehicleWithAllRequiredFields_CreatesVehicleSuccessfully`
- [ ] 1.6 Implementar teste `Handle_NewVehicleWithoutVin_ThrowsDomainException`
- [ ] 1.7 Implementar teste `Handle_DemonstrationVehicleWithoutDemoPurpose_ThrowsDomainException`
- [ ] 1.8 Implementar teste `Handle_DuplicateVin_ThrowsDomainException`
- [ ] 1.9 Executar todos os testes e garantir 100% de aprovação

## Sequenciamento

- **Bloqueado por**: Nenhum
- **Desbloqueia**: 2.0 (Testes de Integração)
- **Paralelizável**: Sim — pode executar em paralelo com todas as tarefas de frontend

## Detalhes de Implementação

### Estrutura do Arquivo de Teste

```csharp
// GestAuto.Stock.UnitTest/Application/CreateVehicleCommandHandlerTests.cs
namespace GestAuto.Stock.UnitTest.Application;

public class CreateVehicleCommandHandlerTests
{
    private readonly CreateVehicleCommandHandler _handler;
    private readonly Mock<IVehicleRepository> _vehicleRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    public CreateVehicleCommandHandlerTests()
    {
        _vehicleRepositoryMock = new Mock<IVehicleRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new CreateVehicleCommandHandler(
            _vehicleRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }
}
```

### Cenários de Teste por Categoria

| Categoria | Campo Testado | Cenário | Resultado Esperado |
|-----------|---------------|---------|-------------------|
| Novo | VIN | Ausente | `DomainException` |
| Novo | Origem | Diferente de Montadora | `DomainException` no check-in |
| Seminovo | Placa | Ausente | `DomainException` |
| Seminovo | MileageKm | Ausente | `DomainException` |
| Seminovo | EvaluationId | Ausente | `DomainException` |
| Seminovo | Todos | Válidos | Sucesso |
| Demonstração | DemoPurpose | Ausente | `DomainException` |
| Demonstração | IsRegistered + Plate | true sem Plate | `DomainException` |
| Qualquer | VIN | Duplicado | `DomainException` |

### Dependências de Pacotes

- xUnit (framework de teste)
- Moq (mocking)
- AwesomeAssertions (assertivas fluentes)

## Critérios de Sucesso

- [ ] Todos os 8+ testes implementados e passando
- [ ] Cobertura de 100% das validações de categoria
- [ ] Padrão AAA seguido em todos os testes
- [ ] Nomenclatura clara: `Method_Scenario_ExpectedResult`
- [ ] Testes executam em < 5 segundos no total
