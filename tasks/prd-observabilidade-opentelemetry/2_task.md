---
status: pending
parallelizable: true
blocked_by: ["1.0"]
---

<task_context>
<domain>backend/commercial</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>high</complexity>
<dependencies>otel-collector, entity-framework-core, aspnetcore</dependencies>
<unblocks>3.0, 5.0</unblocks>
</task_context>

# Tarefa 2.0: Instrumentação OpenTelemetry - commercial-api (.NET)

## Visão Geral

Implementar instrumentação completa de OpenTelemetry na API commercial (.NET 8), incluindo traces para requisições HTTP, queries de banco de dados (Entity Framework Core), chamadas HTTP externas e logs estruturados em JSON com correlação de trace. Esta implementação servirá como **template para a stock-api**.

<requirements>
- Adicionar pacotes NuGet do OpenTelemetry
- Configurar TracerProvider com exportação OTLP/gRPC
- Instrumentar AspNetCore (requisições HTTP de entrada)
- Instrumentar HttpClient (chamadas HTTP de saída)
- Instrumentar Entity Framework Core (queries de banco)
- Configurar logs estruturados JSON com Serilog
- Filtrar endpoints de health check e swagger
- Adicionar variáveis de ambiente no docker-compose
</requirements>

## Subtarefas

- [ ] 2.1 Adicionar pacotes NuGet de OpenTelemetry ao projeto
- [ ] 2.2 Criar extensão `OpenTelemetryExtensions.cs` para configuração centralizada
- [ ] 2.3 Configurar TracerProvider no `Program.cs`
- [ ] 2.4 Configurar instrumentação AspNetCore com filtros de endpoints
- [ ] 2.5 Configurar instrumentação HttpClient para propagação de contexto
- [ ] 2.6 Configurar instrumentação Entity Framework Core
- [ ] 2.7 Configurar Serilog para logs JSON estruturados com trace context
- [ ] 2.8 Adicionar variáveis de ambiente OTel no docker-compose
- [ ] 2.9 Criar testes unitários para filtros de instrumentação
- [ ] 2.10 Validar traces no Grafana/Tempo após deploy

## Detalhes de Implementação

### 2.1 Pacotes NuGet

Adicionar ao arquivo `.csproj`:

```xml
<PackageReference Include="OpenTelemetry" Version="1.7.0" />
<PackageReference Include="OpenTelemetry.Exporter.OpenTelemetryProtocol" Version="1.7.0" />
<PackageReference Include="OpenTelemetry.Extensions.Hosting" Version="1.7.0" />
<PackageReference Include="OpenTelemetry.Instrumentation.AspNetCore" Version="1.7.0" />
<PackageReference Include="OpenTelemetry.Instrumentation.Http" Version="1.7.0" />
<PackageReference Include="OpenTelemetry.Instrumentation.EntityFrameworkCore" Version="1.0.0-beta.11" />
```

### 2.2 Extensão de Configuração

Criar `Extensions/OpenTelemetryExtensions.cs`:

```csharp
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Commercial.API.Extensions;

public static class OpenTelemetryExtensions
{
    private static readonly string[] IgnoredPaths = ["/health", "/ready", "/swagger"];

    public static IServiceCollection AddObservability(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var serviceName = configuration["OTEL_SERVICE_NAME"] ?? "commercial";
        var serviceVersion = configuration["OTEL_SERVICE_VERSION"] ?? "1.0.0";
        var otlpEndpoint = configuration["OTEL_EXPORTER_OTLP_ENDPOINT"] ?? "http://otel-collector:4317";

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(
                    serviceName: serviceName,
                    serviceVersion: serviceVersion))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation(options =>
                {
                    options.Filter = ctx => !ShouldIgnorePath(ctx.Request.Path);
                    options.RecordException = true;
                    options.EnrichWithHttpRequest = (activity, request) =>
                    {
                        activity.SetTag("http.request.path", request.Path);
                    };
                    options.EnrichWithHttpResponse = (activity, response) =>
                    {
                        activity.SetTag("http.response.status_code", response.StatusCode);
                    };
                })
                .AddHttpClientInstrumentation(options =>
                {
                    options.RecordException = true;
                    options.EnrichWithHttpRequestMessage = (activity, request) =>
                    {
                        activity.SetTag("http.request.uri", SanitizeUrl(request.RequestUri?.ToString()));
                    };
                })
                .AddEntityFrameworkCoreInstrumentation(options =>
                {
                    options.SetDbStatementForText = true;
                    options.SetDbStatementForStoredProcedure = true;
                })
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(otlpEndpoint);
                    options.Protocol = OtlpExportProtocol.Grpc;
                    options.ExportProcessorType = ExportProcessorType.Batch;
                    options.BatchExportProcessorOptions = new BatchExportProcessorOptions<System.Diagnostics.Activity>
                    {
                        MaxQueueSize = 2048,
                        ScheduledDelayMilliseconds = 5000,
                        ExporterTimeoutMilliseconds = 5000,
                        MaxExportBatchSize = 512
                    };
                }));

        return services;
    }

    private static bool ShouldIgnorePath(PathString path)
    {
        var pathValue = path.Value ?? string.Empty;
        return IgnoredPaths.Any(ignored => pathValue.StartsWith(ignored, StringComparison.OrdinalIgnoreCase));
    }

    private static string? SanitizeUrl(string? url)
    {
        if (string.IsNullOrEmpty(url)) return url;
        // Remove tokens e dados sensíveis de query strings
        var uri = new Uri(url);
        return $"{uri.Scheme}://{uri.Host}{uri.AbsolutePath}";
    }
}
```

### 2.3 Configuração no Program.cs

```csharp
// Program.cs
using Commercial.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Adicionar observabilidade
builder.Services.AddObservability(builder.Configuration);

// ... resto da configuração
```

### 2.7 Configuração Serilog para Logs JSON

Atualizar `appsettings.json`:

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

Criar enricher para trace context (`Logging/SpanEnricher.cs`):

```csharp
using System.Diagnostics;
using Serilog.Core;
using Serilog.Events;

namespace Commercial.API.Logging;

public class SpanEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var activity = Activity.Current;
        if (activity == null) return;

        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("trace_id", activity.TraceId.ToString()));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("span_id", activity.SpanId.ToString()));
    }
}
```

### 2.8 Variáveis de Ambiente Docker

Adicionar ao `docker-compose.yml` (seção do commercial):

```yaml
commercial:
  environment:
    - OTEL_SERVICE_NAME=commercial
    - OTEL_SERVICE_VERSION=1.0.0
    - OTEL_EXPORTER_OTLP_ENDPOINT=http://otel-collector:4317
    - OTEL_EXPORTER_OTLP_PROTOCOL=grpc
    - OTEL_TRACES_EXPORTER=otlp
    - OTEL_LOGS_EXPORTER=otlp
    - OTEL_METRICS_EXPORTER=none
```

## Critérios de Sucesso

- [ ] Aplicação inicia sem erros com instrumentação OTel
- [ ] Requisições HTTP geram traces visíveis no Grafana/Tempo
- [ ] Queries EF Core aparecem como spans filhos
- [ ] Health check e swagger NÃO geram traces
- [ ] Logs incluem `trace_id` e `span_id` quando em contexto de requisição
- [ ] Exceções são registradas no span com stack trace
- [ ] Overhead de latência < 5% (medir antes/depois)
- [ ] Testes unitários passando

## Sequenciamento

- **Bloqueado por:** 1.0 (Validação de Infraestrutura)
- **Desbloqueia:** 3.0 (stock-api), 5.0 (frontend)
- **Paralelizável:** Sim (pode executar em paralelo com 4.0)

## Arquivos Afetados

| Arquivo | Ação | Descrição |
|---------|------|-----------|
| `services/commercial/*.csproj` | Modificar | Adicionar pacotes NuGet |
| `services/commercial/Extensions/OpenTelemetryExtensions.cs` | Criar | Configuração centralizada |
| `services/commercial/Logging/SpanEnricher.cs` | Criar | Enricher de trace context |
| `services/commercial/Program.cs` | Modificar | Adicionar `AddObservability()` |
| `services/commercial/appsettings.json` | Modificar | Configuração Serilog |
| `docker-compose.yml` | Modificar | Variáveis de ambiente OTel |

## Riscos e Mitigações

| Risco | Probabilidade | Mitigação |
|-------|---------------|-----------|
| Conflito de versão de pacotes | Baixa | Usar versões compatíveis documentadas |
| Overhead de performance | Baixa | Medir latência antes/depois; ajustar batch size |
| EF Core instrumentation beta | Média | Testar exaustivamente; ter fallback sem EF instrumentation |

## Entregáveis

1. Código de instrumentação no commercial-api
2. Configuração de logs JSON estruturados
3. Atualização do docker-compose
4. Testes unitários para filtros
5. Screenshot de trace no Grafana comprovando funcionamento
