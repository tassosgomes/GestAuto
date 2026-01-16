---
status: completed
parallelizable: false
blocked_by: []
---

<task_context>
<domain>infra/setup</domain>
<type>configuration</type>
<scope>infrastructure</scope>
<complexity>medium</complexity>
<dependencies>docker|postgresql|rabbitmq</dependencies>
<unblocks>2.0</unblocks>
</task_context>

# Tarefa 1.0: Configurar Infraestrutura Base do Projeto

## Visão Geral

Criar a estrutura base do projeto .NET 8 seguindo Clean Architecture, incluindo a configuração de Docker Compose para PostgreSQL e RabbitMQ, e a estrutura de pastas conforme definido na especificação técnica.

<requirements>
- Criar Solution .NET 8 com estrutura de pastas Clean Architecture
- Criar/Atualizar Docker Compose na raiz do repositório com PostgreSQL 18 e RabbitMQ 4.1 (compartilhado entre microserviços)
- Criar projetos para cada camada (API, Application, Domain, Infra, Tests) dentro de `services/commercial/`
- Configurar referências entre projetos
- Criar migrations iniciais do banco de dados (schema `commercial`)
</requirements>

## Subtarefas

- [x] 1.1 Criar Solution `GestAuto.Commercial.sln` ✅
- [x] 1.2 Criar projeto `GestAuto.Commercial.API` (1-Services) ✅
- [x] 1.3 Criar projeto `GestAuto.Commercial.Application` (2-Application) ✅
- [x] 1.4 Criar projeto `GestAuto.Commercial.Domain` (3-Domain) ✅
- [x] 1.5 Criar projeto `GestAuto.Commercial.Infra` (4-Infra) ✅
- [x] 1.6 Criar projetos de teste (UnitTest, IntegrationTest, End2EndTest) ✅
- [x] 1.7 Configurar referências entre projetos conforme Clean Architecture ✅
- [x] 1.8 Criar/Atualizar `docker-compose.yml` na raiz do repositório com PostgreSQL 18 e RabbitMQ 4.1 ✅
- [x] 1.9 Configurar `appsettings.json` com connection strings ✅
- [x] 1.10 Instalar pacotes NuGet essenciais (EF Core, FluentValidation, Serilog) ✅
- [x] 1.11 Criar `CommercialDbContext.cs` básico ✅
- [x] 1.12 Criar migration inicial (estrutura base) ✅

## Sequenciamento

- **Bloqueado por:** Nenhuma tarefa
- **Desbloqueia:** 2.0 (Entidades do Domain)
- **Paralelizável:** Não (é a tarefa inicial)

## Detalhes de Implementação

### Estrutura de Pastas

```
GestAuto/                              # Raiz do repositório
├── docker-compose.yml                 # Compartilhado entre todos os microserviços
├── services/
│   └── commercial/                    # Módulo Comercial
│       ├── GestAuto.Commercial.sln
│       ├── 1-Services/
│       │   └── GestAuto.Commercial.API/
│       ├── 2-Application/
│       │   └── GestAuto.Commercial.Application/
│       ├── 3-Domain/
│       │   └── GestAuto.Commercial.Domain/
│       ├── 4-Infra/
│       │   └── GestAuto.Commercial.Infra/
│       └── 5-Tests/
│           ├── GestAuto.Commercial.UnitTest/
│           ├── GestAuto.Commercial.IntegrationTest/
│           └── GestAuto.Commercial.End2EndTest/
│   └── [outros microserviços futuros]
└── frontend/                          # Frontend (futuro)
```

> **Nota:** O `docker-compose.yml` fica na raiz do repositório para ser compartilhado entre todos os microserviços e o frontend.

### Docker Compose (na raiz do repositório)

```yaml
# docker-compose.yml - Localizado em /GestAuto/docker-compose.yml
version: '3.8'

services:
  postgres:
    image: postgres:18
    container_name: gestauto-postgres
    environment:
      POSTGRES_USER: gestauto
      POSTGRES_PASSWORD: gestauto123
      POSTGRES_DB: gestauto
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U gestauto"]
      interval: 10s
      timeout: 5s
      retries: 5

  rabbitmq:
    image: rabbitmq:4.1-management
    container_name: gestauto-rabbitmq
    environment:
      RABBITMQ_DEFAULT_USER: gestauto
      RABBITMQ_DEFAULT_PASS: gestauto123
    ports:
      - "5672:5672"
      - "15672:15672"
    volumes:
      - rabbitmq_data:/var/lib/rabbitmq
    healthcheck:
      test: ["CMD", "rabbitmq-diagnostics", "check_running"]
      interval: 10s
      timeout: 5s
      retries: 5

volumes:
  postgres_data:
  rabbitmq_data:
```

> **Importante:** O banco `gestauto` é único e cada microserviço terá seu próprio schema (ex: `commercial`, `used_vehicles`, `finance`).

### Pacotes NuGet Essenciais

**GestAuto.Commercial.API:**
- Microsoft.AspNetCore.OpenApi
- Swashbuckle.AspNetCore
- Serilog.AspNetCore
- Serilog.Sinks.Console
- Serilog.Formatting.Elasticsearch

**GestAuto.Commercial.Application:**
- FluentValidation
- FluentValidation.DependencyInjectionExtensions

**GestAuto.Commercial.Infra:**
- Npgsql.EntityFrameworkCore.PostgreSQL
- Microsoft.EntityFrameworkCore.Design
- RabbitMQ.Client

**GestAuto.Commercial.UnitTest:**
- xunit
- xunit.runner.visualstudio
- FluentAssertions
- NSubstitute

**GestAuto.Commercial.IntegrationTest:**
- Testcontainers.PostgreSql
- Testcontainers.RabbitMq

## Critérios de Sucesso

- [x] Solution compila sem erros ✅
- [x] `docker-compose up` inicia PostgreSQL e RabbitMQ ✅
- [x] Conexão com banco de dados funciona via EF Core ✅
- [x] Estrutura de pastas segue padrão Clean Architecture ✅
- [x] Todos os projetos de teste executam (mesmo sem testes ainda) ✅
- [x] Health check básico respondendo em `/health` ✅
