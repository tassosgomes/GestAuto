# GestAuto Commercial Module

Sistema de Gestão Comercial para Concessionárias de Veículos - Módulo de Leads, Propostas, Test-Drives e Avaliações.

## 📋 Visão Geral

O módulo comercial do GestAuto é responsável por gerenciar todo o fluxo de vendas de uma concessionária, incluindo:

- **Gestão de Leads**: Cadastro, qualificação e acompanhamento de potenciais clientes
- **Propostas Comerciais**: Criação, negociação e fechamento de propostas
- **Test-Drives**: Agendamento e controle de test-drives
- **Avaliações de Seminovos**: Solicitação e processamento de avaliações de veículos usados

## 🛠 Tecnologias

- **.NET 8**: Framework para aplicação
- **PostgreSQL 15**: Banco de dados relacional
- **RabbitMQ 3.12**: Message broker para eventos assíncronos
- **Entity Framework Core 8**: ORM para acesso a dados
- **Logto**: Autenticação e autorização via JWT
- **FluentValidation**: Validação de dados
- **Serilog**: Logging estruturado
- **Swagger/OpenAPI**: Documentação de API
- **AsyncAPI 2.6**: Documentação de eventos

## 📐 Arquitetura

O módulo segue a arquitetura limpa com as seguintes camadas:

```
1-Services/
├── GestAuto.Commercial.API/          # Camada de apresentação (Controllers)
│   ├── Controllers/                   # Endpoints REST
│   ├── Middleware/                    # Middleware (autenticação, exceções)
│   ├── Program.cs                     # Configuração da aplicação
│   └── appsettings.json              # Configurações

2-Application/
├── GestAuto.Commercial.Application/   # Camada de aplicação
│   ├── Commands/                      # Operações de escrita (CQRS)
│   ├── Queries/                       # Operações de leitura (CQRS)
│   ├── Handlers/                      # Implementação dos handlers
│   ├── DTOs/                          # Data Transfer Objects
│   ├── Interfaces/                    # Interfaces de contrato
│   └── Validators/                    # Validações FluentValidation

3-Domain/
├── GestAuto.Commercial.Domain/        # Camada de domínio
│   ├── Entities/                      # Entidades do domínio
│   ├── ValueObjects/                  # Value Objects
│   ├── Enums/                         # Enumerações
│   ├── Events/                        # Domain Events
│   ├── Exceptions/                    # Exceções de negócio
│   └── Interfaces/                    # Interfaces de repositório

4-Infra/
├── GestAuto.Commercial.Infra/         # Camada de infraestrutura
│   ├── Entities/                      # Entidades para mapeamento EF
│   ├── EntityConfigurations/          # Configuração dos mapeamentos
│   ├── Migrations/                    # Migrações do banco de dados
│   ├── Messaging/                     # Integração com RabbitMQ
│   ├── Repositories/                  # Implementação dos repositórios
│   ├── CommercialDbContext.cs         # DbContext do EF Core
│   └── Consumers/                     # Consumidores de eventos

5-Tests/
├── GestAuto.Commercial.UnitTest/      # Testes unitários
├── GestAuto.Commercial.IntegrationTest/ # Testes de integração
└── GestAuto.Commercial.End2EndTest/   # Testes E2E
```

## 🚀 Guia de Início Rápido

### Pré-requisitos

- [Docker](https://www.docker.com/) e [Docker Compose](https://docs.docker.com/compose/)
- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) ou [VS Code](https://code.visualstudio.com/)

### Setup Local

1. **Clone o repositório**
   ```bash
   git clone https://github.com/tassosgomes/GestAuto.git
   cd GestAuto
   ```

2. **Inicie as dependências com Docker Compose**
   ```bash
   docker-compose up -d
   ```
   Isso iniciará:
   - PostgreSQL 15
   - RabbitMQ 3.12
   - Adminer (cliente web para PostgreSQL)

3. **Execute as migrações do banco de dados**
   ```bash
   cd services/commercial
   dotnet ef database update --project 4-Infra/GestAuto.Commercial.Infra
   ```

4. **Execute a aplicação**
   ```bash
   dotnet run --project 1-Services/GestAuto.Commercial.API
   ```

   A API estará disponível em: `http://localhost:5092` (Development)

5. **Acesse a documentação**
   - **Swagger UI (REST)**: http://localhost:5092/swagger
   - **AsyncAPI UI (Eventos)**: http://localhost:5092/asyncapi
   - **OpenAPI JSON**: http://localhost:5092/swagger/v1/swagger.json
   - **AsyncAPI YAML**: http://localhost:5092/asyncapi.yaml

## 🔐 Autenticação e Autorização

### Logto JWT

A API utiliza autenticação JWT através do Logto. Para fazer requisições autenticadas:

1. Obtenha um token JWT do seu provedor Logto
2. Inclua o token no header de todas as requisições:
   ```
   Authorization: Bearer <seu_token_jwt>
   ```

### Roles e Permissões

| Role | Descrição | Permissões |
|------|-----------|-----------|
| `sales_person` | Vendedor | Acesso aos próprios leads, propostas e test-drives |
| `manager` | Gerente | Acesso a todos os registros + aprovação de descontos |

### Políticas de Autorização

```csharp
// Requer role sales_person ou manager
[Authorize(Policy = "SalesPerson")]

// Requer role manager
[Authorize(Policy = "Manager")]
```

## 🌍 Endpoints da API

### Leads

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| `POST` | `/api/v1/leads` | Criar novo lead |
| `GET` | `/api/v1/leads` | Listar leads com paginação |
| `GET` | `/api/v1/leads/{id}` | Obter detalhes do lead |
| `PUT` | `/api/v1/leads/{id}` | Atualizar lead |
| `POST` | `/api/v1/leads/{id}/qualify` | Qualificar lead |
| `POST` | `/api/v1/leads/{id}/status` | Alterar status do lead |
| `POST` | `/api/v1/leads/{id}/interactions` | Registrar interação |
| `GET` | `/api/v1/leads/{id}/interactions` | Listar interações |

### Propostas

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| `POST` | `/api/v1/proposals` | Criar proposta |
| `GET` | `/api/v1/proposals` | Listar propostas |
| `GET` | `/api/v1/proposals/{id}` | Obter proposta |
| `PUT` | `/api/v1/proposals/{id}` | Atualizar proposta |
| `POST` | `/api/v1/proposals/{id}/items` | Adicionar item |
| `POST` | `/api/v1/proposals/{id}/discount` | Aplicar desconto |
| `POST` | `/api/v1/proposals/{id}/discount/approve` | Aprovar desconto |
| `POST` | `/api/v1/proposals/{id}/close` | Fechar proposta |

### Test-Drives

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| `POST` | `/api/v1/test-drives` | Agendar test-drive |
| `GET` | `/api/v1/test-drives` | Listar test-drives |
| `GET` | `/api/v1/test-drives/{id}` | Obter test-drive |
| `POST` | `/api/v1/test-drives/{id}/complete` | Completar test-drive |
| `POST` | `/api/v1/test-drives/{id}/cancel` | Cancelar test-drive |

### Avaliações

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| `POST` | `/api/v1/used-vehicle-evaluations` | Solicitar avaliação |
| `GET` | `/api/v1/used-vehicle-evaluations` | Listar avaliações |
| `GET` | `/api/v1/used-vehicle-evaluations/{id}` | Obter avaliação |
| `POST` | `/api/v1/used-vehicle-evaluations/{id}/customer-response` | Resposta do cliente |

## 📊 Eventos Assíncronos

O módulo publica e consome eventos via RabbitMQ para integração com outros módulos.

### Eventos Publicados

- `commercial.lead.created`: Novo lead cadastrado
- `commercial.lead.qualified`: Lead qualificado
- `commercial.lead.status-changed`: Status do lead alterado
- `commercial.proposal.created`: Proposta criada
- `commercial.proposal.discount-applied`: Desconto aplicado
- `commercial.proposal.closed`: Proposta fechada (venda concluída)
- `commercial.testdrive.scheduled`: Test-drive agendado
- `commercial.testdrive.completed`: Test-drive concluído
- `commercial.used-vehicle.evaluation-requested`: Avaliação de seminovo solicitada

### Eventos Consumidos

- `used-vehicles.evaluation.responded`: Resposta de avaliação (do módulo de seminovos)
- `finance.order.updated`: Atualização de pedido (do módulo financeiro)

Consulte `docs/asyncapi.yaml` para detalhes completos sobre os schemas de eventos.

## ⚙️ Variáveis de Ambiente

### Banco de Dados

| Variável | Descrição | Valor Padrão |
|----------|-----------|--------------|
| `ConnectionStrings__CommercialDatabase` | Connection string PostgreSQL | `Host=localhost;Port=5432;Database=gestauto_commercial;...` |

### RabbitMQ

| Variável | Descrição | Valor Padrão |
|----------|-----------|--------------|
| `RabbitMQ__HostName` | Host do RabbitMQ | `localhost` |
| `RabbitMQ__Port` | Porta do RabbitMQ | `5672` |
| `RabbitMQ__UserName` | Usuário RabbitMQ | `gestauto` |
| `RabbitMQ__Password` | Senha RabbitMQ | `gestauto123` |
| `RabbitMQ__VirtualHost` | Virtual host | `/` |

### Autenticação (Logto)

| Variável | Descrição | Valor Padrão |
|----------|-----------|--------------|
| `Logto__Authority` | Authority URL do Logto | - |
| `Logto__Audience` | Audience/Resource Identifier | - |

### Logging

| Variável | Descrição | Valor Padrão |
|----------|-----------|--------------|
| `Serilog__MinimumLevel__Default` | Nível mínimo de log | `Information` |
| `Serilog__MinimumLevel__Override__Microsoft` | Nível para logs do .NET | `Warning` |

## 🧪 Testes

### Executar Todos os Testes

```bash
# Testes unitários
dotnet test 5-Tests/GestAuto.Commercial.UnitTest/GestAuto.Commercial.UnitTest.csproj

# Testes de integração (requer Docker)
dotnet test 5-Tests/GestAuto.Commercial.IntegrationTest/GestAuto.Commercial.IntegrationTest.csproj

# Testes E2E
dotnet test 5-Tests/GestAuto.Commercial.End2EndTest/GestAuto.Commercial.End2EndTest.csproj
```

### Executar Testes Específicos

```bash
# Por padrão de nome
dotnet test --filter "ClassName=LeadServiceTests"

# Por trait
dotnet test --filter "Category=Integration"
```

## 📚 Documentação

### Swagger/OpenAPI

A documentação interativa dos endpoints REST está disponível em:
- **Swagger UI**: http://localhost:5092/swagger
- **OpenAPI JSON**: http://localhost:5092/swagger/v1/swagger.json

Você pode:
- Explorar todos os endpoints REST
- Visualizar modelos de requisição/resposta
- Testar endpoints diretamente no navegador
- Gerar SDKs clientes

### AsyncAPI (Eventos Assíncronos)

A documentação de eventos do módulo está disponível em:

**Interface Web Interativa:**
- **AsyncAPI Viewer**: http://localhost:5092/asyncapi (redireciona para Redocly)
  - Explore canais, operações, schemas e exemplos de eventos
  - Interface visual e interativa

**Especificação Raw:**
- **AsyncAPI YAML**: http://localhost:5092/asyncapi.yaml
  - Retorna a especificação completa em YAML
  - Integre com ferramentas de API Gateway ou validadores

**Alternativas para Visualizar:**
- [AsyncAPI Studio Online](https://studio.asyncapi.com/) - Copie o conteúdo de `/asyncapi.yaml`
- [ReDoc AsyncAPI](https://redocly.com/docs/api-reference/) - Cole a URL do servidor
- [CLI Validator](https://www.asyncapi.com/tools/cli) - Valide localmente

**Eventos Documentados:**
- 9 eventos publicados (leads, propostas, test-drives, avaliações)
- 2 eventos consumidos (avaliação respondida, pedido atualizado)
- Protocolo: RabbitMQ (AMQP 0.9.1)
- Schemas completos com exemplos

> Para mais detalhes sobre a implementação da AsyncAPI UI, veja [ASYNCAPI_UI.md](./ASYNCAPI_UI.md)

## 🐛 Desenvolvimento

### Estrutura de Pasta

Ao criar novos recursos, mantenha a seguinte estrutura:

```
LeadFeature/
├── 1-Services/
│   └── Controllers/LeadController.cs
├── 2-Application/
│   ├── Commands/CreateLeadCommand.cs
│   ├── Handlers/CreateLeadCommandHandler.cs
│   ├── Queries/GetLeadQuery.cs
│   ├── DTOs/LeadDTOs.cs
│   └── Validators/CreateLeadValidator.cs
├── 3-Domain/
│   ├── Entities/Lead.cs
│   ├── Events/LeadCreatedEvent.cs
│   └── Interfaces/ILeadRepository.cs
└── 4-Infra/
    ├── Entities/LeadEntity.cs
    ├── EntityConfigurations/LeadConfiguration.cs
    └── Repositories/LeadRepository.cs
```

### Padrões

#### CQRS

- **Commands**: Operações que modificam estado (criar, atualizar, deletar)
- **Queries**: Operações que consultam dados (listar, obter)
- Cada um tem seu próprio handler

#### Domain Events

Emita eventos de domínio para ações importantes:

```csharp
public class LeadCreatedEvent : IDomainEvent
{
    public Guid LeadId { get; init; }
    public string Name { get; init; }
    public DateTime OccurredAt { get; init; }
}

// No aggregado
lead.AddDomainEvent(new LeadCreatedEvent { ... });
```

#### Validações

Use FluentValidation para todas as validações:

```csharp
public class CreateLeadValidator : AbstractValidator<CreateLeadCommand>
{
    public CreateLeadValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Email).EmailAddress();
    }
}
```

## 🔄 Pipeline de CI/CD

O projeto utiliza GitHub Actions para:
- Executar testes automaticamente em cada PR
- Validar código com SonarQube
- Build e deploy em desenvolvimento
- Build e push de imagens Docker

## 📖 Referências

- [Documentação .NET 8](https://learn.microsoft.com/en-us/dotnet/)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [AsyncAPI Specification](https://www.asyncapi.com/)
- [RabbitMQ](https://www.rabbitmq.com/)
- [Logto](https://logto.io/)

## 👥 Suporte

Para dúvidas ou sugestões sobre o módulo:

- 📧 Email: suporte@gestauto.com.br
- 📌 Issues: [GitHub Issues](https://github.com/tassosgomes/GestAuto/issues)
- 💬 Discussions: [GitHub Discussions](https://github.com/tassosgomes/GestAuto/discussions)

## 📝 Licença

Proprietary - Todos os direitos reservados
