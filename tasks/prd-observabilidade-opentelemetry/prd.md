# PRD: Implementação de Observabilidade com OpenTelemetry

## Visão Geral

O sistema GestAuto atualmente carece de visibilidade centralizada sobre o comportamento das aplicações em produção, dificultando a identificação de gargalos de performance, diagnóstico de erros e análise de fluxos distribuídos entre serviços.

Este PRD define a implementação de uma solução completa de observabilidade utilizando **OpenTelemetry** para instrumentação de logs e traces em todas as aplicações do sistema, permitindo monitoramento em tempo real, correlação de eventos entre serviços e diagnóstico rápido de problemas.

**Aplicações no escopo:**
- **commercial-api** (.NET) - API do módulo comercial
- **stock-api** (.NET) - API do módulo de estoque
- **vehicle-evaluation** (Java/Spring Boot) - API de avaliação de veículos
- **frontend** (React) - Interface do usuário

## Objetivos

### Objetivos de Negócio
- Reduzir o tempo médio de diagnóstico de incidentes (MTTD) em **70%**
- Reduzir o tempo médio de resolução de problemas (MTTR) em **50%**
- Aumentar a disponibilidade percebida do sistema para **99.5%**

### Métricas de Sucesso
- **100%** das requisições HTTP gerando traces rastreáveis
- **100%** dos logs correlacionados com Trace ID
- Latência adicionada pela instrumentação **< 5%** do tempo de resposta
- Dashboard operacional disponível com visualização de traces e logs em até **30 dias** após implementação

### Critérios de Aceite Globais
- Todas as 4 aplicações instrumentadas e enviando telemetria
- Traces visíveis no Grafana via Tempo
- Logs visíveis no OpenSearch Dashboards
- Correlação funcional entre logs e traces via Trace ID

## Histórias de Usuário

### US-01: Rastreamento de Requisições
**Como** desenvolvedor/operador do sistema  
**Quero** visualizar o trace completo de uma requisição através de todos os serviços  
**Para que** eu possa identificar gargalos e erros em fluxos distribuídos

**Critérios de Aceite:**
- Traces exibem todos os spans de uma requisição end-to-end
- Cada span mostra duração, status e exceções
- É possível navegar do frontend até as APIs backend

### US-02: Correlação de Logs
**Como** desenvolvedor/operador do sistema  
**Quero** que todos os logs incluam o Trace ID da requisição  
**Para que** eu possa correlacionar logs com traces específicos

**Critérios de Aceite:**
- Logs incluem campos: trace_id, span_id, service_name, timestamp
- É possível filtrar logs por Trace ID no OpenSearch Dashboards
- Link direto do Grafana (trace) para OpenSearch (logs)

### US-03: Monitoramento de Erros
**Como** desenvolvedor/operador do sistema  
**Quero** ser notificado sobre erros e exceções nas aplicações  
**Para que** eu possa agir proativamente antes de impactar usuários

**Critérios de Aceite:**
- Exceções são registradas no span com stack trace
- Logs de erro incluem contexto completo da requisição
- É possível identificar padrões de erro por serviço/endpoint

### US-04: Análise de Performance
**Como** líder técnico  
**Quero** visualizar métricas de latência por serviço e endpoint  
**Para que** eu possa identificar oportunidades de otimização

**Critérios de Aceite:**
- Dashboard mostra P50, P90, P99 de latência por endpoint
- Identificação de endpoints mais lentos
- Histórico de performance ao longo do tempo

## Funcionalidades Principais

### F-01: Instrumentação de Traces

Todas as requisições HTTP devem gerar traces automaticamente com propagação de contexto entre serviços.

**Requisitos Funcionais:**

| ID | Requisito |
|----|-----------|
| RF-01.1 | Todas as requisições HTTP de entrada devem criar um span root |
| RF-01.2 | Chamadas HTTP para outros serviços devem propagar o contexto (W3C Trace Context) |
| RF-01.3 | Traces devem incluir: service_name, service_version, duration, status_code |
| RF-01.4 | Exceções devem ser registradas no span com mensagem e stack trace |
| RF-01.5 | Health checks (/health, /ready) não devem gerar traces |
| RF-01.6 | Queries de banco de dados devem gerar spans filhos (EF Core/.NET, JDBC/Java) |

### F-02: Instrumentação de Logs Estruturados

Logs devem ser estruturados em JSON e enriquecidos com contexto de rastreamento. **Todas as APIs (.NET e Java) devem emitir logs no mesmo formato padronizado** para garantir consistência na análise e correlação.

**Requisitos Funcionais:**

| ID | Requisito |
|----|-----------|
| RF-02.1 | Logs devem ser emitidos em formato JSON estruturado |
| RF-02.2 | Campos obrigatórios: timestamp, level, message, service_name, trace_id, span_id |
| RF-02.3 | Campos opcionais quando disponíveis: user_id, request_path, http_method |
| RF-02.4 | Níveis de log: DEBUG, INFO, WARNING, ERROR |
| RF-02.5 | Logs DEBUG habilitados apenas em ambiente de desenvolvimento |
| RF-02.6 | Informações sensíveis (senhas, tokens, CPF) não devem ser logadas |

**RF-02.7: Formato Padronizado de Log (Obrigatório para todas as APIs)**

Todas as APIs devem emitir logs JSON com a seguinte estrutura:

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
    "trace_id": "abc123def456",
    "span_id": "789xyz"
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

**Campos por Categoria:**

| Categoria | Campo | Tipo | Obrigatório | Descrição |
|-----------|-------|------|-------------|-----------|
| Root | `timestamp` | ISO 8601 | ✅ | Data/hora UTC do log |
| Root | `level` | string | ✅ | DEBUG, INFO, WARN, ERROR |
| Root | `message` | string | ✅ | Mensagem descritiva do evento |
| Service | `service.name` | string | ✅ | Nome do serviço (pasta) |
| Service | `service.version` | string | ✅ | Versão da aplicação |
| Trace | `trace.trace_id` | string | ✅* | ID do trace (quando disponível) |
| Trace | `trace.span_id` | string | ✅* | ID do span atual |
| Context | `context.user_id` | string | ❌ | ID do usuário autenticado |
| Context | `context.request_path` | string | ❌ | Path da requisição HTTP |
| Context | `context.http_method` | string | ❌ | Método HTTP (GET, POST, etc) |
| Context | `context.http_status` | int | ❌ | Status code da resposta |
| Error | `error.type` | string | ❌ | Tipo/classe da exceção |
| Error | `error.message` | string | ❌ | Mensagem de erro |
| Error | `error.stack_trace` | string | ❌ | Stack trace (apenas ERROR) |

> *Trace ID e Span ID são obrigatórios quando há contexto de trace ativo

### F-03: Exportação de Telemetria

Telemetria deve ser enviada ao OpenTelemetry Collector de forma resiliente.

**Requisitos Funcionais:**

| ID | Requisito |
|----|-----------|
| RF-03.1 | Traces e logs devem ser enviados via protocolo OTLP gRPC (interno) |
| RF-03.2 | Frontend deve enviar via OTLP HTTP (externo, através do Traefik) |
| RF-03.3 | Batch de telemetria a cada 10 segundos |
| RF-03.4 | Timeout de exportação de 5 segundos |
| RF-03.5 | Falhas de exportação não devem impactar o funcionamento da aplicação |
| RF-03.6 | Fila de exportação com limite de 100 spans/logs |

### F-04: Instrumentação do Frontend

O frontend React deve rastrear interações do usuário e chamadas de API.

**Requisitos Funcionais:**

| ID | Requisito |
|----|-----------|
| RF-04.1 | Carregamento de página deve gerar trace (document-load) |
| RF-04.2 | Chamadas Axios devem gerar spans com propagação de headers (interceptor global) |
| RF-04.3 | Interações de usuário (click, submit) devem gerar spans |
| RF-04.4 | Erros JavaScript não tratados devem ser capturados e enviados |
| RF-04.5 | Instrumentação habilitada apenas em produção |
| RF-04.6 | Hook customizado useTracing disponível para spans manuais |

## Experiência do Usuário

### Personas

**Desenvolvedor Backend**
- Precisa diagnosticar erros em chamadas de API
- Quer entender o fluxo de uma requisição entre serviços
- Necessita correlacionar logs com código específico

**Operador/SRE**
- Monitora saúde do sistema em tempo real
- Investiga incidentes e identifica causa raiz
- Analisa tendências de performance

**Líder Técnico**
- Revisa métricas de performance semanalmente
- Identifica gargalos para planejamento de otimização
- Valida SLAs e SLOs do sistema

### Fluxos Principais

**Fluxo 1: Investigação de Erro**
1. Operador recebe alerta de erro
2. Acessa Grafana e busca traces com erro no período
3. Identifica trace específico com falha
4. Visualiza spans e identifica serviço/endpoint com problema
5. Clica no link para ver logs correlacionados no OpenSearch
6. Analisa stack trace e contexto para diagnóstico

**Fluxo 2: Análise de Latência**
1. Usuário reporta lentidão em funcionalidade específica
2. Desenvolvedor busca traces do endpoint afetado
3. Identifica span com maior duração
4. Analisa se é chamada de banco, serviço externo ou processamento
5. Implementa otimização baseada em dados concretos

### Requisitos de Interface

- Dashboard Grafana com visão geral de traces por serviço
- Drill-down de trace para visualizar spans hierarquicamente
- OpenSearch Dashboard com filtros por service_name, trace_id, level
- Link bidirecional entre Grafana e OpenSearch

## Restrições Técnicas de Alto Nível

### Infraestrutura Disponível

| Componente | Endpoint Interno | Endpoint Externo |
|------------|------------------|------------------|
| OpenTelemetry Collector | `http://otel-collector:4317` (gRPC) | `https://otel.tasso.dev.br` (HTTP) |
| Tempo (traces) | Interno via Collector | - |
| OpenSearch (logs) | Interno via Collector | - |
| Grafana | - | `https://grafana.tasso.dev.br` |
| OpenSearch Dashboards | - | `https://dash.tasso.dev.br` |

### Requisitos de Performance

- Overhead de instrumentação **< 5%** no tempo de resposta
- Exportação em batch para minimizar I/O de rede
- Processamento assíncrono de telemetria

### Requisitos de Segurança

- Não logar informações sensíveis (senhas, tokens, CPF, dados financeiros)
- Sanitizar URLs que contenham tokens ou IDs sensíveis
- Headers de autenticação não devem ser incluídos em traces

### Integrações Requeridas

- **Keycloak**: Extrair user_id do token JWT para enriquecimento de logs
- **PostgreSQL**: Instrumentação de queries (EF Core, JDBC)
- **RabbitMQ**: Propagação de contexto em mensagens (futuro)

### Compatibilidade de Versões

| Aplicação | Stack | Versão Mínima OTel |
|-----------|-------|-------------------|
| commercial-api | .NET 8 | OpenTelemetry .NET 1.7.0 |
| stock-api | .NET 8 | OpenTelemetry .NET 1.7.0 |
| vehicle-evaluation | Java 21 / Spring Boot 3.x | OpenTelemetry Java 1.32.0 |
| frontend | React 18 / Vite | OpenTelemetry JS 1.x |

## Não-Objetivos (Fora de Escopo)

### Explicitamente Excluído

- **Métricas (metrics)**: Apenas traces e logs serão implementados nesta fase
- **Alertas automáticos**: Configuração de alertas será um PRD separado
- **Instrumentação de RabbitMQ**: Propagação de contexto em filas será fase futura
- **Profiling contínuo**: Não incluído nesta implementação
- **Real User Monitoring (RUM) avançado**: Apenas instrumentação básica do frontend

### Considerações Futuras

- Integração com sistema de alertas (Alertmanager/PagerDuty)
- Métricas de aplicação (Prometheus)
- Distributed tracing em mensageria assíncrona
- Sampling dinâmico baseado em criticidade

### Limitações Conhecidas

- Frontend só envia telemetria em produção (não em desenvolvimento local)
- Logs DEBUG desabilitados em produção para reduzir volume
- Retenção de dados conforme configuração do Tempo/OpenSearch (não definida aqui)

## Questões Respondidas

| ID | Questão | Resposta |
|----|---------|----------|
| QA-01 | O OpenTelemetry Collector já está configurado? | ✅ Sim, endpoint funcional em `https://otel.tasso.dev.br/v1/traces` |
| QA-02 | Qual a política de retenção de traces e logs? | **1 semana** |
| QA-03 | O endpoint OTEL está configurado no Traefik? | ✅ Sim, testado e funcional |
| QA-04 | Há necessidade de autenticação para o collector? | ❌ Não, rede interna para estudo |
| QA-05 | As APIs .NET usam Entity Framework Core? | ✅ Sim, instrumentação de DB incluída no escopo |
| QA-06 | Qual o nome dos serviços (service_name)? | Usar o **nome da pasta do serviço**: `commercial`, `stock`, `vehicle-evaluation`, `frontend` |
| QA-07 | O frontend usa fetch ou Axios? | **Axios v1.13.2** - instância centralizada em `src/lib/api.ts` |
| QA-08 | Há chamadas HTTP entre as APIs? | Possivelmente sim, ou haverá no futuro. **Implementar propagação de contexto** |
| QA-09 | Quem terá acesso aos dashboards? | Inicialmente o próprio desenvolvedor (papel SRE/Dev) |
| QA-10 | Há requisitos LGPD? | ❌ Não formalmente, mas **não exibir dados sensíveis** |
| QA-11 | SLA da stack de observabilidade? | Não é requisito para este projeto |
| QA-12 | Qual a ordem de prioridade? | Qualquer ordem, sem restrição |
| QA-13 | Feature flag para rollout gradual? | ❌ Não necessário |

## Questões em Aberto

*Todas as questões foram respondidas.* ✅

---

## Anexo A: Especificações Técnicas de Referência

> **Nota**: As especificações detalhadas de implementação (código, pacotes, configurações) devem ser documentadas em uma **Tech Spec** separada. O PRD original continha exemplos de implementação que servirão como base para a Tech Spec.

### Resumo de Tecnologias

| Aplicação | Service Name | Pacotes/Dependências Principais |
|-----------|--------------|--------------------------------|
| commercial-api | `commercial` | OpenTelemetry.Exporter.OpenTelemetryProtocol, OpenTelemetry.Instrumentation.AspNetCore, OpenTelemetry.Instrumentation.Http, OpenTelemetry.Instrumentation.EntityFrameworkCore |
| stock-api | `stock` | OpenTelemetry.Exporter.OpenTelemetryProtocol, OpenTelemetry.Instrumentation.AspNetCore, OpenTelemetry.Instrumentation.Http, OpenTelemetry.Instrumentation.EntityFrameworkCore |
| vehicle-evaluation | `vehicle-evaluation` | opentelemetry-spring-boot-starter, opentelemetry-exporter-otlp, opentelemetry-logback-appender |
| frontend | `frontend` | @opentelemetry/sdk-trace-web, @opentelemetry/auto-instrumentations-web, @opentelemetry/exporter-trace-otlp-http |

### Endpoints de Telemetria

```
# APIs (interno - Docker network)
OTEL_EXPORTER_OTLP_ENDPOINT=http://otel-collector:4317

# Frontend (externo - via Traefik/HTTPS)
OTEL_EXPORTER_OTLP_ENDPOINT=https://otel.tasso.dev.br/v1/traces
```

---

## Histórico de Revisões

| Data | Versão | Autor | Descrição |
|------|--------|-------|-----------|
| 2026-01-24 | 1.0 | - | Versão inicial adaptada do PRD técnico |
| 2026-01-24 | 1.1 | - | Questões respondidas; adicionado formato padronizado de logs |
| 2026-01-24 | 1.2 | - | Todas as questões respondidas; confirmado uso de Axios no frontend |
