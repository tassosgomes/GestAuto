---
status: completed
parallelizable: true
blocked_by: ["10.0", "11.0"]
---

<task_context>
<domain>testing</domain>
<type>testing</type>
<scope>quality</scope>
<complexity>high</complexity>
<dependencies>testcontainers|database|rabbitmq</dependencies>
<unblocks>13.0</unblocks>
</task_context>

# Tarefa 12.0: Implementar Testes de Integração e E2E

## Visão Geral

Implementar testes de integração abrangentes usando Testcontainers para PostgreSQL e RabbitMQ. Cobrir os cenários principais de cada fluxo: Leads, Propostas, Test-Drives, Avaliações e Consumers de eventos.

<requirements>
- Configurar Testcontainers para PostgreSQL e RabbitMQ
- Implementar fixtures compartilhadas entre testes
- Criar testes de integração para repositórios
- Criar testes de integração para API endpoints
- Criar testes E2E para fluxos completos
- Garantir isolamento entre testes
</requirements>

## Subtarefas

### Infraestrutura de Testes

- [x] 12.1 Criar `PostgresFixture` com Testcontainers
- [x] 12.2 Criar `RabbitMqFixture` com Testcontainers
- [x] 12.3 Criar `WebApplicationFactory` customizada para testes de API
- [x] 12.4 Configurar coleções de testes para compartilhar fixtures

### Testes de Repositórios

- [x] 12.5 Criar testes de integração para `LeadRepository`
- [x] 12.6 Criar testes de integração para `ProposalRepository`
- [x] 12.7 Criar testes de integração para `TestDriveRepository`
- [x] 12.8 Criar testes de integração para `OutboxRepository`

### Testes de API - Leads

- [x] 12.9 Testar `POST /api/v1/leads` - criar lead
- [x] 12.10 Testar `GET /api/v1/leads` - listar leads com filtros
- [x] 12.11 Testar `POST /api/v1/leads/{id}/qualify` - qualificar lead
- [x] 12.12 Testar filtro por vendedor (RBAC)

### Testes de API - Propostas

- [x] 12.13 Testar `POST /api/v1/proposals` - criar proposta
- [x] 12.14 Testar `POST /api/v1/proposals/{id}/discount` - aplicar desconto
- [x] 12.15 Testar `POST /api/v1/proposals/{id}/approve-discount` - aprovar desconto (gerente)
- [x] 12.16 Testar `POST /api/v1/proposals/{id}/close` - fechar proposta

### Testes E2E

- [x] 12.17 Testar fluxo completo: Lead → Qualificação → Proposta → Fechamento
- [x] 12.18 Testar fluxo de desconto com aprovação gerencial
- [x] 12.19 Testar fluxo de avaliação de seminovo
- [x] 12.20 Testar processamento de eventos via Outbox

## Sequenciamento

- **Bloqueado por:** 10.0 (Test-Drives), 11.0 (Avaliações/Consumers)
- **Desbloqueia:** 13.0 (Documentação)
- **Paralelizável:** Sim (pode executar junto com 13.0)

## Detalhes de Implementação

### PostgresFixture

```csharp
public class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:18")
        .WithDatabase("commercial_test")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        // Aplicar migrations
        var options = new DbContextOptionsBuilder<CommercialDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        using var context = new CommercialDbContext(options);
        await context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    public CommercialDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CommercialDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        return new CommercialDbContext(options);
    }
}

[CollectionDefinition("Postgres")]
public class PostgresCollection : ICollectionFixture<PostgresFixture> { }
```

### RabbitMqFixture

```csharp
public class RabbitMqFixture : IAsyncLifetime
{
    private readonly RabbitMqContainer _container = new RabbitMqBuilder()
        .WithImage("rabbitmq:4.1-management")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    public string ConnectionString => _container.GetConnectionString();
    public string HostName => _container.Hostname;
    public int Port => _container.GetMappedPublicPort(5672);

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    public IConnection CreateConnection()
    {
        var factory = new ConnectionFactory
        {
            HostName = HostName,
            Port = Port,
            UserName = "test",
            Password = "test"
        };

        return factory.CreateConnection();
    }
}

[CollectionDefinition("RabbitMq")]
public class RabbitMqCollection : ICollectionFixture<RabbitMqFixture> { }
```

### CustomWebApplicationFactory

```csharp
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly PostgresFixture _postgresFixture;
    private readonly RabbitMqFixture _rabbitMqFixture;

    public CustomWebApplicationFactory(
        PostgresFixture postgresFixture, 
        RabbitMqFixture rabbitMqFixture)
    {
        _postgresFixture = postgresFixture;
        _rabbitMqFixture = rabbitMqFixture;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<CommercialDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // Add test DbContext
            services.AddDbContext<CommercialDbContext>(options =>
            {
                options.UseNpgsql(_postgresFixture.ConnectionString);
            });

            // Configure test RabbitMQ
            services.AddSingleton(_rabbitMqFixture.CreateConnection());

            // Mock authentication for tests
            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("SalesPerson", policy => 
                    policy.RequireAuthenticatedUser());
                options.AddPolicy("Manager", policy => 
                    policy.RequireClaim("role", "manager"));
            });
        });
    }
}

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "Test User"),
            new Claim("sub", Guid.NewGuid().ToString()),
            new Claim("sales_person_id", Guid.NewGuid().ToString()),
            new Claim("role", "sales_person")
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
```

### Testes de Integração - LeadRepository

```csharp
[Collection("Postgres")]
public class LeadRepositoryTests : IClassFixture<PostgresFixture>
{
    private readonly PostgresFixture _fixture;

    public LeadRepositoryTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task AddAsync_ShouldPersistLead()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var repository = new LeadRepository(context);

        var lead = Lead.Create(
            "João Silva",
            new Email("joao@email.com"),
            new Phone("11999998888"),
            LeadSource.Showroom,
            Guid.NewGuid()
        );

        // Act
        await repository.AddAsync(lead, CancellationToken.None);
        await context.SaveChangesAsync();

        // Assert
        var savedLead = await repository.GetByIdAsync(lead.Id, CancellationToken.None);
        savedLead.Should().NotBeNull();
        savedLead!.Name.Should().Be("João Silva");
        savedLead.Email.Value.Should().Be("joao@email.com");
    }

    [Fact]
    public async Task ListBySalesPersonAsync_ShouldFilterBySalesPerson()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var repository = new LeadRepository(context);

        var salesPersonId = Guid.NewGuid();
        var otherSalesPersonId = Guid.NewGuid();

        var lead1 = Lead.Create("Lead 1", new Email("lead1@test.com"), new Phone("11999998881"), LeadSource.Google, salesPersonId);
        var lead2 = Lead.Create("Lead 2", new Email("lead2@test.com"), new Phone("11999998882"), LeadSource.Google, salesPersonId);
        var lead3 = Lead.Create("Lead 3", new Email("lead3@test.com"), new Phone("11999998883"), LeadSource.Google, otherSalesPersonId);

        await repository.AddAsync(lead1, CancellationToken.None);
        await repository.AddAsync(lead2, CancellationToken.None);
        await repository.AddAsync(lead3, CancellationToken.None);
        await context.SaveChangesAsync();

        // Act
        var results = await repository.ListBySalesPersonAsync(
            salesPersonId, null, null, 1, 10, CancellationToken.None);

        // Assert
        results.Should().HaveCount(2);
        results.Should().OnlyContain(l => l.SalesPersonId == salesPersonId);
    }

    [Fact]
    public async Task ListBySalesPersonAsync_ShouldFilterByScore()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var repository = new LeadRepository(context);
        var scoringService = new LeadScoringService();

        var salesPersonId = Guid.NewGuid();

        var lead1 = Lead.Create("Lead Gold", new Email("gold@test.com"), new Phone("11999998884"), LeadSource.Showroom, salesPersonId);
        var lead2 = Lead.Create("Lead Bronze", new Email("bronze@test.com"), new Phone("11999998885"), LeadSource.ClassifiedsPortal, salesPersonId);

        // Qualificar lead1 como Gold
        var qualification1 = new Qualification(
            hasTradeInVehicle: false,
            tradeInVehicle: null,
            PaymentMethod.Financing,
            DateTime.UtcNow.AddDays(7),
            false
        );
        lead1.Qualify(qualification1, scoringService);

        await repository.AddAsync(lead1, CancellationToken.None);
        await repository.AddAsync(lead2, CancellationToken.None);
        await context.SaveChangesAsync();

        // Act
        var results = await repository.ListBySalesPersonAsync(
            salesPersonId, null, LeadScore.Gold, 1, 10, CancellationToken.None);

        // Assert
        results.Should().HaveCount(1);
        results.First().Score.Should().Be(LeadScore.Gold);
    }
}
```

### Testes de API - Leads

```csharp
[Collection("Integration")]
public class LeadApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public LeadApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateLead_ShouldReturn201AndLeadResponse()
    {
        // Arrange
        var request = new
        {
            Name = "Maria Santos",
            Email = "maria@email.com",
            Phone = "11999997777",
            Source = "showroom",
            InterestedModel = "Corolla"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/leads", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var lead = await response.Content.ReadFromJsonAsync<LeadResponse>();
        lead.Should().NotBeNull();
        lead!.Name.Should().Be("Maria Santos");
        lead.Status.Should().Be("New");
        lead.Score.Should().Be("Bronze");
    }

    [Fact]
    public async Task QualifyLead_ShouldCalculateCorrectScore()
    {
        // Arrange
        // Primeiro, criar um lead
        var createRequest = new
        {
            Name = "Carlos Diamond",
            Email = "carlos@email.com",
            Phone = "11999996666",
            Source = "showroom"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/v1/leads", createRequest);
        var createdLead = await createResponse.Content.ReadFromJsonAsync<LeadResponse>();

        // Qualificar como Diamante (Financiado + Usado + < 15 dias)
        var qualifyRequest = new
        {
            HasTradeInVehicle = true,
            TradeInVehicle = new
            {
                Brand = "Honda",
                Model = "Civic",
                Year = 2020,
                Mileage = 30000,
                LicensePlate = "ABC1D23",
                Color = "Prata",
                GeneralCondition = "Bom",
                HasDealershipServiceHistory = true
            },
            PaymentMethod = "Financing",
            ExpectedPurchaseDate = DateTime.UtcNow.AddDays(10),
            InterestedInTestDrive = true
        };

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/v1/leads/{createdLead!.Id}/qualify", 
            qualifyRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var qualifiedLead = await response.Content.ReadFromJsonAsync<LeadResponse>();
        qualifiedLead.Should().NotBeNull();
        qualifiedLead!.Score.Should().Be("Diamond");
    }

    [Fact]
    public async Task ListLeads_ShouldReturnPaginatedResults()
    {
        // Arrange - criar alguns leads
        for (int i = 0; i < 5; i++)
        {
            await _client.PostAsJsonAsync("/api/v1/leads", new
            {
                Name = $"Lead {i}",
                Email = $"lead{i}@test.com",
                Phone = $"1199999888{i}",
                Source = "google"
            });
        }

        // Act
        var response = await _client.GetAsync("/api/v1/leads?page=1&pageSize=3");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<LeadListItemResponse>>();
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(3);
        result.HasNextPage.Should().BeTrue();
    }
}
```

### Testes de API - Propostas e Descontos

```csharp
[Collection("Integration")]
public class ProposalApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ProposalApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ApplyDiscount_Above5Percent_ShouldRequireApproval()
    {
        // Arrange
        var leadId = await CreateLeadAsync();
        var proposalId = await CreateProposalAsync(leadId, vehiclePrice: 100000);

        var discountRequest = new
        {
            Amount = 6000, // 6% do valor
            Reason = "Fidelização"
        };

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/v1/proposals/{proposalId}/discount", 
            discountRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var proposal = await response.Content.ReadFromJsonAsync<ProposalResponse>();
        proposal!.Status.Should().Be("AwaitingDiscountApproval");
        proposal.DiscountApproved.Should().BeFalse();
    }

    [Fact]
    public async Task ApproveDiscount_AsSalesPerson_ShouldReturn403()
    {
        // Arrange
        var leadId = await CreateLeadAsync();
        var proposalId = await CreateProposalAsync(leadId, vehiclePrice: 100000);

        await _client.PostAsJsonAsync($"/api/v1/proposals/{proposalId}/discount", new
        {
            Amount = 6000,
            Reason = "Teste"
        });

        // Act - tentar aprovar como vendedor
        var response = await _client.PostAsync(
            $"/api/v1/proposals/{proposalId}/approve-discount", 
            null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CloseProposal_ShouldMarkLeadAsConverted()
    {
        // Arrange
        var leadId = await CreateLeadAsync();
        var proposalId = await CreateProposalAsync(leadId);

        // Act
        var response = await _client.PostAsync(
            $"/api/v1/proposals/{proposalId}/close", 
            null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verificar lead
        var leadResponse = await _client.GetAsync($"/api/v1/leads/{leadId}");
        var lead = await leadResponse.Content.ReadFromJsonAsync<LeadResponse>();
        lead!.Status.Should().Be("Converted");
    }

    private async Task<Guid> CreateLeadAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/leads", new
        {
            Name = "Test Lead",
            Email = $"test{Guid.NewGuid()}@test.com",
            Phone = "11999998888",
            Source = "google"
        });

        var lead = await response.Content.ReadFromJsonAsync<LeadResponse>();
        return lead!.Id;
    }

    private async Task<Guid> CreateProposalAsync(Guid leadId, decimal vehiclePrice = 150000)
    {
        var response = await _client.PostAsJsonAsync("/api/v1/proposals", new
        {
            LeadId = leadId,
            VehicleModel = "Corolla",
            VehicleTrim = "XEi",
            VehicleColor = "Prata",
            VehicleYear = 2025,
            IsReadyDelivery = true,
            VehiclePrice = vehiclePrice,
            PaymentMethod = "Cash"
        });

        var proposal = await response.Content.ReadFromJsonAsync<ProposalResponse>();
        return proposal!.Id;
    }
}
```

### Teste E2E - Fluxo Completo

```csharp
[Collection("Integration")]
public class SalesFlowE2ETests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public SalesFlowE2ETests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CompleteFlow_FromLeadToClosedSale()
    {
        // 1. Criar Lead
        var leadResponse = await _client.PostAsJsonAsync("/api/v1/leads", new
        {
            Name = "Cliente Premium",
            Email = "premium@test.com",
            Phone = "11999995555",
            Source = "showroom",
            InterestedModel = "Camry"
        });
        var lead = await leadResponse.Content.ReadFromJsonAsync<LeadResponse>();
        lead.Should().NotBeNull();
        lead!.Score.Should().Be("Bronze");

        // 2. Qualificar Lead
        var qualifyResponse = await _client.PostAsJsonAsync($"/api/v1/leads/{lead.Id}/qualify", new
        {
            HasTradeInVehicle = true,
            TradeInVehicle = new
            {
                Brand = "Toyota",
                Model = "Corolla",
                Year = 2021,
                Mileage = 25000,
                LicensePlate = "DEF2G34",
                Color = "Branco",
                GeneralCondition = "Excelente",
                HasDealershipServiceHistory = true
            },
            PaymentMethod = "Financing",
            ExpectedPurchaseDate = DateTime.UtcNow.AddDays(5),
            InterestedInTestDrive = true
        });
        var qualifiedLead = await qualifyResponse.Content.ReadFromJsonAsync<LeadResponse>();
        qualifiedLead!.Score.Should().Be("Diamond");

        // 3. Criar Proposta
        var proposalResponse = await _client.PostAsJsonAsync("/api/v1/proposals", new
        {
            LeadId = lead.Id,
            VehicleModel = "Camry",
            VehicleTrim = "XLE V6",
            VehicleColor = "Preto",
            VehicleYear = 2025,
            IsReadyDelivery = true,
            VehiclePrice = 280000,
            PaymentMethod = "Financing",
            DownPayment = 80000,
            Installments = 48
        });
        var proposal = await proposalResponse.Content.ReadFromJsonAsync<ProposalResponse>();
        proposal.Should().NotBeNull();

        // 4. Adicionar itens extras
        await _client.PostAsJsonAsync($"/api/v1/proposals/{proposal!.Id}/items", new
        {
            Description = "Película de proteção solar",
            Value = 1500
        });

        // 5. Fechar venda
        var closeResponse = await _client.PostAsync($"/api/v1/proposals/{proposal.Id}/close", null);
        closeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var closedProposal = await closeResponse.Content.ReadFromJsonAsync<ProposalResponse>();
        closedProposal!.Status.Should().Be("Closed");

        // 6. Verificar que lead foi convertido
        var finalLeadResponse = await _client.GetAsync($"/api/v1/leads/{lead.Id}");
        var finalLead = await finalLeadResponse.Content.ReadFromJsonAsync<LeadResponse>();
        finalLead!.Status.Should().Be("Converted");
    }
}
```

## Critérios de Sucesso

- [x] Testcontainers inicializam corretamente PostgreSQL e RabbitMQ
- [x] Migrations são aplicadas automaticamente nos testes
- [x] Testes de repositório cobrem operações CRUD e queries
- [x] Testes de API validam autenticação e autorização
- [x] Testes de API validam respostas e status codes
- [x] Teste E2E cobre fluxo completo de venda
- [ ] Cobertura de código > 80% em handlers e entidades (não verificado)
- [x] Todos os testes são isolados (não interferem entre si)
- [ ] CI/CD executa testes automaticamente (não configurado - fora do escopo)
