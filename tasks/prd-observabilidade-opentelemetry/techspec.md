# Tech Spec: Implementação de Observabilidade com OpenTelemetry

## Resumo Executivo

Esta Tech Spec define a estratégia de implementação de observabilidade distribuída para o sistema GestAuto utilizando OpenTelemetry. A solução instrumentará 4 aplicações (`commercial`, `stock`, `vehicle-evaluation`, `frontend`) para gerar traces e logs correlacionados, enviados ao OpenTelemetry Collector existente e visualizados no Grafana (Tempo) e OpenSearch Dashboards.

A arquitetura adota uma abordagem de instrumentação não-intrusiva usando bibliotecas oficiais do OpenTelemetry para cada stack (.NET, Java, JavaScript), com formato de log JSON padronizado e propagação de contexto W3C Trace Context entre serviços. O overhead esperado é inferior a 5% no tempo de resposta, com exportação em batch para minimizar impacto em I/O.

---

## Arquitetura do Sistema

### Visão Geral dos Componentes

```
┌─────────────────────────────────────────────────────────────────────────┐
│                              FRONTEND                                    │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │  React App (frontend)                                            │   │
│  │  - WebTracerProvider + BatchSpanProcessor                        │   │
│  │  - Axios Interceptors (propagação W3C headers)                   │   │
│  │  - Auto-instrumentação: fetch, document-load, user-interaction   │   │
│  └───────────────────────────┬─────────────────────────────────────┘   │
└──────────────────────────────│──────────────────────────────────────────┘
                               │ OTLP/HTTP (https://otel.tasso.dev.br)
                               ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                         BACKEND SERVICES                                 │
│                                                                          │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────────────┐  │
│  │ commercial-api  │  │   stock-api     │  │   vehicle-evaluation    │  │
│  │    (.NET 8)     │  │    (.NET 8)     │  │   (Java 21/Spring)      │  │
│  │                 │  │                 │  │                         │  │
│  │ - OTel .NET     │  │ - OTel .NET     │  │ - OTel Java Agent       │  │
│  │ - AspNetCore    │  │ - AspNetCore    │  │ - Spring Boot Starter   │  │
│  │ - EF Core       │  │ - EF Core       │  │ - JDBC Instrumentation  │  │
│  │ - HttpClient    │  │ - HttpClient    │  │ - Logback Appender      │  │
│  └────────┬────────┘  └────────┬────────┘  └───────────┬─────────────┘  │
│           │                    │                       │                 │
│           └────────────────────┼───────────────────────┘                 │
│                                │ OTLP/gRPC (otel-collector:4317)         │
└────────────────────────────────│─────────────────────────────────────────┘
                                 ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                      OBSERVABILITY STACK                                 │
│  ┌─────────────────────────────────────────────────────────────────┐   │
│  │              OpenTelemetry Collector                             │   │
│  │  - Receivers: OTLP (gRPC:4317, HTTP:4318)                       │   │
│  │  - Processors: batch, memory_limiter                            │   │
│  │  - Exporters: tempo, opensearch                                 │   │
│  └─────────────────────────┬───────────────────────────────────────┘   │
│                            │                                            │
│            ┌───────────────┴───────────────┐                           │
│            ▼                               ▼                            │
│  ┌─────────────────┐            ┌─────────────────────┐                │
│  │     Tempo       │            │    OpenSearch       │                │
│  │   (Traces)      │            │     (Logs)          │                │
│  └────────┬────────┘            └──────────┬──────────┘                │
│           │                                │                            │
│           ▼                                ▼                            │
│  ┌─────────────────┐            ┌─────────────────────┐                │
│  │    Grafana      │◄──────────►│OpenSearch Dashboards│                │
│  │ grafana.tasso.  │            │   dash.tasso.dev.br │                │
│  │    dev.br       │            │                     │                │
│  └─────────────────┘            └─────────────────────┘                │
└─────────────────────────────────────────────────────────────────────────┘
```

**Componentes e Responsabilidades:**

| Componente | Responsabilidade |
|------------|------------------|
| **APIs Backend** | Gerar traces (spans) para requisições HTTP, queries DB e chamadas externas; emitir logs JSON correlacionados |
| **Frontend** | Gerar traces para carregamento de página, interações e chamadas API; propagar contexto via headers |
| **OTel Collector** | Receber, processar e rotear telemetria para backends de storage |
| **Tempo** | Armazenar e indexar traces para consulta |
| **OpenSearch** | Armazenar e indexar logs para análise |
| **Grafana/Dashboards** | Visualização unificada com links bidirecionais trace↔logs |

---

## Design de Implementação

### Interfaces Principais

#### .NET - Extensão de Configuração OpenTelemetry

```csharp
// Shared/Observability/OpenTelemetryExtensions.cs
namespace GestAuto.Shared.Observability;

public static class OpenTelemetryExtensions
{
    public static IServiceCollection AddGestAutoObservability(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName)
    {
        var serviceVersion = configuration["ServiceVersion"] ?? "1.0.0";
        var otlpEndpoint = configuration["OTEL_EXPORTER_OTLP_ENDPOINT"] 
            ?? "http://otel-collector:4317";

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName, serviceVersion: serviceVersion))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation(ConfigureAspNetCore)
                .AddHttpClientInstrumentation(ConfigureHttpClient)
                .AddEntityFrameworkCoreInstrumentation(ConfigureEfCore)
                .AddOtlpExporter(o => ConfigureOtlp(o, otlpEndpoint)));

        return services;
    }
}
```

#### Java - Configuração OpenTelemetry Spring Boot

```java
// config/OpenTelemetryConfig.java
@Configuration
public class OpenTelemetryConfig {
    
    @Bean
    public Tracer tracer(OpenTelemetry openTelemetry) {
        return openTelemetry.getTracer("vehicle-evaluation", "1.0.0");
    }
}
```

#### Frontend - Módulo de Telemetria

```typescript
// src/telemetry/index.ts
export function initTelemetry(): void {
  const provider = new WebTracerProvider({
    resource: new Resource({
      [ATTR_SERVICE_NAME]: 'frontend',
      [ATTR_SERVICE_VERSION]: '1.0.0',
    }),
  });
  
  provider.addSpanProcessor(
    new BatchSpanProcessor(new OTLPTraceExporter({
      url: 'https://otel.tasso.dev.br/v1/traces',
    }))
  );
  
  provider.register({ contextManager: new ZoneContextManager() });
  registerInstrumentations({ instrumentations: [getWebAutoInstrumentations()] });
}
```

### Modelos de Dados

#### Formato Padronizado de Log (Todas as APIs)

```json
{
  "timestamp": "2026-01-24T14:30:00.123Z",
  "level": "INFO",
  "message": "Pedido criado com sucesso",
  "service": {
    "name": "commercial",
    "version": "1.0.0"
  },
  "trace": {
    "trace_id": "abc123def456789012345678901234ab",
    "span_id": "789xyz1234567890"
  },
  "context": {
    "user_id": "user-uuid",
    "request_path": "/api/leads",
    "http_method": "POST",
    "http_status": 201
  },
  "error": {
    "type": "ValidationException",
    "message": "Campo obrigatório não preenchido",
    "stack_trace": "..."
  }
}
```

#### Campos por Stack

| Campo | .NET (Serilog) | Java (Logback) | Frontend |
|-------|----------------|----------------|----------|
| `timestamp` | `@timestamp` | `@timestamp` | Automatico |
| `level` | `Level` | `level` | `severity` |
| `message` | `RenderedMessage` | `message` | `body` |
| `service.name` | Resource attribute | Spring property | Resource attribute |
| `trace.trace_id` | Activity.TraceId | MDC.trace_id | Span context |
| `trace.span_id` | Activity.SpanId | MDC.span_id | Span context |

### Endpoints de API

Não há novos endpoints de API. A instrumentação é transparente para consumers.

**Endpoints Ignorados (não geram traces):**

| Aplicação | Endpoints Ignorados |
|-----------|---------------------|
| commercial-api | `/health`, `/ready`, `/swagger/*` |
| stock-api | `/health`, `/ready`, `/swagger/*` |
| vehicle-evaluation | `/actuator/*`, `/swagger-ui/*`, `/v3/api-docs/*` |

---

## Pontos de Integração

### OpenTelemetry Collector

| Origem | Protocolo | Endpoint | Porta |
|--------|-----------|----------|-------|
| commercial-api | OTLP/gRPC | `otel-collector` | 4317 |
| stock-api | OTLP/gRPC | `otel-collector` | 4317 |
| vehicle-evaluation | OTLP/gRPC | `otel-collector` | 4317 |
| frontend | OTLP/HTTP | `otel.tasso.dev.br` | 443 |

**Configuração de Exportação:**

```yaml
# Parâmetros compartilhados
batch_size: 512
export_timeout: 5s
queue_size: 2048
retry_on_failure: true
```

### Keycloak (Extração de User ID)

O `user_id` será extraído do token JWT para enriquecimento de logs:

- **.NET**: Extraído via `ClaimsPrincipal` no middleware
- **Java**: Extraído via `SecurityContextHolder`
- **Frontend**: Disponível via hook `useAuth()` existente

### Bancos de Dados

| API | ORM | Instrumentação |
|-----|-----|----------------|
| commercial | Entity Framework Core | `AddEntityFrameworkCoreInstrumentation()` |
| stock | Entity Framework Core | `AddEntityFrameworkCoreInstrumentation()` |
| vehicle-evaluation | JPA/Hibernate | Auto-instrumentação JDBC via Spring Boot Starter |

---

## Análise de Impacto

| Componente Afetado | Tipo de Impacto | Descrição & Nível de Risco | Ação Requerida |
|--------------------|-----------------|---------------------------|----------------|
| `commercial-api/Program.cs` | Mudança de configuração | Adicionar configuração OTel. Baixo risco. | Adicionar pacotes NuGet e configuração |
| `commercial-api/*.csproj` | Dependências | 5 novos pacotes NuGet. Baixo risco. | Validar compatibilidade |
| `stock-api/Program.cs` | Mudança de configuração | Adicionar configuração OTel. Baixo risco. | Adicionar pacotes e configuração |
| `stock-api/*.csproj` | Dependências | 5 novos pacotes NuGet. Baixo risco. | Validar compatibilidade |
| `vehicle-evaluation/pom.xml` | Dependências | 4 novas dependências Maven. Baixo risco. | Adicionar ao parent pom |
| `vehicle-evaluation/application.yml` | Configuração | Novos properties OTel. Baixo risco. | Adicionar configuração |
| `vehicle-evaluation/logback-spring.xml` | Novo arquivo | Configuração de logging. Baixo risco. | Criar arquivo |
| `frontend/package.json` | Dependências | 8 novos pacotes npm. Baixo risco. | npm install |
| `frontend/src/main.tsx` | Inicialização | Chamar `initTelemetry()`. Baixo risco. | Modificar entry point |
| `frontend/src/lib/api.ts` | Interceptor | Adicionar propagação de headers. Médio risco. | Modificar interceptors |
| Docker Compose | Variáveis de ambiente | Novas env vars OTel. Baixo risco. | Atualizar docker-compose |

---

## Abordagem de Testes

### Testes Unitários

**Componentes a testar:**

1. **OpenTelemetryExtensions (.NET)**
   - Verificar que `AddGestAutoObservability` registra serviços corretamente
   - Mock do `IConfiguration` para diferentes cenários

2. **Axios Interceptors (Frontend)**
   - Verificar injeção de headers W3C quando telemetria ativa
   - Verificar que não injeta headers em desenvolvimento

3. **Log Enrichers**
   - Verificar formato JSON de saída
   - Verificar inclusão de trace_id/span_id quando disponíveis

**Cenários Críticos:**

```csharp
// .NET - Verificar filtro de health checks
[Fact]
public void AspNetCoreInstrumentation_ShouldIgnoreHealthEndpoints()
{
    // Arrange
    var httpContext = CreateMockHttpContext("/health");
    
    // Act
    var shouldInstrument = _filter(httpContext);
    
    // Assert
    shouldInstrument.Should().BeFalse();
}
```

### Testes de Integração

**Localização:** `tests/integration/observability/`

**Cenários:**

1. **Trace End-to-End**
   - Simular requisição HTTP
   - Verificar que span é criado com atributos corretos
   - Verificar propagação de contexto entre serviços

2. **Log Correlation**
   - Emitir log dentro de um span ativo
   - Verificar que log contém trace_id e span_id corretos

3. **Export Resilience**
   - Simular falha do collector
   - Verificar que aplicação continua funcionando
   - Verificar que fila de exportação respeita limite

**Setup de Teste:**

```yaml
# docker-compose.test.yml
services:
  otel-collector-mock:
    image: otel/opentelemetry-collector-contrib:latest
    command: ["--config=/etc/otel-config.yaml"]
    volumes:
      - ./otel-config-test.yaml:/etc/otel-config.yaml
    ports:
      - "4317:4317"
```

---

## Sequenciamento de Desenvolvimento

### Ordem de Construção

| Fase | Componente | Justificativa | Duração Estimada |
|------|------------|---------------|------------------|
| 1 | **commercial-api** | Base .NET mais completa, serve de template | 2 dias |
| 2 | **stock-api** | Cópia da configuração de commercial | 0.5 dia |
| 3 | **vehicle-evaluation** | Stack diferente (Java), requer configuração específica | 2 dias |
| 4 | **frontend** | Depende das APIs para teste E2E | 1.5 dias |
| 5 | **Validação E2E** | Testes de trace completo frontend→backend | 1 dia |
| 6 | **Documentação** | Dashboards, runbooks | 0.5 dia |

**Total:** ~7.5 dias

### Dependências Técnicas

| Dependência | Status | Bloqueante? |
|-------------|--------|-------------|
| OpenTelemetry Collector | ✅ Disponível | Não |
| Tempo (trace storage) | ✅ Disponível | Não |
| OpenSearch (log storage) | ✅ Disponível | Não |
| Grafana datasources | ✅ Configurados | Não |
| Network Docker (otel-collector) | ⚠️ Verificar | Sim - primeiro passo |

---

## Monitoramento e Observabilidade

### Métricas Expostas

> **Nota:** Métricas (Prometheus) estão fora do escopo desta fase. Apenas traces e logs serão implementados.

### Logs Principais

| Evento | Nível | Exemplo |
|--------|-------|---------|
| Requisição recebida | INFO | `"Requisição recebida: POST /api/leads"` |
| Operação de negócio | INFO | `"Lead criado com sucesso: id={LeadId}"` |
| Validação falhou | WARN | `"Validação falhou: {Mensagem}"` |
| Erro inesperado | ERROR | `"Erro ao processar requisição"` + stack trace |
| Query lenta | WARN | `"Query executada em {Duration}ms"` (>500ms) |

### Dashboards Grafana

**Dashboard: GestAuto - Service Overview**

Painéis propostos:
1. **Requests/s por serviço** (Tempo)
2. **Latência P50/P90/P99 por endpoint** (Tempo)
3. **Taxa de erros por serviço** (Tempo)
4. **Top 10 endpoints mais lentos** (Tempo)
5. **Traces com erro** (Tempo + link OpenSearch)

**Queries Tempo:**

```promql
# Taxa de requisições
rate(traces_spanmetrics_calls_total{service_name="commercial"}[5m])

# Latência P99
histogram_quantile(0.99, rate(traces_spanmetrics_latency_bucket{service_name="commercial"}[5m]))
```

---

## Considerações Técnicas

### Decisões Principais

| Decisão | Justificativa | Alternativas Rejeitadas |
|---------|---------------|------------------------|
| **OTLP/gRPC para backends** | Menor overhead, melhor performance | HTTP/JSON (maior payload) |
| **OTLP/HTTP para frontend** | CORS-friendly, funciona via Traefik | gRPC (não suportado em browsers) |
| **Batch export a cada 5s** | Balance entre latência e throughput | Imediato (muito I/O), 30s (muito delay) |
| **Fila máxima de 2048 spans** | Evita memory pressure | Ilimitado (risco OOM) |
| **Serilog .NET (manter)** | Já instalado em commercial-api | Trocar por Microsoft.Extensions.Logging puro |
| **Logback Java** | Padrão Spring Boot | Log4j2 (menos integrado com OTel) |

### Riscos Conhecidos

| Risco | Probabilidade | Impacto | Mitigação |
|-------|---------------|---------|-----------|
| Overhead de instrumentação > 5% | Baixa | Médio | Benchmark antes/depois; ajustar sampling se necessário |
| Collector indisponível | Baixa | Baixo | Exportação com retry e fallback silencioso |
| Volume de logs alto | Média | Médio | Log level INFO em produção; sampling de traces se necessário |
| CORS bloqueando frontend | Baixa | Alto | Collector já configurado com headers CORS |
| Incompatibilidade de versão OTel | Baixa | Médio | Usar versões LTS; testar em staging |

### Requisitos Especiais

**Performance:**
- Latência adicional por instrumentação: < 2ms por requisição
- Memory footprint OTel SDK: < 50MB adicional por aplicação
- CPU overhead: < 2% adicional

**Segurança:**
- Sanitização obrigatória de URLs com tokens
- Headers `Authorization` não incluídos em traces
- Dados sensíveis (CPF, senha) mascarados em logs

### Conformidade com Padrões

| Padrão | Conformidade | Observações |
|--------|--------------|-------------|
| [dotnet-logging.md](../rules/dotnet-logging.md) | ✅ Conforme | Segue formato JSON e configuração OTel |
| [dotnet-observability.md](../rules/dotnet-observability.md) | ✅ Conforme | Health checks mantidos e filtrados |
| [java-logging.md](../rules/java-logging.md) | ✅ Conforme | Segue formato JSON e Logback config |
| [java-observability.md](../rules/java-observability.md) | ✅ Conforme | Actuator endpoints filtrados |
| [react-logging.md](../rules/react-logging.md) | ✅ Conforme | Telemetria apenas em produção |
| [restful.md](../rules/restful.md) | N/A | Não afeta design de API |

---

## Apêndice: Configurações Detalhadas

### A. Pacotes NuGet (.NET)

```xml
<!-- Adicionar aos projetos .API -->
<PackageReference Include="OpenTelemetry" Version="1.7.0" />
<PackageReference Include="OpenTelemetry.Exporter.OpenTelemetryProtocol" Version="1.7.0" />
<PackageReference Include="OpenTelemetry.Extensions.Hosting" Version="1.7.0" />
<PackageReference Include="OpenTelemetry.Instrumentation.AspNetCore" Version="1.7.0" />
<PackageReference Include="OpenTelemetry.Instrumentation.Http" Version="1.7.0" />
<PackageReference Include="OpenTelemetry.Instrumentation.EntityFrameworkCore" Version="1.0.0-beta.11" />
```

### B. Dependências Maven (Java)

```xml
<properties>
    <opentelemetry.version>1.32.0</opentelemetry.version>
    <opentelemetry-instrumentation.version>2.0.0</opentelemetry-instrumentation.version>
</properties>

<dependencies>
    <dependency>
        <groupId>io.opentelemetry</groupId>
        <artifactId>opentelemetry-api</artifactId>
        <version>${opentelemetry.version}</version>
    </dependency>
    <dependency>
        <groupId>io.opentelemetry</groupId>
        <artifactId>opentelemetry-sdk</artifactId>
        <version>${opentelemetry.version}</version>
    </dependency>
    <dependency>
        <groupId>io.opentelemetry</groupId>
        <artifactId>opentelemetry-exporter-otlp</artifactId>
        <version>${opentelemetry.version}</version>
    </dependency>
    <dependency>
        <groupId>io.opentelemetry.instrumentation</groupId>
        <artifactId>opentelemetry-spring-boot-starter</artifactId>
        <version>${opentelemetry-instrumentation.version}</version>
    </dependency>
    <dependency>
        <groupId>io.opentelemetry.instrumentation</groupId>
        <artifactId>opentelemetry-logback-appender-1.0</artifactId>
        <version>${opentelemetry-instrumentation.version}</version>
    </dependency>
</dependencies>
```

### C. Pacotes npm (Frontend)

```bash
npm install @opentelemetry/api \
            @opentelemetry/sdk-trace-web \
            @opentelemetry/exporter-trace-otlp-http \
            @opentelemetry/context-zone \
            @opentelemetry/auto-instrumentations-web \
            @opentelemetry/resources \
            @opentelemetry/semantic-conventions \
            @opentelemetry/instrumentation
```

### D. Variáveis de Ambiente

```yaml
# docker-compose.yml - adicionar a cada serviço
environment:
  - OTEL_SERVICE_NAME=${SERVICE_NAME}
  - OTEL_SERVICE_VERSION=1.0.0
  - OTEL_EXPORTER_OTLP_ENDPOINT=http://otel-collector:4317
  - OTEL_EXPORTER_OTLP_PROTOCOL=grpc
  - OTEL_TRACES_EXPORTER=otlp
  - OTEL_LOGS_EXPORTER=otlp
  - OTEL_METRICS_EXPORTER=none
```

---

## Histórico de Revisões

| Data | Versão | Autor | Descrição |
|------|--------|-------|-----------|
| 2026-01-24 | 1.0 | - | Versão inicial da Tech Spec |
