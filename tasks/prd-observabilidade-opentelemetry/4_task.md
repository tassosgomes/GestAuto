---
status: pending
parallelizable: true
blocked_by: ["1.0"]
---

<task_context>
<domain>backend/vehicle-evaluation</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>high</complexity>
<dependencies>otel-collector, spring-boot, jpa, logback</dependencies>
<unblocks>5.0</unblocks>
</task_context>

# Tarefa 4.0: Instrumentação OpenTelemetry - vehicle-evaluation (Java/Spring Boot)

## Visão Geral

Implementar instrumentação OpenTelemetry na API vehicle-evaluation (Java 21 / Spring Boot 3.x), utilizando o OpenTelemetry Spring Boot Starter para auto-instrumentação de requisições HTTP, queries JDBC/JPA e logs via Logback. O formato de log JSON deve seguir o padrão estabelecido no PRD para consistência com as APIs .NET.

<requirements>
- Adicionar dependências Maven do OpenTelemetry
- Configurar OpenTelemetry via application.yml
- Configurar auto-instrumentação Spring Boot
- Configurar Logback para logs JSON estruturados com trace context
- Filtrar endpoints do Actuator e Swagger
- Adicionar variáveis de ambiente no docker-compose
</requirements>

## Subtarefas

- [ ] 4.1 Adicionar dependências Maven do OpenTelemetry ao pom.xml
- [ ] 4.2 Configurar properties do OpenTelemetry no application.yml
- [ ] 4.3 Criar classe de configuração OpenTelemetryConfig (se necessário)
- [ ] 4.4 Criar/atualizar logback-spring.xml para logs JSON estruturados
- [ ] 4.5 Configurar filtro de endpoints (actuator, swagger)
- [ ] 4.6 Adicionar variáveis de ambiente OTel no docker-compose
- [ ] 4.7 Criar testes de integração para validar traces
- [ ] 4.8 Validar traces no Grafana/Tempo após deploy

## Detalhes de Implementação

### 4.1 Dependências Maven

Adicionar ao `pom.xml`:

```xml
<properties>
    <opentelemetry.version>1.32.0</opentelemetry.version>
    <opentelemetry-instrumentation.version>2.0.0</opentelemetry-instrumentation.version>
</properties>

<dependencies>
    <!-- OpenTelemetry Core -->
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
    
    <!-- Spring Boot Starter (auto-instrumentação) -->
    <dependency>
        <groupId>io.opentelemetry.instrumentation</groupId>
        <artifactId>opentelemetry-spring-boot-starter</artifactId>
        <version>${opentelemetry-instrumentation.version}</version>
    </dependency>
    
    <!-- Logback Appender para logs com trace context -->
    <dependency>
        <groupId>io.opentelemetry.instrumentation</groupId>
        <artifactId>opentelemetry-logback-appender-1.0</artifactId>
        <version>${opentelemetry-instrumentation.version}</version>
    </dependency>
</dependencies>
```

### 4.2 Configuração application.yml

```yaml
# application.yml
otel:
  service:
    name: vehicle-evaluation
  exporter:
    otlp:
      endpoint: ${OTEL_EXPORTER_OTLP_ENDPOINT:http://otel-collector:4317}
      protocol: grpc
  traces:
    exporter: otlp
  logs:
    exporter: otlp
  metrics:
    exporter: none
  instrumentation:
    http:
      server:
        # Filtrar endpoints que não devem gerar traces
        excluded-urls: /actuator/**,/swagger-ui/**,/v3/api-docs/**
    jdbc:
      enabled: true
    spring-webmvc:
      enabled: true

spring:
  application:
    name: vehicle-evaluation
```

### 4.3 Classe de Configuração (opcional)

```java
// config/OpenTelemetryConfig.java
package br.com.gestauto.vehicleevaluation.config;

import io.opentelemetry.api.OpenTelemetry;
import io.opentelemetry.api.trace.Tracer;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;

@Configuration
public class OpenTelemetryConfig {
    
    @Bean
    public Tracer tracer(OpenTelemetry openTelemetry) {
        return openTelemetry.getTracer("vehicle-evaluation", "1.0.0");
    }
}
```

### 4.4 Configuração Logback (logback-spring.xml)

Criar ou atualizar `src/main/resources/logback-spring.xml`:

```xml
<?xml version="1.0" encoding="UTF-8"?>
<configuration>
    
    <!-- Incluir configuração padrão do Spring Boot -->
    <include resource="org/springframework/boot/logging/logback/defaults.xml"/>
    
    <!-- Appender OpenTelemetry para enviar logs ao Collector -->
    <appender name="OTEL" class="io.opentelemetry.instrumentation.logback.appender.v1_0.OpenTelemetryAppender">
        <captureExperimentalAttributes>true</captureExperimentalAttributes>
        <captureCodeAttributes>true</captureCodeAttributes>
    </appender>
    
    <!-- Appender Console em JSON para stdout (capturado pelo Docker) -->
    <appender name="JSON_CONSOLE" class="ch.qos.logback.core.ConsoleAppender">
        <encoder class="net.logstash.logback.encoder.LogstashEncoder">
            <customFields>
                {"service":{"name":"vehicle-evaluation","version":"1.0.0"}}
            </customFields>
            <fieldNames>
                <timestamp>timestamp</timestamp>
                <level>level</level>
                <message>message</message>
                <logger>[ignore]</logger>
                <thread>[ignore]</thread>
                <levelValue>[ignore]</levelValue>
            </fieldNames>
            <!-- Adicionar trace_id e span_id do MDC -->
            <includeMdcKeyName>trace_id</includeMdcKeyName>
            <includeMdcKeyName>span_id</includeMdcKeyName>
        </encoder>
    </appender>
    
    <!-- Configuração por profile -->
    <springProfile name="!local">
        <root level="INFO">
            <appender-ref ref="JSON_CONSOLE"/>
            <appender-ref ref="OTEL"/>
        </root>
    </springProfile>
    
    <springProfile name="local">
        <root level="DEBUG">
            <appender-ref ref="CONSOLE"/>
        </root>
    </springProfile>
    
</configuration>
```

**Dependência adicional para Logstash Encoder:**

```xml
<dependency>
    <groupId>net.logstash.logback</groupId>
    <artifactId>logstash-logback-encoder</artifactId>
    <version>7.4</version>
</dependency>
```

### 4.5 Filtro de Endpoints

O filtro é configurado via `application.yml` (já incluído em 4.2):

```yaml
otel:
  instrumentation:
    http:
      server:
        excluded-urls: /actuator/**,/swagger-ui/**,/v3/api-docs/**
```

### 4.6 Variáveis de Ambiente Docker

Adicionar ao `docker-compose.yml` (seção do vehicle-evaluation):

```yaml
vehicle-evaluation:
  environment:
    - OTEL_SERVICE_NAME=vehicle-evaluation
    - OTEL_SERVICE_VERSION=1.0.0
    - OTEL_EXPORTER_OTLP_ENDPOINT=http://otel-collector:4317
    - OTEL_EXPORTER_OTLP_PROTOCOL=grpc
    - OTEL_TRACES_EXPORTER=otlp
    - OTEL_LOGS_EXPORTER=otlp
    - OTEL_METRICS_EXPORTER=none
    - SPRING_PROFILES_ACTIVE=prod
```

## Critérios de Sucesso

- [ ] Aplicação inicia sem erros com instrumentação OTel
- [ ] Traces aparecem no Grafana/Tempo com service_name="vehicle-evaluation"
- [ ] Queries JDBC/JPA aparecem como spans filhos
- [ ] Endpoints /actuator/* e /swagger-ui/* NÃO geram traces
- [ ] Logs JSON contêm `trace_id` e `span_id` no formato padronizado
- [ ] Exceções são registradas no span com stack trace
- [ ] Logs DEBUG desabilitados em produção

## Sequenciamento

- **Bloqueado por:** 1.0 (Validação de Infraestrutura)
- **Desbloqueia:** 5.0 (frontend)
- **Paralelizável:** Sim (pode executar em paralelo com 2.0)

## Arquivos Afetados

| Arquivo | Ação | Descrição |
|---------|------|-----------|
| `services/vehicle-evaluation/pom.xml` | Modificar | Adicionar dependências Maven |
| `services/vehicle-evaluation/src/main/resources/application.yml` | Modificar | Configuração OTel |
| `services/vehicle-evaluation/src/main/resources/logback-spring.xml` | Criar/Modificar | Configuração de logs JSON |
| `services/vehicle-evaluation/src/main/java/.../config/OpenTelemetryConfig.java` | Criar | Bean do Tracer |
| `docker-compose.yml` | Modificar | Variáveis de ambiente OTel |

## Diferenças em Relação ao .NET

| Aspecto | .NET (commercial/stock) | Java (vehicle-evaluation) |
|---------|-------------------------|---------------------------|
| Biblioteca | OpenTelemetry .NET SDK | OpenTelemetry Java SDK |
| Auto-instrumentação | Pacotes individuais | Spring Boot Starter |
| ORM | Entity Framework Core | JPA/Hibernate via JDBC |
| Logging | Serilog | Logback + Logstash Encoder |
| Config | appsettings.json | application.yml |

## Riscos e Mitigações

| Risco | Probabilidade | Mitigação |
|-------|---------------|-----------|
| Conflito com Spring Boot 3.x | Baixa | Usar versões compatíveis documentadas |
| Logback encoder não disponível | Baixa | Adicionar dependência logstash-logback-encoder |
| Filtro de endpoints não funcionando | Média | Testar manualmente; usar sampler customizado se necessário |

## Entregáveis

1. Código de instrumentação no vehicle-evaluation
2. Configuração Logback para logs JSON
3. Atualização do docker-compose
4. Screenshot de trace no Grafana comprovando funcionamento
5. Log de exemplo em JSON mostrando trace_id/span_id
