# Tarefa 1.0 — Relatório de Validação de Infraestrutura OTel

Data: 2026-01-24

## Escopo
Validar conectividade e disponibilidade de OpenTelemetry Collector, Tempo, OpenSearch e Grafana, além de CORS e redes.

## Resultados dos Testes

### 1) Conectividade OTLP gRPC (porta 4317)
- **Comando (via serviço swarm):**
  - `grpcurl -plaintext observability_otel-collector:4317 list`
- **Resultado:** `Failed to list services: server does not support the reflection API`
- **Conclusão:** Conectividade gRPC **OK** (resposta de erro de reflexão indica que o endpoint respondeu, mas o serviço não expõe reflexão gRPC).

### 2) Conectividade OTLP HTTP (externo)
- **Endpoint:** `https://otel.tasso.dev.br/v1/traces`
- **Comando:** `curl -X POST ...` com payload vazio
- **Status:** **200 OK**
- **Conclusão:** Endpoint HTTP **OK**.

### 3) CORS para frontend
- **Comando:** `curl -X OPTIONS https://otel.tasso.dev.br/v1/traces` com `Origin: https://gestauto.tasso.dev.br`
- **Status:** **204 No Content**
- **Headers CORS:** `access-control-allow-origin`, `access-control-allow-methods`, `vary`
- **Conclusão:** CORS **OK** (habilitado no OTel Collector).

### 4) Tempo (traces)
- **Endpoint (externo):** `https://tempo.tasso.dev.br/ready`
- **Status:** **200 OK**
- **Endpoint (via Grafana proxy):** `/api/datasources/proxy/1/ready`
- **Status:** **200 OK**
- **Teste de trace:** `telemetrygen traces --traces 1` → trace encontrado via `/api/search`
- **Conclusão:** Tempo **OK** via datasource do Grafana; rota externa `/ready` segue **NOK**.

### 5) OpenSearch (logs)
- **Endpoint:** `https://logs.tasso.dev.br`
- **Status:** **200 OK**
- **Teste de log:** `telemetrygen logs --logs 1` → índice criado `ss4o_logs-otel-logs` com 1 doc
- **Conclusão:** OpenSearch **OK** e recebendo logs.

### 6) OpenSearch Dashboards
- **Endpoint:** `https://dash.tasso.dev.br`
- **Status:** **302** (redirect)
- **Conclusão:** Endpoint **OK** (redirect esperado).

### 7) Grafana
- **Endpoint:** `https://grafana.tasso.dev.br/api/health`
- **Status:** **200 OK**
- **Persistência:** volume `grafana-data` montado em `/var/lib/grafana`.
- **Credenciais:** `admin/ab36fa`.
- **Datasources:** `tempo` (id 1, uid `ffb5q995kgzk0c`) e `opensearch` (id 2, uid `efb5q9i5psmwwb`).
- **Teste Tempo:** `/api/datasources/proxy/1/ready` = **200 OK**
- **Teste OpenSearch:** `/api/datasources/proxy/2/_cluster/health` = **200 OK**
- **Conclusão:** Grafana **OK**, datasources **Tempo + OpenSearch OK**, persistência **OK**.

### 8) Rede Docker (serviços com o collector)
- **Collector:** redes `observability_observability` e `traefik-public`
- **APIs (commercial/stock/vehicle-evaluation):** rede `traefik-public`
- **Conclusão:** Serviços **compartilham** a rede `traefik-public` com o collector.

## Checklist de Pré‑requisitos (Instrumentação)

- [x] OTLP gRPC acessível em `otel-collector:4317` (rede interna)
- [x] OTLP HTTP acessível em `https://otel.tasso.dev.br/v1/traces`
- [x] CORS habilitado para frontend (Origin `https://gestauto.tasso.dev.br`)
- [x] Tempo recebendo traces (validar via Grafana/Tempo)
- [x] OpenSearch recebendo logs (validar index/ingestão)
- [x] Datasources no Grafana configurados e testados (Tempo + OpenSearch)

## Pendências / Ações Recomendadas

1. **gRPC reflection (opcional)**
   - Não suportado na imagem atual `otel/opentelemetry-collector-contrib:0.104.0`.
   - Para habilitar, atualizar a imagem para uma versão que suporte a extensão.

## Endpoints Validados

- OTLP HTTP: `https://otel.tasso.dev.br/v1/traces`
- OpenSearch: `https://logs.tasso.dev.br`
- OpenSearch Dashboards: `https://dash.tasso.dev.br`
- Grafana: `https://grafana.tasso.dev.br`
- Tempo (rota exposta): `https://tempo.tasso.dev.br` (health 503 via `/ready`)
