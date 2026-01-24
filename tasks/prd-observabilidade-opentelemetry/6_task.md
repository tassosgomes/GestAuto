---
status: pending
parallelizable: false
blocked_by: ["5.0"]
---

<task_context>
<domain>testing/e2e</domain>
<type>testing</type>
<scope>integration</scope>
<complexity>medium</complexity>
<dependencies>all-services, grafana, opensearch</dependencies>
<unblocks>7.0</unblocks>
</task_context>

# Tarefa 6.0: Testes de Integração E2E e Validação de Traces

## Visão Geral

Validar que a instrumentação OpenTelemetry está funcionando corretamente em todos os serviços, com traces completos end-to-end (frontend → backend), correlação de logs, e visualização correta no Grafana/Tempo e OpenSearch Dashboards. Esta tarefa também documenta cenários de teste e cria scripts para validação contínua.

<requirements>
- Validar trace completo frontend → commercial-api → stock-api (se houver chamada)
- Validar trace frontend → vehicle-evaluation
- Verificar correlação de logs com trace_id no OpenSearch
- Validar que health checks não geram traces
- Validar formato JSON dos logs em todas as APIs
- Verificar link Grafana → OpenSearch
- Medir overhead de performance
- Criar scripts de validação automatizada
</requirements>

## Subtarefas

- [ ] 6.1 Criar cenários de teste E2E documentados
- [ ] 6.2 Validar trace frontend → commercial-api
- [ ] 6.3 Validar trace frontend → stock-api
- [ ] 6.4 Validar trace frontend → vehicle-evaluation
- [ ] 6.5 Validar correlação de logs no OpenSearch
- [ ] 6.6 Validar filtro de health checks (nenhum trace gerado)
- [ ] 6.7 Validar formato JSON dos logs (schema padronizado)
- [ ] 6.8 Medir overhead de performance (antes/depois)
- [ ] 6.9 Criar script de validação automatizada
- [ ] 6.10 Documentar resultados e evidências

## Detalhes de Implementação

### 6.1 Cenários de Teste E2E

| ID | Cenário | Fluxo | Validação Esperada |
|----|---------|-------|-------------------|
| E2E-01 | Lead Creation | Frontend → commercial-api → DB | Trace com 3+ spans (frontend, API, EF Core) |
| E2E-02 | Stock Query | Frontend → stock-api → DB | Trace com 3+ spans |
| E2E-03 | Vehicle Evaluation | Frontend → vehicle-evaluation → DB | Trace com 3+ spans |
| E2E-04 | Health Check | curl /health | Nenhum trace gerado |
| E2E-05 | Error Handling | Requisição inválida | Span com status ERROR e exception |
| E2E-06 | Log Correlation | Qualquer requisição | Log com mesmo trace_id do span |

### 6.2-6.4 Validação de Traces

**Procedimento de Validação:**

1. **Executar ação no frontend** (ex: criar lead)
2. **Capturar o `traceparent` header** (via DevTools Network)
3. **Extrair trace_id** do header (formato: `00-{trace_id}-{span_id}-01`)
4. **Buscar no Grafana/Tempo** pelo trace_id
5. **Verificar hierarquia de spans:**

```
[frontend] document-load
  └── [frontend] fetch POST /api/leads
        └── [commercial] POST /api/leads
              └── [commercial] EF Core INSERT leads
```

**Query Tempo (TraceQL):**

```promql
{ resource.service.name = "frontend" } | { resource.service.name = "commercial" }
```

### 6.5 Validação de Logs no OpenSearch

**Query OpenSearch:**

```json
{
  "query": {
    "bool": {
      "must": [
        { "match": { "trace.trace_id": "abc123def456789012345678901234ab" } }
      ]
    }
  },
  "sort": [
    { "timestamp": { "order": "asc" } }
  ]
}
```

**Validações:**
- Logs de todos os serviços aparecem com mesmo `trace_id`
- Campos obrigatórios presentes: `timestamp`, `level`, `message`, `service.name`
- Campo `trace.span_id` corresponde ao span do trace

### 6.6 Validação de Filtro de Health Checks

```bash
# Executar várias requisições de health check
for i in {1..10}; do
  curl -s https://api.tasso.dev.br/commercial/health
  curl -s https://api.tasso.dev.br/stock/health
  curl -s https://api.tasso.dev.br/vehicle-evaluation/actuator/health
done

# Verificar no Tempo que não há traces para esses endpoints
# Query: { resource.service.name =~ "commercial|stock|vehicle-evaluation" && name =~ "health|actuator" }
# Resultado esperado: 0 traces
```

### 6.7 Validação de Schema JSON de Logs

Criar script de validação (`scripts/validate-log-schema.sh`):

```bash
#!/bin/bash

# Schema JSON esperado (simplificado)
REQUIRED_FIELDS='["timestamp", "level", "message", "service.name"]'

# Buscar último log de cada serviço
for service in commercial stock vehicle-evaluation; do
  echo "Validando schema de logs: $service"
  
  # Buscar último log via OpenSearch
  log=$(curl -s "https://dash.tasso.dev.br/_search" \
    -H "Content-Type: application/json" \
    -d "{\"query\":{\"match\":{\"service.name\":\"$service\"}},\"size\":1,\"sort\":[{\"timestamp\":\"desc\"}]}" \
    | jq '.hits.hits[0]._source')
  
  # Validar campos obrigatórios
  for field in timestamp level message; do
    if ! echo "$log" | jq -e ".$field" > /dev/null 2>&1; then
      echo "❌ Campo obrigatório ausente: $field"
      exit 1
    fi
  done
  
  # Validar service.name
  if ! echo "$log" | jq -e '.service.name' > /dev/null 2>&1; then
    echo "❌ Campo service.name ausente"
    exit 1
  fi
  
  echo "✅ Schema válido para $service"
done
```

### 6.8 Medição de Overhead de Performance

**Metodologia:**

1. **Baseline (sem instrumentação):**
   - Executar 1000 requisições para cada endpoint
   - Registrar P50, P90, P99 de latência
   - Registrar uso de CPU/memória

2. **Com instrumentação:**
   - Repetir mesmas requisições
   - Comparar métricas

**Script de benchmark (`scripts/benchmark-otel.sh`):**

```bash
#!/bin/bash

ENDPOINTS=(
  "https://api.tasso.dev.br/commercial/api/leads"
  "https://api.tasso.dev.br/stock/api/vehicles"
  "https://api.tasso.dev.br/vehicle-evaluation/api/evaluations"
)

for endpoint in "${ENDPOINTS[@]}"; do
  echo "Benchmark: $endpoint"
  
  # Usar hey ou wrk para benchmark
  hey -n 1000 -c 10 -m GET "$endpoint" | tee "benchmark_$(basename $endpoint).txt"
done
```

**Critério de Aceite:** Overhead < 5% no P99 de latência.

### 6.9 Script de Validação Automatizada

Criar `scripts/validate-otel.sh`:

```bash
#!/bin/bash

set -e

echo "=== Validação de Observabilidade OpenTelemetry ==="

# 1. Verificar conectividade com Collector
echo "[1/6] Verificando conectividade com OTel Collector..."
curl -s -o /dev/null -w "%{http_code}" https://otel.tasso.dev.br/v1/traces \
  -H "Content-Type: application/json" -d '{"resourceSpans":[]}' | grep -q "200"
echo "✅ Collector acessível"

# 2. Verificar serviços no Tempo
echo "[2/6] Verificando serviços no Tempo..."
services=$(curl -s "https://grafana.tasso.dev.br/api/datasources/proxy/1/api/search/tags?tag=service.name" | jq -r '.tagValues[]')
for svc in commercial stock vehicle-evaluation frontend; do
  if echo "$services" | grep -q "$svc"; then
    echo "  ✅ Serviço encontrado: $svc"
  else
    echo "  ❌ Serviço NÃO encontrado: $svc"
    exit 1
  fi
done

# 3. Verificar trace recente de cada serviço
echo "[3/6] Verificando traces recentes..."
for svc in commercial stock vehicle-evaluation frontend; do
  count=$(curl -s "https://grafana.tasso.dev.br/api/datasources/proxy/1/api/search?tags=service.name%3D$svc&limit=1" | jq '.traces | length')
  if [ "$count" -gt 0 ]; then
    echo "  ✅ Traces encontrados para: $svc"
  else
    echo "  ⚠️ Nenhum trace recente para: $svc"
  fi
done

# 4. Verificar logs no OpenSearch
echo "[4/6] Verificando logs no OpenSearch..."
for svc in commercial stock vehicle-evaluation; do
  count=$(curl -s "https://dash.tasso.dev.br/_count" \
    -H "Content-Type: application/json" \
    -d "{\"query\":{\"match\":{\"service.name\":\"$svc\"}}}" | jq '.count')
  if [ "$count" -gt 0 ]; then
    echo "  ✅ Logs encontrados para: $svc ($count)"
  else
    echo "  ❌ Nenhum log para: $svc"
    exit 1
  fi
done

# 5. Verificar correlação trace-log
echo "[5/6] Verificando correlação trace-log..."
# Pegar um trace_id recente
trace_id=$(curl -s "https://grafana.tasso.dev.br/api/datasources/proxy/1/api/search?tags=service.name%3Dcommercial&limit=1" | jq -r '.traces[0].traceID')
if [ -n "$trace_id" ] && [ "$trace_id" != "null" ]; then
  log_count=$(curl -s "https://dash.tasso.dev.br/_count" \
    -H "Content-Type: application/json" \
    -d "{\"query\":{\"match\":{\"trace.trace_id\":\"$trace_id\"}}}" | jq '.count')
  if [ "$log_count" -gt 0 ]; then
    echo "  ✅ Logs correlacionados encontrados para trace: $trace_id"
  else
    echo "  ⚠️ Nenhum log correlacionado para trace: $trace_id"
  fi
fi

# 6. Verificar que health checks não geram traces
echo "[6/6] Verificando filtro de health checks..."
health_traces=$(curl -s "https://grafana.tasso.dev.br/api/datasources/proxy/1/api/search?tags=http.url%3D%2Fhealth&limit=10" | jq '.traces | length')
if [ "$health_traces" -eq 0 ]; then
  echo "  ✅ Health checks corretamente filtrados"
else
  echo "  ⚠️ Encontrados $health_traces traces de health check (esperado: 0)"
fi

echo ""
echo "=== Validação Concluída ==="
```

## Critérios de Sucesso

- [ ] Todos os 6 cenários E2E validados com sucesso
- [ ] Traces visíveis no Grafana para todos os 4 serviços
- [ ] Logs correlacionados no OpenSearch com trace_id
- [ ] Health checks NÃO geram traces
- [ ] Schema JSON de logs consistente entre todas as APIs
- [ ] Overhead de latência < 5%
- [ ] Script de validação automatizada funcionando

## Sequenciamento

- **Bloqueado por:** 5.0 (frontend - completa a instrumentação)
- **Desbloqueia:** 7.0 (Dashboards e Documentação)
- **Paralelizável:** Não (validação final)

## Arquivos Criados

| Arquivo | Descrição |
|---------|-----------|
| `scripts/validate-otel.sh` | Script de validação automatizada |
| `scripts/benchmark-otel.sh` | Script de benchmark de performance |
| `scripts/validate-log-schema.sh` | Validador de schema de logs |
| `docs/observability-validation-report.md` | Relatório de validação |

## Evidências a Documentar

1. **Screenshots:**
   - Trace completo frontend→backend no Grafana
   - Logs correlacionados no OpenSearch
   - Dashboard de serviços

2. **Métricas:**
   - Tabela comparativa de latência (antes/depois)
   - Número de traces por serviço
   - Número de logs por serviço

3. **Logs de exemplo:**
   - JSON de log de cada serviço (commercial, stock, vehicle-evaluation)

## Entregáveis

1. Scripts de validação (`scripts/validate-otel.sh`, etc.)
2. Relatório de validação com evidências
3. Tabela de benchmark de performance
4. Checklist de validação documentado
