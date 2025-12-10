## markdown

## status: pending

<task_context>
<domain>infra</domain>
<type>documentation</type>
<scope>deployment</scope>
<complexity>low</complexity>
<dependencies>http_server</dependencies>
</task_context>

# Tarefa 13.0: Documentação, Deploy e Monitoramento

## Visão Geral

Preparar o microserviço para produção com documentação completa, scripts de deploy, configuração de monitoramento, alertas, e integração com o ecossistema GestAuto existente.

<requirements>
- Documentação técnica completa
- Swagger/OpenAPI 3.0 para APIs
- Scripts de deploy automatizados
- Docker Image otimizado
- Configuração de Prometheus/Grafana
- Health checks customizados
- Logs estruturados com correlação ID
- Integration com API Gateway
- Backup e recovery procedures

</requirements>

## Subtarefas

- [ ] 13.1 Criar documentação técnica (README, Architecture)
- [ ] 13.2 Configurar Swagger/OpenAPI com SpringDoc
- [ ] 13.3 Implementar health checks customizados
- [ ] 13.4 Configurar métricas Prometheus
- [ ] 13.5 Criar dashboards Grafana
- [ ] 13.6 Implementar logs estruturados com MDC
- [ ] 13.7 Otimizar Dockerfile multi-stage
- [ ] 13.8 Criar scripts de deploy
- [ ] 13.9 Configurar alertas e SLOs

## Detalhes de Implementação

### Documentação Swagger

```java
@Configuration
@OpenApiDefinition(
    info = @Info(
        title = "GestAuto Vehicle Evaluation API",
        version = "1.0.0",
        description = "API para gerenciamento de avaliações de veículos seminovos",
        contact = @Contact(name = "GestAuto Team", email = "dev@gestauto.com"),
        license = @License(name = "MIT", url = "https://opensource.org/licenses/MIT")
    ),
    servers = {
        @Server(url = "http://localhost:8080", description = "Development"),
        @Server(url = "https://api.gestauto.com/vehicle-evaluation", description = "Production")
    }
)
public class OpenApiConfig {

    @Bean
    public OpenAPI customOpenAPI() {
        return new OpenAPI()
            .components(new Components()
                .addSecuritySchemes("bearer-key",
                    new SecurityScheme()
                        .type(SecurityScheme.Type.HTTP)
                        .scheme("bearer")
                        .bearerFormat("JWT"))
                .addSecuritySchemes("basicAuth",
                    new SecurityScheme()
                        .type(SecurityScheme.Type.HTTP)
                        .scheme("basic")))
            .addSecurityItem(new SecurityRequirement().addList("bearer-key"));
    }
}
```

### Health Checks Customizados

```java
@Component
public class VehicleEvaluationHealthIndicator implements HealthIndicator {
    private final VehicleEvaluationRepository evaluationRepository;
    private final FipeApiClient fipeApiClient;
    private final ImageStorageService imageStorageService;

    @Override
    public Health health() {
        try {
            // Check database connectivity
            long evaluationCount = evaluationRepository.count();

            // Check external services
            boolean fipeHealthy = fipeApiClient.ping();
            boolean storageHealthy = imageStorageService.healthCheck();

            // Overall health
            if (fipeHealthy && storageHealthy) {
                return Health.up()
                    .withDetail("database", "UP")
                    .withDetail("evaluations_count", evaluationCount)
                    .withDetail("fipe_api", "UP")
                    .withDetail("storage", "UP")
                    .build();
            } else {
                return Health.down()
                    .withDetail("fipe_api", fipeHealthy ? "UP" : "DOWN")
                    .withDetail("storage", storageHealthy ? "UP" : "DOWN")
                    .build();
            }
        } catch (Exception e) {
            return Health.down(e).build();
        }
    }
}
```

### Métricas Customizadas

```java
@Configuration
public class MetricsConfig {

    @Bean
    public MeterRegistryCustomizer<MeterRegistry> metricsCommonTags() {
        return registry -> registry.config().commonTags(
            "application", "vehicle-evaluation",
            "version", "1.0.0"
        );
    }

    @Bean
    public TimedAspect timedAspect(MeterRegistry registry) {
        return new TimedAspect(registry);
    }

    @Bean
    public CountedAspect countedAspect(MeterRegistry registry) {
        return new CountedAspect(registry);
    }
}

// Metrics nos handlers
@Service
public class CreateEvaluationHandler {
    private final MeterRegistry meterRegistry;
    private final Counter evaluationCreatedCounter;

    public CreateEvaluationHandler(MeterRegistry meterRegistry) {
        this.meterRegistry = meterRegistry;
        this.evaluationCreatedCounter = Counter.builder("vehicle.evaluations.created")
            .description("Number of evaluations created")
            .register(meterRegistry);
    }

    @Timed(value = "vehicle.evaluation.create.duration", description = "Time to create evaluation")
    @Counted(value = "vehicle.evaluation.create.attempts", description = "Attempts to create evaluation")
    public UUID handle(CreateEvaluationCommand command) {
        // ... implementation ...
        evaluationCreatedCounter.increment();
        return evaluationId;
    }
}
```

### Logs Estruturados com Correlation ID

```java
@Component
public class CorrelationFilter implements Filter {
    private static final String CORRELATION_ID_HEADER = "X-Correlation-ID";
    private static final String CORRELATION_ID_MDC_KEY = "correlationId";

    @Override
    public void doFilter(ServletRequest request, ServletResponse response, FilterChain chain)
            throws IOException, ServletException {

        HttpServletRequest httpRequest = (HttpServletRequest) request;
        HttpServletResponse httpResponse = (HttpServletResponse) response;

        String correlationId = httpRequest.getHeader(CORRELATION_ID_HEADER);
        if (correlationId == null || correlationId.isEmpty()) {
            correlationId = UUID.randomUUID().toString();
        }

        MDC.put(CORRELATION_ID_MDC_KEY, correlationId);
        httpResponse.setHeader(CORRELATION_ID_HEADER, correlationId);

        try {
            chain.doFilter(request, response);
        } finally {
            MDC.remove(CORRELATION_ID_MDC_KEY);
        }
    }
}

@Configuration
public class LoggingConfig {
    @Bean
    public Logger structuredLogger() {
        return LoggerFactory.getLogger("com.gestauto.vehicleevaluation.structured");
    }
}
```

### Dockerfile Otimizado

```dockerfile
# Build stage
FROM openjdk:21-jdk-slim AS build
WORKDIR /build

# Copy Maven wrapper and pom.xml
COPY mvnw .
COPY .mvn .mvn
COPY pom.xml .

# Download dependencies
RUN ./mvnw dependency:go-offline -B

# Copy source code
COPY src ./src

# Build application
RUN ./mvnw clean package -DskipTests -Dspring.profiles.active=prod

# Runtime stage
FROM openjdk:21-jre-slim

# Install healthcheck tools
RUN apt-get update && apt-get install -y \
    curl \
    && rm -rf /var/lib/apt/lists/*

# Create non-root user
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Set working directory
WORKDIR /app

# Copy JAR
COPY --from=build /build/target/vehicle-evaluation-*.jar app.jar

# Change ownership
RUN chown appuser:appuser app.jar

# Switch to non-root user
USER appuser

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:8080/actuator/health || exit 1

# Expose port
EXPOSE 8080

# JVM options
ENV JAVA_OPTS="-Xms256m -Xmx512m -XX:+UseG1GC -XX:+UseContainerSupport"

# Run application
ENTRYPOINT ["sh", "-c", "java $JAVA_OPTS -jar app.jar"]
```

### Scripts de Deploy

```bash
#!/bin/bash
# deploy.sh

set -e

# Configuration
APP_NAME="vehicle-evaluation"
DOCKER_REGISTRY="registry.gestauto.com"
DOCKER_IMAGE="${DOCKER_REGISTRY}/${APP_NAME}:${VERSION:-latest}"
NAMESPACE="gestauto"
REPLICAS=3

echo "Deploying ${APP_NAME} version ${VERSION:-latest}..."

# Build and push Docker image
echo "Building Docker image..."
docker build -t ${DOCKER_IMAGE} .

echo "Pushing Docker image..."
docker push ${DOCKER_IMAGE}

# Deploy to Kubernetes
echo "Deploying to Kubernetes..."
kubectl apply -f k8s/namespace.yaml
kubectl apply -f k8s/configmap.yaml
kubectl apply -f k8s/secret.yaml

# Update deployment
sed "s|IMAGE_PLACEHOLDER|${DOCKER_IMAGE}|g" k8s/deployment.yaml | kubectl apply -f -

# Scale deployment
kubectl scale deployment ${APP_NAME} --replicas=${REPLICAS} -n ${NAMESPACE}

# Wait for rollout
kubectl rollout status deployment/${APP_NAME} -n ${NAMESPACE} --timeout=300s

# Verify deployment
kubectl get pods -n ${NAMESPACE} -l app=${APP_NAME}

echo "Deployment completed successfully!"

# Run smoke tests
./scripts/smoke-test.sh
```

### Dashboard Grafana

```json
{
  "dashboard": {
    "title": "Vehicle Evaluation Service",
    "panels": [
      {
        "title": "Evaluations per Minute",
        "type": "graph",
        "targets": [
          {
            "expr": "rate(vehicle_evaluations_created_total[5m])",
            "legendFormat": "Created/min"
          },
          {
            "expr": "rate(vehicle_evaluations_approved_total[5m])",
            "legendFormat": "Approved/min"
          }
        ]
      },
      {
        "title": "Response Time",
        "type": "graph",
        "targets": [
          {
            "expr": "histogram_quantile(0.95, rate(http_server_requests_duration_seconds_bucket[5m]))",
            "legendFormat": "95th percentile"
          },
          {
            "expr": "histogram_quantile(0.50, rate(http_server_requests_duration_seconds_bucket[5m]))",
            "legendFormat": "50th percentile"
          }
        ]
      },
      {
        "title": "Approval Rate",
        "type": "stat",
        "targets": [
          {
            "expr": "rate(vehicle_evaluations_approved_total[1h]) / rate(vehicle_evaluations_submitted_total[1h]) * 100",
            "legendFormat": "Approval Rate %"
          }
        ]
      },
      {
        "title": "External Services Status",
        "type": "table",
        "targets": [
          {
            "expr": "up{job=\"vehicle-evaluation\"}",
            "format": "table"
          }
        ]
      }
    ]
  }
}
```

### README Documentação

```markdown
# GestAuto Vehicle Evaluation Service

## Overview

Microserviço responsável pelo gerenciamento de avaliações de veículos seminovos no sistema GestAuto.

## Architecture

- **Language**: Java 21
- **Framework**: Spring Boot 3.2
- **Database**: PostgreSQL with dedicated schema `vehicle_evaluation`
- **Cache**: Redis
- **Message Broker**: RabbitMQ
- **Storage**: Cloudflare R2 (S3-compatible)
- **Documentation**: OpenAPI 3.0

## Key Features

- Vehicle evaluation workflow
- Photo documentation management
- Technical checklist
- Automatic valuation based on FIPE
- Approval workflow
- PDF report generation

## Quick Start

```bash
# Build
./mvnw clean package

# Run with Docker
docker build -t gestauto/vehicle-evaluation .
docker run -p 8080:8080 gestauto/vehicle-evaluation

# Access API docs
http://localhost:8080/swagger-ui.html
```

## Environment Variables

See `application.yml` for all configuration options.

## Monitoring

- Health: http://localhost:8080/actuator/health
- Metrics: http://localhost:8080/actuator/prometheus
- Logs: Structured JSON with correlation ID
```

## Critérios de Sucesso

- [x] Documentação completa gerada
- [x] Swagger/OpenAPI funcionando
- [x] Health checks customizados
- [x] Métricas Prometheus disponíveis
- [x] Dashboard Grafana criado
- [x] Logs estruturados implementados
- [x] Docker otimizado
- [x] Scripts de deploy funcionando
- [x] Smoke tests executando

## Sequenciamento

- Bloqueado por: Todas as tarefas anteriores
- Desbloqueia: Produção
- Paralelizável: Não

## Tempo Estimado

- Documentação: 8 horas
- Monitoramento: 8 horas
- Deploy/CI-CD: 6 horas
- Testes finais: 4 horas
- Total: 26 horas