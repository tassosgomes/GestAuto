---
status: completed
parallelizable: true
blocked_by: ["10.0", "11.0"]
completed_at: "2025-12-09T18:45:00Z"
reviewed_by: "GitHub Copilot"
---

<task_context>
<domain>documentation</domain>
<type>documentation</type>
<scope>api_documentation</scope>
<complexity>medium</complexity>
<dependencies>swagger|asyncapi</dependencies>
<unblocks>none</unblocks>
</task_context>

# Tarefa 13.0: Criar Documentação OpenAPI e AsyncAPI

## Visão Geral

Criar documentação completa das APIs REST (OpenAPI/Swagger) e dos eventos assíncronos (AsyncAPI). A documentação deve ser auto-gerada a partir do código e complementada com exemplos e descrições detalhadas.

<requirements>
- Configurar Swagger/OpenAPI com documentação completa
- Criar especificação AsyncAPI para eventos RabbitMQ
- Adicionar exemplos de request/response em todos os endpoints
- Documentar códigos de erro e mensagens
- Gerar documentação em formato exportável (JSON/YAML)
</requirements>

## Subtarefas

### OpenAPI/Swagger

- [x] 13.1 Configurar Swagger com autenticação JWT ✅ CONCLUÍDO
- [x] 13.2 Adicionar descrições detalhadas em todos os endpoints ✅ CONCLUÍDO
- [x] 13.3 Adicionar exemplos de request para cada endpoint ✅ CONCLUÍDO
- [x] 13.4 Adicionar exemplos de response para cada endpoint ✅ CONCLUÍDO
- [x] 13.5 Documentar todos os códigos de status HTTP ✅ CONCLUÍDO
- [x] 13.6 Documentar schemas de DTOs com descrições de campos ✅ CONCLUÍDO
- [x] 13.7 Criar grupos de endpoints por domínio (Leads, Proposals, etc.) ✅ CONCLUÍDO
- [x] 13.8 Gerar arquivo openapi.json/yaml ✅ CONCLUÍDO

### AsyncAPI

- [x] 13.9 Criar especificação AsyncAPI base ✅ CONCLUÍDO
- [x] 13.10 Documentar eventos publicados (LeadCreated, SaleClosed, etc.) ✅ CONCLUÍDO
- [x] 13.11 Documentar eventos consumidos (EvaluationResponded, OrderUpdated) ✅ CONCLUÍDO
- [x] 13.12 Especificar schemas de payload dos eventos ✅ CONCLUÍDO
- [x] 13.13 Documentar exchanges, routing keys e bindings ✅ CONCLUÍDO
- [x] 13.14 Gerar arquivo asyncapi.yaml ✅ CONCLUÍDO

### Documentação Complementar

- [x] 13.15 Criar README do projeto com instruções de setup ✅ CONCLUÍDO
- [x] 13.16 Documentar variáveis de ambiente ✅ CONCLUÍDO
- [x] 13.17 Criar guia de desenvolvimento para novos contribuidores ✅ CONCLUÍDO

## Sequenciamento

- **Bloqueado por:** 10.0 (Test-Drives), 11.0 (Avaliações/Consumers)
- **Desbloqueia:** Nenhuma tarefa
- **Paralelizável:** Sim (pode executar junto com 12.0)

## Detalhes de Implementação

### Configuração Swagger Completa

```csharp
// Program.cs
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "GestAuto Commercial API",
        Version = "v1",
        Description = @"
API do Módulo Comercial do GestAuto - Sistema de Gestão para Concessionárias de Veículos.

## Funcionalidades Principais

- **Gestão de Leads**: Cadastro, qualificação e acompanhamento de potenciais clientes
- **Propostas Comerciais**: Criação e gestão de propostas de venda
- **Test-Drives**: Agendamento e controle de test-drives
- **Avaliações de Seminovos**: Solicitação e acompanhamento de avaliações

## Autenticação

Esta API utiliza autenticação JWT via Logto. Inclua o token no header:

```
Authorization: Bearer <token>
```

## Roles e Permissões

- **Vendedor (sales_person)**: Acesso aos próprios leads e propostas
- **Gerente (manager)**: Acesso a todos os registros + aprovação de descontos

## Códigos de Status

- `200 OK`: Requisição bem-sucedida
- `201 Created`: Recurso criado com sucesso
- `400 Bad Request`: Dados inválidos ou erro de negócio
- `401 Unauthorized`: Token não fornecido ou inválido
- `403 Forbidden`: Sem permissão para o recurso
- `404 Not Found`: Recurso não encontrado
- `500 Internal Server Error`: Erro interno do servidor
",
        Contact = new OpenApiContact
        {
            Name = "GestAuto Team",
            Email = "suporte@gestauto.com.br"
        },
        License = new OpenApiLicense
        {
            Name = "Proprietary"
        }
    });

    // JWT Authentication
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando Bearer scheme. Exemplo: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // XML Comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);

    // Swagger Examples
    options.ExampleFilters();

    // Tags/Groups
    options.TagActionsBy(api =>
    {
        if (api.GroupName != null)
            return new[] { api.GroupName };

        if (api.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
            return new[] { controllerActionDescriptor.ControllerName };

        return new[] { "Other" };
    });

    options.DocInclusionPredicate((name, api) => true);
});

builder.Services.AddSwaggerExamplesFromAssemblyOf<Program>();
```

### Exemplos de Request/Response

```csharp
// Exemplos para Leads
public class CreateLeadRequestExample : IExamplesProvider<CreateLeadRequest>
{
    public CreateLeadRequest GetExamples()
    {
        return new CreateLeadRequest(
            Name: "Maria Santos",
            Email: "maria.santos@email.com",
            Phone: "(11) 99999-8888",
            Source: "showroom",
            InterestedModel: "Toyota Corolla",
            InterestedTrim: "XEi 2.0 Flex",
            InterestedColor: "Prata Metálico"
        );
    }
}

public class LeadResponseExample : IExamplesProvider<LeadResponse>
{
    public LeadResponse GetExamples()
    {
        return new LeadResponse(
            Id: Guid.Parse("550e8400-e29b-41d4-a716-446655440000"),
            Name: "Maria Santos",
            Email: "maria.santos@email.com",
            Phone: "(11) 99999-8888",
            Source: "Showroom",
            Status: "New",
            Score: "Bronze",
            SalesPersonId: Guid.Parse("660e8400-e29b-41d4-a716-446655440001"),
            InterestedModel: "Toyota Corolla",
            InterestedTrim: "XEi 2.0 Flex",
            InterestedColor: "Prata Metálico",
            Qualification: null,
            CreatedAt: DateTime.Parse("2025-01-15T10:30:00Z"),
            UpdatedAt: DateTime.Parse("2025-01-15T10:30:00Z")
        );
    }
}

public class QualifyLeadRequestExample : IExamplesProvider<QualifyLeadRequest>
{
    public QualifyLeadRequest GetExamples()
    {
        return new QualifyLeadRequest(
            HasTradeInVehicle: true,
            TradeInVehicle: new TradeInVehicleDto(
                Brand: "Honda",
                Model: "Civic",
                Year: 2020,
                Mileage: 35000,
                LicensePlate: "ABC1D23",
                Color: "Branco",
                GeneralCondition: "Bom estado, sem avarias",
                HasDealershipServiceHistory: true
            ),
            PaymentMethod: "Financing",
            ExpectedPurchaseDate: DateTime.Parse("2025-01-30"),
            InterestedInTestDrive: true
        );
    }
}

public class LeadQualifiedResponseExample : IExamplesProvider<LeadResponse>
{
    public LeadResponse GetExamples()
    {
        return new LeadResponse(
            Id: Guid.Parse("550e8400-e29b-41d4-a716-446655440000"),
            Name: "Maria Santos",
            Email: "maria.santos@email.com",
            Phone: "(11) 99999-8888",
            Source: "Showroom",
            Status: "InNegotiation",
            Score: "Diamond", // Score calculado automaticamente
            SalesPersonId: Guid.Parse("660e8400-e29b-41d4-a716-446655440001"),
            InterestedModel: "Toyota Corolla",
            InterestedTrim: "XEi 2.0 Flex",
            InterestedColor: "Prata Metálico",
            Qualification: new QualificationResponse(
                HasTradeInVehicle: true,
                TradeInVehicle: new TradeInVehicleResponse(
                    Brand: "Honda",
                    Model: "Civic",
                    Year: 2020,
                    Mileage: 35000,
                    LicensePlate: "ABC-1D23",
                    Color: "Branco",
                    GeneralCondition: "Bom estado, sem avarias",
                    HasDealershipServiceHistory: true
                ),
                PaymentMethod: "Financing",
                ExpectedPurchaseDate: DateTime.Parse("2025-01-30"),
                InterestedInTestDrive: true
            ),
            CreatedAt: DateTime.Parse("2025-01-15T10:30:00Z"),
            UpdatedAt: DateTime.Parse("2025-01-15T14:45:00Z")
        );
    }
}
```

### AsyncAPI Specification

```yaml
# asyncapi.yaml
asyncapi: '2.6.0'
info:
  title: GestAuto Commercial Events API
  version: '1.0.0'
  description: |
    API de eventos assíncronos do Módulo Comercial do GestAuto.
    
    Este documento descreve os eventos publicados e consumidos pelo módulo comercial
    via RabbitMQ para integração com outros módulos do sistema.
  contact:
    name: GestAuto Team
    email: suporte@gestauto.com.br

servers:
  development:
    url: localhost:5672
    protocol: amqp
    description: Servidor de desenvolvimento local
  production:
    url: rabbitmq.gestauto.com.br:5672
    protocol: amqp
    description: Servidor de produção

defaultContentType: application/json

channels:
  commercial.lead.created:
    description: Evento emitido quando um novo lead é cadastrado
    publish:
      operationId: publishLeadCreated
      summary: Publicar evento de lead criado
      message:
        $ref: '#/components/messages/LeadCreatedEvent'

  commercial.lead.scored:
    description: Evento emitido quando um lead é qualificado e recebe uma pontuação
    publish:
      operationId: publishLeadScored
      summary: Publicar evento de lead pontuado
      message:
        $ref: '#/components/messages/LeadScoredEvent'

  commercial.lead.status-changed:
    description: Evento emitido quando o status de um lead é alterado
    publish:
      operationId: publishLeadStatusChanged
      summary: Publicar evento de alteração de status
      message:
        $ref: '#/components/messages/LeadStatusChangedEvent'

  commercial.proposal.created:
    description: Evento emitido quando uma nova proposta é criada
    publish:
      operationId: publishProposalCreated
      summary: Publicar evento de proposta criada
      message:
        $ref: '#/components/messages/ProposalCreatedEvent'

  commercial.sale.closed:
    description: Evento emitido quando uma venda é fechada
    publish:
      operationId: publishSaleClosed
      summary: Publicar evento de venda fechada
      message:
        $ref: '#/components/messages/SaleClosedEvent'

  commercial.used-vehicle.evaluation-requested:
    description: Evento emitido quando uma avaliação de seminovo é solicitada
    publish:
      operationId: publishEvaluationRequested
      summary: Publicar solicitação de avaliação
      message:
        $ref: '#/components/messages/UsedVehicleEvaluationRequestedEvent'

  used-vehicles.evaluation.responded:
    description: Evento consumido quando o módulo de seminovos responde uma avaliação
    subscribe:
      operationId: consumeEvaluationResponded
      summary: Consumir resposta de avaliação
      message:
        $ref: '#/components/messages/EvaluationRespondedEvent'

  finance.order.updated:
    description: Evento consumido quando o módulo financeiro atualiza um pedido
    subscribe:
      operationId: consumeOrderUpdated
      summary: Consumir atualização de pedido
      message:
        $ref: '#/components/messages/OrderUpdatedEvent'

components:
  messages:
    LeadCreatedEvent:
      name: LeadCreatedEvent
      title: Lead Criado
      summary: Evento emitido quando um novo lead é cadastrado
      contentType: application/json
      payload:
        $ref: '#/components/schemas/LeadCreatedPayload'

    LeadScoredEvent:
      name: LeadScoredEvent
      title: Lead Pontuado
      summary: Evento emitido quando um lead é qualificado
      contentType: application/json
      payload:
        $ref: '#/components/schemas/LeadScoredPayload'

    LeadStatusChangedEvent:
      name: LeadStatusChangedEvent
      title: Status do Lead Alterado
      contentType: application/json
      payload:
        $ref: '#/components/schemas/LeadStatusChangedPayload'

    ProposalCreatedEvent:
      name: ProposalCreatedEvent
      title: Proposta Criada
      contentType: application/json
      payload:
        $ref: '#/components/schemas/ProposalCreatedPayload'

    SaleClosedEvent:
      name: SaleClosedEvent
      title: Venda Fechada
      summary: Evento emitido quando uma venda é concluída
      contentType: application/json
      payload:
        $ref: '#/components/schemas/SaleClosedPayload'

    UsedVehicleEvaluationRequestedEvent:
      name: UsedVehicleEvaluationRequestedEvent
      title: Avaliação de Seminovo Solicitada
      contentType: application/json
      payload:
        $ref: '#/components/schemas/EvaluationRequestedPayload'

    EvaluationRespondedEvent:
      name: EvaluationRespondedEvent
      title: Resposta de Avaliação
      summary: Evento emitido pelo módulo de seminovos
      contentType: application/json
      payload:
        $ref: '#/components/schemas/EvaluationRespondedPayload'

    OrderUpdatedEvent:
      name: OrderUpdatedEvent
      title: Pedido Atualizado
      summary: Evento emitido pelo módulo financeiro
      contentType: application/json
      payload:
        $ref: '#/components/schemas/OrderUpdatedPayload'

  schemas:
    LeadCreatedPayload:
      type: object
      required:
        - eventId
        - occurredAt
        - leadId
        - name
        - source
      properties:
        eventId:
          type: string
          format: uuid
          description: ID único do evento
        occurredAt:
          type: string
          format: date-time
          description: Timestamp do evento
        leadId:
          type: string
          format: uuid
          description: ID do lead criado
        name:
          type: string
          description: Nome do cliente
        source:
          type: string
          enum: [Instagram, Referral, Google, Store, Phone, Showroom, ClassifiedsPortal, Other]
          description: Origem do lead

    LeadScoredPayload:
      type: object
      required:
        - eventId
        - occurredAt
        - leadId
        - score
      properties:
        eventId:
          type: string
          format: uuid
        occurredAt:
          type: string
          format: date-time
        leadId:
          type: string
          format: uuid
        score:
          type: string
          enum: [Bronze, Silver, Gold, Diamond]
          description: |
            Classificação do lead:
            - **Diamond**: Financiado + Usado + Compra < 15 dias
            - **Gold**: (À Vista + Usado) OU (Financiado) + Compra < 15 dias
            - **Silver**: À Vista puro
            - **Bronze**: Compra > 30 dias ou sem informações

    SaleClosedPayload:
      type: object
      required:
        - eventId
        - occurredAt
        - proposalId
        - leadId
        - totalValue
      properties:
        eventId:
          type: string
          format: uuid
        occurredAt:
          type: string
          format: date-time
        proposalId:
          type: string
          format: uuid
          description: ID da proposta fechada
        leadId:
          type: string
          format: uuid
          description: ID do lead convertido
        totalValue:
          type: object
          properties:
            amount:
              type: number
              format: decimal
              description: Valor total da venda
            currency:
              type: string
              default: BRL

    EvaluationRequestedPayload:
      type: object
      required:
        - eventId
        - occurredAt
        - evaluationId
        - proposalId
        - brand
        - model
        - year
        - mileage
        - licensePlate
      properties:
        eventId:
          type: string
          format: uuid
        occurredAt:
          type: string
          format: date-time
        evaluationId:
          type: string
          format: uuid
        proposalId:
          type: string
          format: uuid
        brand:
          type: string
          description: Marca do veículo
        model:
          type: string
          description: Modelo do veículo
        year:
          type: integer
          description: Ano do veículo
        mileage:
          type: integer
          description: Quilometragem
        licensePlate:
          type: string
          description: Placa do veículo

    EvaluationRespondedPayload:
      type: object
      required:
        - evaluationId
        - evaluatedValue
        - respondedAt
      properties:
        evaluationId:
          type: string
          format: uuid
        evaluatedValue:
          type: number
          format: decimal
          description: Valor avaliado pelo setor de seminovos
        notes:
          type: string
          description: Observações da avaliação
        respondedAt:
          type: string
          format: date-time

    OrderUpdatedPayload:
      type: object
      required:
        - orderId
        - proposalId
        - newStatus
        - updatedAt
      properties:
        orderId:
          type: string
          format: uuid
        proposalId:
          type: string
          format: uuid
        newStatus:
          type: string
          enum: [AwaitingDocumentation, CreditAnalysis, CreditApproved, CreditRejected, AwaitingVehicle, ReadyForDelivery, Delivered]
        estimatedDeliveryDate:
          type: string
          format: date-time
        updatedAt:
          type: string
          format: date-time
```

### README do Projeto

```markdown
# GestAuto Commercial Module

Módulo Comercial do GestAuto - Sistema de gestão para concessionárias de veículos.

## Tecnologias

- .NET 8
- PostgreSQL 15
- RabbitMQ 3.12
- Entity Framework Core
- FluentValidation
- Serilog

## Requisitos

- Docker e Docker Compose
- .NET 8 SDK
- Visual Studio 2022 ou VS Code

## Setup Rápido

1. Clone o repositório
2. Execute o Docker Compose para subir as dependências:
   ```bash
   docker-compose up -d
   ```
3. Execute as migrations:
   ```bash
   dotnet ef database update --project 4-Infra/GestAuto.Commercial.Infra
   ```
4. Execute a aplicação:
   ```bash
   dotnet run --project 1-Services/GestAuto.Commercial.API
   ```

## Variáveis de Ambiente

| Variável | Descrição | Padrão |
|----------|-----------|--------|
| `ConnectionStrings__Commercial` | Connection string PostgreSQL | `Host=localhost;...` |
| `RabbitMQ__HostName` | Host do RabbitMQ | `localhost` |
| `RabbitMQ__Port` | Porta do RabbitMQ | `5672` |
| `RabbitMQ__UserName` | Usuário RabbitMQ | `gestauto` |
| `RabbitMQ__Password` | Senha RabbitMQ | `gestauto123` |
| `Logto__Authority` | Authority do Logto | - |
| `Logto__Audience` | Audience do Logto | - |

## Documentação

- **Swagger UI**: http://localhost:5000/swagger
- **OpenAPI JSON**: http://localhost:5000/swagger/v1/swagger.json
- **AsyncAPI**: `docs/asyncapi.yaml`

## Testes

```bash
# Unit tests
dotnet test 5-Tests/GestAuto.Commercial.UnitTest

# Integration tests (requer Docker)
dotnet test 5-Tests/GestAuto.Commercial.IntegrationTest
```
```

## Critérios de Sucesso

- [ ] Swagger UI exibe todos os endpoints documentados
- [ ] Todos os endpoints têm exemplos de request e response
- [ ] Todos os schemas de DTOs estão documentados
- [ ] Códigos de status HTTP estão documentados em cada endpoint
- [ ] AsyncAPI documenta todos os eventos publicados e consumidos
- [ ] Schemas de eventos estão completos e validados
- [ ] README contém instruções claras de setup
- [ ] Variáveis de ambiente estão documentadas
- [ ] Documentação pode ser exportada em JSON/YAML
