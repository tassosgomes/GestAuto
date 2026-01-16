# Tech Spec - Módulo Comercial GestAuto

## Resumo Executivo

Esta especificação técnica define a arquitetura e implementação do **Módulo Comercial** do GestAuto, um microserviço isolado responsável pelo fluxo de vendas em concessionárias de veículos.

A solução será construída em **.NET 8** seguindo **Clean Architecture** com **CQRS nativo** (sem MediatR). O microserviço expõe APIs REST documentadas via **OpenAPI/Swagger**, publica e consome eventos via **RabbitMQ** com documentação **AsyncAPI**, utiliza **PostgreSQL** como banco de dados com **Entity Framework Core** para acesso a dados, e implementa **Outbox Pattern** para garantia transacional de eventos. Autenticação via **Logto** com RBAC para Vendedor e Gerente.

## Arquitetura do Sistema

### Visão Geral dos Componentes

```
┌─────────────────────────────────────────────────────────────────────┐
│                        GestAuto.Commercial                          │
├─────────────────────────────────────────────────────────────────────┤
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐                  │
│  │   API REST  │  │  Consumers  │  │  Outbox     │                  │
│  │  (Swagger)  │  │ (RabbitMQ)  │  │  Processor  │                  │
│  └──────┬──────┘  └──────┬──────┘  └──────┬──────┘                  │
│         │                │                │                         │
│         ▼                ▼                ▼                         │
│  ┌─────────────────────────────────────────────────────────────┐    │
│  │                    Application Layer                        │    │
│  │  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐     │    │
│  │  │ Commands │  │ Queries  │  │Validators│  │  DTOs    │     │    │
│  │  └──────────┘  └──────────┘  └──────────┘  └──────────┘     │    │
│  └─────────────────────────────────────────────────────────────┘    │
│         │                                                           │
│         ▼                                                           │
│  ┌─────────────────────────────────────────────────────────────┐    │
│  │                      Domain Layer                           │    │
│  │  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐     │    │
│  │  │ Entities │  │ Services │  │  Events  │  │Interfaces│     │    │
│  │  └──────────┘  └──────────┘  └──────────┘  └──────────┘     │    │
│  └─────────────────────────────────────────────────────────────┘    │
│         │                                                           │
│         ▼                                                           │
│  ┌─────────────────────────────────────────────────────────────┐    │
│  │                      Infra Layer                            │    │
│  │  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐     │    │
│  │  │  Repos   │  │ Messaging│  │  Outbox  │  │   Auth   │     │    │
│  │  │ (EFCore) │  │(RabbitMQ)│  │  (PG)    │  │ (Logto)  │     │    │
│  │  └──────────┘  └──────────┘  └──────────┘  └──────────┘     │    │
│  └─────────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────────┘
         │                │                │
         ▼                ▼                ▼
    ┌──────────┐     ┌─────────┐      ┌─────────┐
    │PostgreSQL│     │RabbitMQ │      │  Logto  │
    └──────────┘     └─────────┘      └─────────┘
```

### Componentes Principais

| Componente | Responsabilidade |
|------------|------------------|
| **API REST** | Endpoints HTTP para operações síncronas (CRUD, consultas) |
| **Consumers** | Consumidores de eventos RabbitMQ (respostas de avaliação, atualizações financeiro) |
| **Outbox Processor** | Background service que processa a tabela outbox e publica eventos |
| **Application** | Commands, Queries, Handlers, Validators, DTOs |
| **Domain** | Entities, Aggregates, Domain Events, Domain Services, Interfaces |
| **Infra** | Repositórios (EF Core), Messaging (RabbitMQ), Auth (Logto) |

## Design de Implementação

### Estrutura de Pastas

```
GestAuto.Commercial/
├── GestAuto.Commercial.sln
├── docker-compose.yml
├── 1-Services/
│   └── GestAuto.Commercial.API/
│       ├── Controller/
│       │   ├── LeadController.cs
│       │   ├── ProposalController.cs
│       │   ├── TestDriveController.cs
│       │   └── EvaluationController.cs
│       ├── Consumer/
│       │   ├── UsedVehicleEvaluationRespondedConsumer.cs
│       │   └── OrderUpdatedConsumer.cs
│       ├── BackgroundService/
│       │   └── OutboxProcessorService.cs
│       ├── Middleware/
│       │   └── TenantMiddleware.cs
│       └── Program.cs
├── 2-Application/
│   └── GestAuto.Commercial.Application/
│       ├── Lead/
│       │   ├── Command/
│       │   │   ├── CreateLeadCommand.cs
│       │   │   ├── CreateLeadHandler.cs
│       │   │   ├── QualifyLeadCommand.cs
│       │   │   └── QualifyLeadHandler.cs
│       │   ├── Query/
│       │   │   ├── GetLeadQuery.cs
│       │   │   ├── GetLeadHandler.cs
│       │   │   ├── ListLeadsQuery.cs
│       │   │   └── ListLeadsHandler.cs
│       │   └── Validator/
│       │       └── CreateLeadValidator.cs
│       ├── Proposal/
│       ├── TestDrive/
│       ├── Evaluation/
│       ├── Common/
│       │   ├── Interface/
│       │   │   ├── ICommandHandler.cs
│       │   │   └── IQueryHandler.cs
│       │   └── Behavior/
│       └── DTO/
├── 3-Domain/
│   └── GestAuto.Commercial.Domain/
│       ├── Entity/
│       │   ├── Lead.cs
│       │   ├── Qualification.cs
│       │   ├── Proposal.cs
│       │   ├── ProposalItem.cs
│       │   ├── TestDrive.cs
│       │   ├── UsedVehicle.cs
│       │   ├── UsedVehicleEvaluation.cs
│       │   ├── Interaction.cs
│       │   └── Order.cs
│       ├── ValueObject/
│       │   ├── Email.cs
│       │   ├── Phone.cs
│       │   ├── Money.cs
│       │   └── LicensePlate.cs
│       ├── Enum/
│       │   ├── LeadStatus.cs
│       │   ├── LeadScore.cs
│       │   ├── LeadSource.cs
│       │   ├── PaymentMethod.cs
│       │   ├── ProposalStatus.cs
│       │   └── OrderStatus.cs
│       ├── Event/
│       │   ├── LeadCreatedEvent.cs
│       │   ├── LeadScoredEvent.cs
│       │   ├── ProposalCreatedEvent.cs
│       │   ├── SaleClosedEvent.cs
│       │   └── UsedVehicleEvaluationRequestedEvent.cs
│       ├── Service/
│       │   └── LeadScoringService.cs
│       └── Interface/
│           ├── ILeadRepository.cs
│           ├── IProposalRepository.cs
│           ├── ITestDriveRepository.cs
│           ├── IEventPublisher.cs
│           └── IUnitOfWork.cs
├── 4-Infra/
│   └── GestAuto.Commercial.Infra/
│       ├── Repository/
│       │   ├── LeadRepository.cs
│       │   ├── ProposalRepository.cs
│       │   └── TestDriveRepository.cs
│       ├── Messaging/
│       │   ├── RabbitMqPublisher.cs
│       │   └── Configuration/
│       │       └── RabbitMqConfiguration.cs
│       ├── Outbox/
│       │   ├── OutboxMessage.cs
│       │   └── OutboxRepository.cs
│       ├── Auth/
│       │   └── LogtoAuthHandler.cs
│       ├── Persistence/
│       │   ├── CommercialDbContext.cs
│       │   └── UnitOfWork.cs
│       └── Migration/
└── 5-Tests/
    ├── GestAuto.Commercial.UnitTest/
    │   ├── GestAuto.Commercial.UnitTest.csproj
    │   ├── Domain/
    │   │   ├── LeadTests.cs
    │   │   ├── ProposalTests.cs
    │   │   └── LeadScoringServiceTests.cs
    │   └── Application/
    │       └── Handler/
    ├── GestAuto.Commercial.IntegrationTest/
    │   ├── GestAuto.Commercial.IntegrationTest.csproj
    │   ├── Repository/
    │   └── Fixture/
    │       └── PostgresFixture.cs
    └── GestAuto.Commercial.End2EndTest/
        └── GestAuto.Commercial.End2EndTest.csproj
```

### Interfaces Principais

```csharp
// CQRS - Interfaces Base
public interface ICommand<TResponse> { }

public interface IQuery<TResponse> { }

public interface ICommandHandler<TCommand, TResponse> 
    where TCommand : ICommand<TResponse>
{
    Task<TResponse> HandleAsync(TCommand command, CancellationToken cancellationToken);
}

public interface IQueryHandler<TQuery, TResponse> 
    where TQuery : IQuery<TResponse>
{
    Task<TResponse> HandleAsync(TQuery query, CancellationToken cancellationToken);
}

// Unit of Work with Outbox (EF Core)
public interface IUnitOfWork : IDisposable
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    Task BeginTransactionAsync(CancellationToken cancellationToken);
    Task CommitAsync(CancellationToken cancellationToken);
    Task RollbackAsync(CancellationToken cancellationToken);
    void AddDomainEvent(IDomainEvent domainEvent);
}

// Event Publisher
public interface IEventPublisher
{
    Task PublishAsync<T>(T domainEvent, CancellationToken cancellationToken) where T : IDomainEvent;
}

// Base Repository
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken);
    Task UpdateAsync(T entity, CancellationToken cancellationToken);
}
```

### Modelos de Dados

#### Domain Entities

```csharp
// Lead - Aggregate Root
public class Lead
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public Email Email { get; private set; }
    public Phone Phone { get; private set; }
    public LeadSource Source { get; private set; }
    public LeadStatus Status { get; private set; }
    public LeadScore Score { get; private set; }
    public Guid SalesPersonId { get; private set; }
    public Qualification? Qualification { get; private set; }
    public List<Interaction> Interactions { get; private set; } = new();
    
    // Optional interest fields
    public string? InterestedModel { get; private set; }
    public string? InterestedTrim { get; private set; }
    public string? InterestedColor { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public static Lead Create(string name, Email email, Phone phone, 
        LeadSource source, Guid salesPersonId)
    {
        var lead = new Lead
        {
            Id = Guid.NewGuid(),
            Name = name,
            Email = email,
            Phone = phone,
            Source = source,
            Status = LeadStatus.New,
            Score = LeadScore.Bronze,
            SalesPersonId = salesPersonId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        lead.AddEvent(new LeadCreatedEvent(lead.Id, lead.Name, lead.SalesPersonId));
        return lead;
    }

    public void Qualify(Qualification qualification, LeadScoringService scoringService)
    {
        Qualification = qualification;
        Score = scoringService.Calculate(this);
        UpdatedAt = DateTime.UtcNow;
        
        AddEvent(new LeadScoredEvent(Id, Score));
    }
}

// Qualification - Complex Value Object
public class Qualification
{
    public bool HasTradeInVehicle { get; private set; }
    public UsedVehicle? TradeInVehicle { get; private set; }
    public PaymentMethod PaymentMethod { get; private set; }
    public DateTime? IdealPurchaseDate { get; private set; }
    public bool InterestedInTestDrive { get; private set; }
    
    public int DaysUntilPurchase => IdealPurchaseDate.HasValue 
        ? (IdealPurchaseDate.Value - DateTime.UtcNow).Days 
        : int.MaxValue;
}

// Proposal - Aggregate Root
public class Proposal
{
    public Guid Id { get; private set; }
    public Guid LeadId { get; private set; }
    public Guid SalesPersonId { get; private set; }
    public ProposalStatus Status { get; private set; }
    
    // Vehicle
    public string Model { get; private set; }
    public string Trim { get; private set; }
    public string Color { get; private set; }
    public int Year { get; private set; }
    public bool ReadyForDelivery { get; private set; }
    public Money VehiclePrice { get; private set; }
    
    // Items and Values
    public List<ProposalItem> ExtraItems { get; private set; } = new();
    public Money DiscountAmount { get; private set; }
    public string? DiscountReason { get; private set; }
    public Guid? DiscountApproverId { get; private set; }
    
    // Trade-in
    public UsedVehicleEvaluation? TradeInEvaluation { get; private set; }
    
    // Payment
    public PaymentMethod PaymentMethod { get; private set; }
    public Money? DownPayment { get; private set; }
    public int? NumberOfInstallments { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public Money TotalValue => CalculateTotalValue();
    
    public decimal DiscountPercentage => VehiclePrice.Value > 0 
        ? (DiscountAmount.Value / VehiclePrice.Value) * 100 
        : 0;

    public bool RequiresDiscountApproval => DiscountPercentage > 5;

    public void ApplyDiscount(Money amount, string reason, Guid? approverId = null)
    {
        if (Status == ProposalStatus.Closed)
            throw new InvalidOperationException("Cannot modify closed proposal");

        DiscountAmount = amount;
        DiscountReason = reason;

        if (DiscountPercentage > 5)
        {
            if (approverId == null)
            {
                Status = ProposalStatus.AwaitingDiscountApproval;
                return;
            }
            DiscountApproverId = approverId;
        }
        
        UpdatedAt = DateTime.UtcNow;
    }

    public void Close()
    {
        ValidateForClosure();
        Status = ProposalStatus.Closed;
        UpdatedAt = DateTime.UtcNow;
        
        AddEvent(new SaleClosedEvent(Id, LeadId, TotalValue));
    }
}

// Domain Service - Lead Scoring
public class LeadScoringService
{
    public LeadScore Calculate(Lead lead)
    {
        if (lead.Qualification == null)
            return LeadScore.Bronze;

        var q = lead.Qualification;
        var baseScore = CalculateBaseScore(q);
        var bonus = CalculateBonus(lead, q);
        
        return ApplyBonus(baseScore, bonus);
    }

    private LeadScore CalculateBaseScore(Qualification q)
    {
        var hasFinancing = q.PaymentMethod == PaymentMethod.Financing;
        var hasTradeIn = q.HasTradeInVehicle;
        var nearPurchase = q.DaysUntilPurchase <= 15;
        var cashPayment = q.PaymentMethod == PaymentMethod.Cash;

        // Diamond: Financing + Trade-in + Purchase < 15 days
        if (hasFinancing && hasTradeIn && nearPurchase)
            return LeadScore.Diamond;

        // Gold: (Cash + Trade-in) OR (Financing) + Purchase < 15 days
        if (((cashPayment && hasTradeIn) || hasFinancing) && nearPurchase)
            return LeadScore.Gold;

        // Silver: Pure cash payment
        if (cashPayment && !hasTradeIn)
            return LeadScore.Silver;

        // Bronze: Purchase > 30 days or no info
        return LeadScore.Bronze;
    }

    private int CalculateBonus(Lead lead, Qualification q)
    {
        var bonus = 0;
        
        // Hot source: +1
        if (lead.Source == LeadSource.Showroom || lead.Source == LeadSource.Phone)
            bonus++;
        
        // Premium trade-in: +1
        if (q.TradeInVehicle?.IsPremium() == true)
            bonus++;
        
        // TODO: Stale inventory model: +1 (requires inventory integration)
        
        return bonus;
    }

    private LeadScore ApplyBonus(LeadScore current, int bonus)
    {
        var level = (int)current - bonus;
        return (LeadScore)Math.Max(level, (int)LeadScore.Diamond);
    }
}
```

#### Esquema de Banco de Dados

```sql
-- Leads
CREATE TABLE leads (
    id UUID PRIMARY KEY,
    name VARCHAR(200) NOT NULL,
    email VARCHAR(200) NOT NULL,
    phone VARCHAR(20) NOT NULL,
    source VARCHAR(50) NOT NULL,
    status VARCHAR(50) NOT NULL,
    score VARCHAR(20) NOT NULL,
    sales_person_id UUID NOT NULL,
    interested_model VARCHAR(100),
    interested_trim VARCHAR(100),
    interested_color VARCHAR(50),
    created_at TIMESTAMP NOT NULL,
    updated_at TIMESTAMP NOT NULL
);

-- Qualifications
CREATE TABLE qualifications (
    id UUID PRIMARY KEY,
    lead_id UUID NOT NULL REFERENCES leads(id),
    has_trade_in_vehicle BOOLEAN NOT NULL,
    payment_method VARCHAR(50) NOT NULL,
    ideal_purchase_date DATE,
    interested_in_test_drive BOOLEAN NOT NULL,
    created_at TIMESTAMP NOT NULL,
    UNIQUE(lead_id)
);

-- Used Vehicles (for trade-in)
CREATE TABLE used_vehicles (
    id UUID PRIMARY KEY,
    qualification_id UUID REFERENCES qualifications(id),
    proposal_id UUID REFERENCES proposals(id),
    brand VARCHAR(100) NOT NULL,
    model VARCHAR(100) NOT NULL,
    year INTEGER NOT NULL,
    mileage INTEGER NOT NULL,
    license_plate VARCHAR(10),
    color VARCHAR(50),
    serviced_at_dealer BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP NOT NULL
);

-- Interactions (history)
CREATE TABLE interactions (
    id UUID PRIMARY KEY,
    lead_id UUID NOT NULL REFERENCES leads(id),
    type VARCHAR(50) NOT NULL,
    channel VARCHAR(50),
    description TEXT,
    outcome VARCHAR(100),
    created_at TIMESTAMP NOT NULL,
    created_by UUID NOT NULL
);

-- Proposals
CREATE TABLE proposals (
    id UUID PRIMARY KEY,
    lead_id UUID NOT NULL REFERENCES leads(id),
    sales_person_id UUID NOT NULL,
    status VARCHAR(50) NOT NULL,
    model VARCHAR(100) NOT NULL,
    trim VARCHAR(100) NOT NULL,
    color VARCHAR(50) NOT NULL,
    year INTEGER NOT NULL,
    ready_for_delivery BOOLEAN NOT NULL,
    vehicle_price DECIMAL(15,2) NOT NULL,
    discount_amount DECIMAL(15,2) DEFAULT 0,
    discount_reason TEXT,
    discount_approver_id UUID,
    payment_method VARCHAR(50) NOT NULL,
    down_payment DECIMAL(15,2),
    number_of_installments INTEGER,
    created_at TIMESTAMP NOT NULL,
    updated_at TIMESTAMP NOT NULL
);

-- Proposal Items
CREATE TABLE proposal_items (
    id UUID PRIMARY KEY,
    proposal_id UUID NOT NULL REFERENCES proposals(id),
    description VARCHAR(200) NOT NULL,
    value DECIMAL(15,2) NOT NULL
);

-- Used Vehicle Evaluations
CREATE TABLE used_vehicle_evaluations (
    id UUID PRIMARY KEY,
    proposal_id UUID NOT NULL REFERENCES proposals(id),
    used_vehicle_id UUID NOT NULL REFERENCES used_vehicles(id),
    status VARCHAR(50) NOT NULL,
    evaluated_value DECIMAL(15,2),
    evaluator_id UUID,
    customer_accepted BOOLEAN,
    created_at TIMESTAMP NOT NULL,
    responded_at TIMESTAMP
);

-- Test Drives
CREATE TABLE test_drives (
    id UUID PRIMARY KEY,
    lead_id UUID NOT NULL REFERENCES leads(id),
    sales_person_id UUID NOT NULL,
    vehicle_id UUID NOT NULL,
    scheduled_date TIMESTAMP NOT NULL,
    completed_date TIMESTAMP,
    status VARCHAR(50) NOT NULL,
    starting_mileage INTEGER,
    ending_mileage INTEGER,
    starting_fuel_level VARCHAR(20),
    ending_fuel_level VARCHAR(20),
    notes TEXT,
    created_at TIMESTAMP NOT NULL
);

-- Orders (post-sale tracking)
CREATE TABLE orders (
    id UUID PRIMARY KEY,
    proposal_id UUID NOT NULL REFERENCES proposals(id),
    status VARCHAR(50) NOT NULL,
    estimated_arrival DATE,
    notes TEXT,
    created_at TIMESTAMP NOT NULL,
    updated_at TIMESTAMP NOT NULL
);

-- Outbox (transactional messaging)
CREATE TABLE outbox_messages (
    id UUID PRIMARY KEY,
    event_type VARCHAR(200) NOT NULL,
    payload JSONB NOT NULL,
    created_at TIMESTAMP NOT NULL,
    processed_at TIMESTAMP,
    attempts INTEGER DEFAULT 0,
    error TEXT
);

CREATE INDEX idx_outbox_pending ON outbox_messages(created_at) 
    WHERE processed_at IS NULL;

-- Audit
CREATE TABLE audit (
    id UUID PRIMARY KEY,
    entity VARCHAR(100) NOT NULL,
    entity_id UUID NOT NULL,
    action VARCHAR(50) NOT NULL,
    previous_data JSONB,
    new_data JSONB,
    user_id UUID NOT NULL,
    created_at TIMESTAMP NOT NULL
);

CREATE INDEX idx_leads_sales_person ON leads(sales_person_id);
CREATE INDEX idx_leads_status ON leads(status);
CREATE INDEX idx_leads_score ON leads(score);
CREATE INDEX idx_proposals_lead ON proposals(lead_id);
CREATE INDEX idx_proposals_status ON proposals(status);
CREATE INDEX idx_audit_entity ON audit(entity, entity_id);
```

### Endpoints de API

#### Leads API

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| `POST` | `/api/v1/leads` | Create new lead |
| `GET` | `/api/v1/leads` | List leads (paginated, filters) |
| `GET` | `/api/v1/leads/{id}` | Get lead by ID |
| `PUT` | `/api/v1/leads/{id}` | Update lead |
| `PATCH` | `/api/v1/leads/{id}/status` | Change lead status |
| `POST` | `/api/v1/leads/{id}/qualify` | Qualify lead |
| `POST` | `/api/v1/leads/{id}/interactions` | Register interaction |
| `GET` | `/api/v1/leads/{id}/interactions` | List interactions |

#### Propostas API

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| `POST` | `/api/v1/proposals` | Create proposal |
| `GET` | `/api/v1/proposals` | List proposals |
| `GET` | `/api/v1/proposals/{id}` | Get proposal |
| `PUT` | `/api/v1/proposals/{id}` | Update proposal |
| `POST` | `/api/v1/proposals/{id}/items` | Add extra item |
| `DELETE` | `/api/v1/proposals/{id}/items/{itemId}` | Remove item |
| `POST` | `/api/v1/proposals/{id}/discount` | Apply discount |
| `POST` | `/api/v1/proposals/{id}/approve-discount` | Approve discount (manager) |
| `POST` | `/api/v1/proposals/{id}/close` | Close proposal |

#### Test Drives API

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| `POST` | `/api/v1/test-drives` | Schedule test-drive |
| `GET` | `/api/v1/test-drives` | List test-drives |
| `GET` | `/api/v1/test-drives/{id}` | Get test-drive |
| `POST` | `/api/v1/test-drives/{id}/complete` | Register completion |
| `POST` | `/api/v1/test-drives/{id}/cancel` | Cancel scheduling |

#### Avaliações API

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| `POST` | `/api/v1/used-vehicle-evaluations` | Request evaluation |
| `GET` | `/api/v1/used-vehicle-evaluations` | List evaluations |
| `GET` | `/api/v1/used-vehicle-evaluations/{id}` | Get evaluation |
| `POST` | `/api/v1/used-vehicle-evaluations/{id}/customer-response` | Register acceptance/rejection |

#### Orders API

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| `GET` | `/api/v1/orders` | List orders |
| `GET` | `/api/v1/orders/{id}` | Get order |
| `POST` | `/api/v1/orders/{id}/notes` | Add note |

## Pontos de Integração

### Published Events (AsyncAPI)

```yaml
asyncapi: '2.6.0'
info:
  title: GestAuto Commercial API
  version: '1.0.0'
channels:
  commercial.lead.created:
    publish:
      message:
        payload:
          type: object
          properties:
            id:
              type: string
              format: uuid
            name:
              type: string
            email:
              type: string
            salesPersonId:
              type: string
              format: uuid
            source:
              type: string
            occurredAt:
              type: string
              format: date-time
              
  commercial.lead.scored:
    publish:
      message:
        payload:
          type: object
          properties:
            leadId:
              type: string
              format: uuid
            score:
              type: string
              enum: [Diamond, Gold, Silver, Bronze]
            previousScore:
              type: string
            occurredAt:
              type: string
              format: date-time

  commercial.proposal.created:
    publish:
      message:
        payload:
          type: object
          properties:
            id:
              type: string
              format: uuid
            leadId:
              type: string
              format: uuid
            salesPersonId:
              type: string
              format: uuid
            totalValue:
              type: number
            occurredAt:
              type: string
              format: date-time

  commercial.sale.closed:
    publish:
      message:
        payload:
          type: object
          properties:
            proposalId:
              type: string
              format: uuid
            leadId:
              type: string
              format: uuid
            salesPersonId:
              type: string
              format: uuid
            totalValue:
              type: number
            paymentMethod:
              type: string
            hasTradeInVehicle:
              type: boolean
            occurredAt:
              type: string
              format: date-time

  used-vehicles.evaluation.requested:
    publish:
      message:
        payload:
          type: object
          properties:
            evaluationId:
              type: string
              format: uuid
            proposalId:
              type: string
              format: uuid
            usedVehicle:
              type: object
              properties:
                brand:
                  type: string
                model:
                  type: string
                year:
                  type: integer
                mileage:
                  type: integer
                licensePlate:
                  type: string
            occurredAt:
              type: string
              format: date-time

  commercial.test-drive.scheduled:
    publish:
      message:
        payload:
          type: object
          properties:
            testDriveId:
              type: string
              format: uuid
            leadId:
              type: string
              format: uuid
            vehicleId:
              type: string
              format: uuid
            scheduledDate:
              type: string
              format: date-time
            occurredAt:
              type: string
              format: date-time
```

### Consumed Events

| Exchange | Routing Key | Description | Action |
|----------|-------------|-----------|------|
| `used-vehicles` | `evaluation.responded` | Used vehicle evaluation completed | Updates proposal with value |
| `finance` | `order.updated` | Order status changed | Updates tracking |

### Autenticação (Logto)

```csharp
// Configuration in Program.cs
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Logto:Authority"];
        options.Audience = builder.Configuration["Logto:Audience"];
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };
    });

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SalesPerson", policy => 
        policy.RequireClaim("role", "salesperson", "manager"));
    
    options.AddPolicy("Manager", policy => 
        policy.RequireClaim("role", "manager"));
});

// Data filter by sales person
public class SalesPersonFilterService
{
    private readonly IHttpContextAccessor _contextAccessor;

    public Guid? GetCurrentSalesPersonId()
    {
        var user = _contextAccessor.HttpContext?.User;
        var salesPersonId = user?.FindFirst("sub")?.Value;
        return salesPersonId != null ? Guid.Parse(salesPersonId) : null;
    }

    public bool IsManager()
    {
        var user = _contextAccessor.HttpContext?.User;
        return user?.HasClaim("role", "manager") ?? false;
    }
}
```

## Análise de Impacto

| Componente Afetado | Tipo de Impacto | Descrição & Nível de Risco | Ação Requerida |
|-------------------|-----------------|---------------------------|----------------|
| **RabbitMQ** | Infraestrutura Nova | Provisionar exchanges e queues. Baixo risco. | Criar docker-compose |
| **PostgreSQL** | Infraestrutura Nova | Novo schema de banco. Baixo risco (greenfield). | Executar migrations |
| **Logto** | Integração Externa | Dependência de autenticação. Médio risco. | Configurar tenant e roles |
| **Módulo Seminovos** | Integração Futura | Consumirá eventos de avaliação. Baixo risco. | Documentar contratos |
| **Módulo Financeiro** | Integração Futura | Consumirá evento VendaFechada. Baixo risco. | Documentar contratos |

## Abordagem de Testes

### Unit Tests

**Components to test:**
- Domain entities (Lead, Proposal, TestDrive)
- Lead scoring service
- Value Objects (Email, Phone, Money)
- Command and Query handlers
- FluentValidation validators

**Critical scenarios:**
```csharp
// Lead Scoring Service Test
[Theory]
[InlineData(PaymentMethod.Financing, true, 10, LeadScore.Diamond)]
[InlineData(PaymentMethod.Cash, true, 10, LeadScore.Gold)]
[InlineData(PaymentMethod.Financing, false, 10, LeadScore.Gold)]
[InlineData(PaymentMethod.Cash, false, 10, LeadScore.Silver)]
[InlineData(PaymentMethod.Cash, false, 45, LeadScore.Bronze)]
public void Calculate_ShouldReturnCorrectScore(
    PaymentMethod paymentMethod, bool hasTradeIn, int daysUntilPurchase, LeadScore expected)
{
    // Arrange
    var service = new LeadScoringService();
    var lead = CreateLeadWithQualification(paymentMethod, hasTradeIn, daysUntilPurchase);
    
    // Act
    var result = service.Calculate(lead);
    
    // Assert
    result.Should().Be(expected);
}

// Discount rule test
[Fact]
public void ApplyDiscount_Above5Percent_ShouldRequireApproval()
{
    // Arrange
    var proposal = CreateProposal(vehiclePrice: 100000);
    
    // Act
    proposal.ApplyDiscount(new Money(6000), "Negotiation"); // 6%
    
    // Assert
    proposal.Status.Should().Be(ProposalStatus.AwaitingDiscountApproval);
    proposal.DiscountApproverId.Should().BeNull();
}
```

### Integration Tests

**Technology:** Testcontainers (PostgreSQL + RabbitMQ)

```csharp
public class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        await RunMigrations();
    }

    public async Task DisposeAsync() => await _container.DisposeAsync();
}

[Collection("Postgres")]
public class LeadRepositoryTests : IClassFixture<PostgresFixture>
{
    private readonly PostgresFixture _fixture;

    [Fact]
    public async Task AddAsync_ShouldInsertLead()
    {
        // Arrange
        await using var dbContext = _fixture.CreateDbContext();
        var repo = new LeadRepository(dbContext);
        var lead = Lead.Create("John", new Email("john@test.com"), 
            new Phone("11999999999"), LeadSource.Showroom, Guid.NewGuid());
        
        // Act
        await repo.AddAsync(lead, CancellationToken.None);
        await dbContext.SaveChangesAsync();
        var retrieved = await repo.GetByIdAsync(lead.Id, CancellationToken.None);
        
        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("John");
    }
}
```

## Development Sequencing

### Build Order

| Phase | Component | Rationale | Duration |
|------|-----------|---------------|----------|
| **1** | Base infrastructure | Docker-compose, migrations, structured project | 2 days |
| **2** | Domain Layer | Entities, Value Objects, Domain Services | 3 days |
| **3** | Infra - Repositories | EF Core + PostgreSQL | 2 days |
| **4** | Application - Leads | Commands, Queries, Handlers for Leads | 3 days |
| **5** | API - Leads | Controllers, validation, auth | 2 days |
| **6** | Application - Proposals | Commands, Queries for Proposals | 3 days |
| **7** | API - Proposals | Controllers and discount rules | 2 days |
| **8** | Messaging - Outbox | Outbox pattern + RabbitMQ publisher | 2 days |
| **9** | Test-Drives & Evaluations | Secondary flows | 3 days |
| **10** | Consumers | External event consumers | 2 days |
| **11** | Integration Tests | Testcontainers + E2E scenarios | 3 days |
| **12** | Documentation | OpenAPI + AsyncAPI | 1 day |

**Estimated total:** ~28 days (5-6 weeks)

### Technical Dependencies

| Dependency | Status | Blocking? |
|-------------|--------|-------------|
| PostgreSQL (Docker) | To provision | Yes |
| RabbitMQ (Docker) | To provision | Yes |
| Logto | To configure | Partial (can mock) |
| AsyncAPI Contracts Used Vehicles | To define | No (can simulate) |

## Monitoring and Observability

### Metrics (Prometheus)

```csharp
// Custom metrics
public static class CommercialMetrics
{
    public static readonly Counter LeadsCreated = Metrics.CreateCounter(
        "gestauto_commercial_leads_created_total",
        "Total leads created",
        new CounterConfiguration { LabelNames = new[] { "source" } });

    public static readonly Counter ProposalsClosed = Metrics.CreateCounter(
        "gestauto_commercial_proposals_closed_total",
        "Total proposals closed",
        new CounterConfiguration { LabelNames = new[] { "payment_method" } });

    public static readonly Histogram LeadScoringDuration = Metrics.CreateHistogram(
        "gestauto_commercial_lead_scoring_seconds",
        "Time to score lead");

    public static readonly Gauge LeadsByScore = Metrics.CreateGauge(
        "gestauto_commercial_leads_by_score",
        "Number of leads by score",
        new GaugeConfiguration { LabelNames = new[] { "score" } });
}
```

### Structured Logs (Serilog + ECS)

```csharp
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .WriteTo.Console(new EcsTextFormatter())
    .CreateLogger();

// Log example
_logger.LogInformation(
    "Lead {LeadId} scored as {Score} by sales person {SalesPersonId}",
    lead.Id, score, salesPersonId);
```

### Health Checks

```csharp
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString, name: "postgres")
    .AddRabbitMQ(rabbitConnectionString, name: "rabbitmq");

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
```

## Technical Considerations

### Key Decisions

| Decision | Rationale | Rejected Alternatives |
|---------|---------------|------------------------|
| **Native CQRS** | Simplicity, no external dependency | MediatR (licensing, overhead) |
| **Entity Framework Core** | Productivity, migrations, project standard | Dapper (more verbose, no migrations) |
| **Outbox Pattern** | Transactional guarantee for events | Direct publishing (can lose events) |
| **PostgreSQL** | Robust, JSONB for auditing, defined standard | Oracle (cost), SQL Server |
| **Testcontainers** | Realistic tests, isolation | InMemory (doesn't represent production) |

### Known Risks

| Risk | Probability | Impact | Mitigation |
|-------|--------------|---------|-----------|
| Logto latency | Medium | Medium | Token cache, circuit breaker |
| Event volume | Low | High | Monitor outbox, scale processor |
| Scoring rules | Medium | Low | Extensive tests, feature flag |

### Standards Compliance

- ✅ Clean Architecture conforme `dotnet-architecture.md`
- ✅ CQRS nativo (sem MediatR) conforme `dotnet-architecture.md`
- ✅ English naming convention conforme `dotnet-coding-standards.md`
- ✅ Entity Framework Core + PostgreSQL conforme `dotnet-libraries-config.md`
- ✅ Serilog + ECS conforme `dotnet-observability.md`
- ✅ xUnit + Testcontainers conforme `dotnet-testing.md`
- ✅ REST + OpenAPI conforme `restful.md`
- ✅ Folder structure conforme `dotnet-folders.md`

---

**Document created on:** 08/12/2025  
**Version:** 1.0  
**Status:** Approved for implementation
