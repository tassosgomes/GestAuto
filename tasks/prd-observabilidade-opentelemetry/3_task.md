---
status: pending
parallelizable: false
blocked_by: ["2.0"]
---

<task_context>
<domain>backend/stock</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>low</complexity>
<dependencies>otel-collector, entity-framework-core, aspnetcore</dependencies>
<unblocks>5.0</unblocks>
</task_context>

# Tarefa 3.0: Instrumentação OpenTelemetry - stock-api (.NET)

## Visão Geral

Replicar a instrumentação OpenTelemetry implementada na commercial-api para a stock-api. Como ambas são aplicações .NET 8 com Entity Framework Core, a configuração será praticamente idêntica, apenas alterando o `service_name` para "stock".

**Nota:** Esta tarefa é uma cópia direta do padrão estabelecido em 2.0, portanto tem complexidade baixa.

<requirements>
- Copiar configuração de OpenTelemetry da commercial-api
- Ajustar service_name para "stock"
- Adicionar pacotes NuGet
- Configurar logs JSON estruturados
- Adicionar variáveis de ambiente no docker-compose
</requirements>

## Subtarefas

- [x] 3.1 Adicionar pacotes NuGet de OpenTelemetry ao projeto stock-api
- [x] 3.2 Copiar `OpenTelemetryExtensions.cs` da commercial-api
- [x] 3.3 Copiar `SpanEnricher.cs` da commercial-api
- [x] 3.4 Configurar TracerProvider no `Program.cs`
- [x] 3.5 Atualizar configuração Serilog no appsettings.json
- [x] 3.6 Adicionar variáveis de ambiente OTel no docker-compose
- [ ] 3.7 Validar traces no Grafana/Tempo após deploy

## Detalhes de Implementação

### 3.1 Pacotes NuGet

Adicionar ao arquivo `services/stock/*.csproj`:

```xml
<PackageReference Include="OpenTelemetry" Version="1.7.0" />
<PackageReference Include="OpenTelemetry.Exporter.OpenTelemetryProtocol" Version="1.7.0" />
<PackageReference Include="OpenTelemetry.Extensions.Hosting" Version="1.7.0" />
<PackageReference Include="OpenTelemetry.Instrumentation.AspNetCore" Version="1.7.0" />
<PackageReference Include="OpenTelemetry.Instrumentation.Http" Version="1.7.0" />
<PackageReference Include="OpenTelemetry.Instrumentation.EntityFrameworkCore" Version="1.0.0-beta.11" />
```

### 3.2-3.3 Copiar Arquivos

Copiar os seguintes arquivos da commercial-api:

```bash
# De services/commercial/ para services/stock/
cp Extensions/OpenTelemetryExtensions.cs ../stock/Extensions/
cp Logging/SpanEnricher.cs ../stock/Logging/
```

**Nota:** Os arquivos podem ser copiados sem modificações, pois o `service_name` é configurado via variável de ambiente.

### 3.4 Configuração no Program.cs

```csharp
// Program.cs
using Stock.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Adicionar observabilidade
builder.Services.AddObservability(builder.Configuration);

// ... resto da configuração
```

### 3.5 Configuração Serilog

Adicionar ao `appsettings.json`:

```json
{
  "Serilog": {
    "Using": ["Serilog.Sinks.Console"],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "formatter": "Serilog.Formatting.Json.JsonFormatter, Serilog"
        }
      }
    ],
    "Enrich": ["FromLogContext", "WithSpan"]
  }
}
```

### 3.6 Variáveis de Ambiente Docker

Adicionar ao `docker-compose.yml` (seção do stock):

```yaml
stock:
  environment:
    - OTEL_SERVICE_NAME=stock
    - OTEL_SERVICE_VERSION=1.0.0
    - OTEL_EXPORTER_OTLP_ENDPOINT=http://otel-collector:4317
    - OTEL_EXPORTER_OTLP_PROTOCOL=grpc
    - OTEL_TRACES_EXPORTER=otlp
    - OTEL_LOGS_EXPORTER=otlp
    - OTEL_METRICS_EXPORTER=none
```

## Critérios de Sucesso

- [ ] Aplicação inicia sem erros com instrumentação OTel
- [ ] Traces aparecem no Grafana/Tempo com service_name="stock"
- [ ] Queries EF Core aparecem como spans filhos
- [ ] Health check e swagger NÃO geram traces
- [ ] Logs incluem `trace_id` e `span_id`
- [ ] Traces de stock-api visíveis junto com commercial-api no mesmo dashboard

## Sequenciamento

- **Bloqueado por:** 2.0 (commercial-api - estabelece o padrão)
- **Desbloqueia:** 5.0 (frontend - mais um backend disponível)
- **Paralelizável:** Não (depende do padrão de 2.0)

## Arquivos Afetados

| Arquivo | Ação | Descrição |
|---------|------|-----------|
| `services/stock/*.csproj` | Modificar | Adicionar pacotes NuGet |
| `services/stock/Extensions/OpenTelemetryExtensions.cs` | Criar | Copiar de commercial |
| `services/stock/Logging/SpanEnricher.cs` | Criar | Copiar de commercial |
| `services/stock/Program.cs` | Modificar | Adicionar `AddObservability()` |
| `services/stock/appsettings.json` | Modificar | Configuração Serilog |
| `docker-compose.yml` | Modificar | Variáveis de ambiente OTel |

## Estimativa de Tempo

**0.5 dia** - Como é uma cópia do padrão já estabelecido, a implementação é rápida.

## Entregáveis

1. Código de instrumentação no stock-api
2. Atualização do docker-compose
3. Screenshot de trace no Grafana comprovando service_name="stock"
