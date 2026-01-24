---
status: pending
parallelizable: false
blocked_by: ["6.0"]
---

<task_context>
<domain>documentation/observability</domain>
<type>documentation</type>
<scope>operational</scope>
<complexity>low</complexity>
<dependencies>grafana, opensearch-dashboards</dependencies>
<unblocks></unblocks>
</task_context>

# Tarefa 7.0: Dashboards Grafana e Documentação Operacional

## Visão Geral

Criar dashboards no Grafana para visualização de traces e métricas de observabilidade, configurar index patterns no OpenSearch Dashboards para logs, e documentar runbooks operacionais para investigação de incidentes. Esta é a tarefa final que entrega a solução completa para uso operacional.

<requirements>
- Criar dashboard Grafana "GestAuto - Service Overview"
- Configurar index patterns no OpenSearch Dashboards
- Criar link bidirecional Grafana ↔ OpenSearch
- Documentar runbook de investigação de incidentes
- Documentar runbook de análise de performance
- Criar guia de uso para desenvolvedores
</requirements>

## Subtarefas

- [ ] 7.1 Criar dashboard Grafana "GestAuto - Service Overview"
- [ ] 7.2 Configurar datasource links (Tempo → OpenSearch)
- [ ] 7.3 Configurar index pattern no OpenSearch Dashboards
- [ ] 7.4 Criar saved searches para logs comuns
- [ ] 7.5 Documentar runbook de investigação de incidentes
- [ ] 7.6 Documentar runbook de análise de performance
- [ ] 7.7 Criar guia de uso da telemetria para desenvolvedores
- [ ] 7.8 Validar todos os links e dashboards

## Detalhes de Implementação

### 7.1 Dashboard Grafana "GestAuto - Service Overview"

**Painéis propostos:**

| # | Painel | Tipo | Query/Fonte |
|---|--------|------|-------------|
| 1 | Service Health Overview | Status | Lista de serviços com status |
| 2 | Requests/s por Serviço | Time series | Tempo - rate de spans |
| 3 | Latência P50/P90/P99 | Time series | Tempo - histograma |
| 4 | Taxa de Erros | Time series | Tempo - spans com error |
| 5 | Top 10 Endpoints Lentos | Table | Tempo - ordenado por duration |
| 6 | Traces Recentes com Erro | Table | Tempo - filtrado por error |
| 7 | Service Map | Node graph | Tempo - dependências |

**JSON do Dashboard (simplificado):**

```json
{
  "title": "GestAuto - Service Overview",
  "tags": ["gestauto", "observability"],
  "panels": [
    {
      "title": "Requests por Serviço",
      "type": "timeseries",
      "datasource": "Tempo",
      "targets": [
        {
          "expr": "{ resource.service.name =~ \"commercial|stock|vehicle-evaluation|frontend\" }",
          "refId": "A"
        }
      ]
    },
    {
      "title": "Latência P99",
      "type": "timeseries",
      "datasource": "Tempo",
      "targets": [
        {
          "expr": "{ resource.service.name =~ \"commercial|stock|vehicle-evaluation\" } | quantile_over_time(duration, 0.99)",
          "refId": "A"
        }
      ]
    },
    {
      "title": "Traces com Erro",
      "type": "table",
      "datasource": "Tempo",
      "targets": [
        {
          "expr": "{ status = error }",
          "refId": "A"
        }
      ]
    }
  ]
}
```

### 7.2 Configurar Datasource Links

Configurar link do Tempo para OpenSearch para navegação trace→logs:

1. Acessar Grafana → Configuration → Data Sources → Tempo
2. Em "Trace to logs", configurar:
   - Data source: OpenSearch
   - Tags: `service.name`
   - Mapped tags: `trace_id` → `trace.trace_id`
   - Internal link: checked

**Configuração via API:**

```bash
curl -X PUT "https://grafana.tasso.dev.br/api/datasources/1" \
  -H "Content-Type: application/json" \
  -d '{
    "jsonData": {
      "tracesToLogs": {
        "datasourceUid": "opensearch-uid",
        "tags": ["service.name"],
        "mappedTags": [{"key": "trace_id", "value": "trace.trace_id"}],
        "spanStartTimeShift": "-1h",
        "spanEndTimeShift": "1h",
        "filterByTraceID": true,
        "filterBySpanID": false
      }
    }
  }'
```

### 7.3 Index Pattern no OpenSearch Dashboards

1. Acessar https://dash.tasso.dev.br
2. Stack Management → Index Patterns
3. Criar pattern: `logs-*`
4. Time field: `timestamp`
5. Configurar campos formatados:
   - `trace.trace_id`: Link para Grafana/Tempo
   - `level`: Color mapping (ERROR=red, WARN=yellow)

### 7.4 Saved Searches

Criar as seguintes saved searches:

| Nome | Query | Descrição |
|------|-------|-----------|
| Errors - All Services | `level:ERROR` | Todos os erros |
| Errors - Commercial | `level:ERROR AND service.name:commercial` | Erros do commercial |
| Slow Queries | `message:*slow* OR message:*timeout*` | Queries lentas |
| Auth Failures | `level:WARN AND message:*auth*` | Falhas de autenticação |

### 7.5 Runbook: Investigação de Incidentes

Criar `docs/runbooks/incident-investigation.md`:

```markdown
# Runbook: Investigação de Incidentes

## Objetivo
Guia passo-a-passo para investigação de incidentes usando traces e logs.

## Pré-requisitos
- Acesso ao Grafana (https://grafana.tasso.dev.br)
- Acesso ao OpenSearch Dashboards (https://dash.tasso.dev.br)

## Procedimento

### 1. Identificar o Período do Incidente
- Obter timestamp aproximado do início do problema
- Definir janela de tempo (ex: últimos 30 minutos)

### 2. Buscar Traces com Erro no Grafana
1. Acessar dashboard "GestAuto - Service Overview"
2. Selecionar período do incidente no time picker
3. Verificar painel "Traces com Erro"
4. Clicar em um trace para ver detalhes

### 3. Analisar Hierarquia de Spans
1. No trace view, identificar span com erro (marcado em vermelho)
2. Verificar atributos do span:
   - `http.status_code`: código de erro HTTP
   - `exception.message`: mensagem de exceção
   - `exception.stacktrace`: stack trace completo
3. Identificar serviço/endpoint afetado

### 4. Correlacionar com Logs
1. No trace view, clicar em "Logs for this span"
2. Ou copiar `trace_id` e buscar no OpenSearch:
   ```
   trace.trace_id:"<trace_id>"
   ```
3. Analisar logs em ordem cronológica

### 5. Identificar Causa Raiz
- Verificar se erro é em:
  - Validação (código 400)
  - Autenticação (código 401/403)
  - Dependência externa (timeout, connection error)
  - Banco de dados (constraint violation, deadlock)
  - Bug de código (null reference, etc)

### 6. Documentar e Escalar
- Documentar trace_id e timestamp
- Criar issue com evidências
- Escalar se necessário

## Comandos Úteis

### Buscar traces por trace_id (TraceQL)
```
{ trace:id = "abc123..." }
```

### Buscar traces com erro em período
```
{ status = error && resource.service.name = "commercial" }
```

### Buscar logs correlacionados (OpenSearch)
```
trace.trace_id:"abc123..." AND level:ERROR
```
```

### 7.6 Runbook: Análise de Performance

Criar `docs/runbooks/performance-analysis.md`:

```markdown
# Runbook: Análise de Performance

## Objetivo
Identificar gargalos de performance usando traces.

## Procedimento

### 1. Identificar Endpoints Lentos
1. Dashboard "GestAuto - Service Overview" → "Top 10 Endpoints Lentos"
2. Ordenar por P99 latency
3. Identificar outliers (> 1s de latência)

### 2. Analisar Trace de Endpoint Lento
1. Buscar traces do endpoint:
   ```
   { resource.service.name = "commercial" && name = "POST /api/leads" }
   ```
2. Ordenar por duration (desc)
3. Abrir trace mais lento

### 3. Identificar Span Gargalo
1. No waterfall view, identificar span com maior duration
2. Tipos comuns de gargalo:
   - **DB Query**: span EF Core/JDBC com duration alto
   - **HTTP External**: chamada para outro serviço
   - **Processing**: gap entre spans (código de aplicação)

### 4. Ações de Otimização

| Tipo de Gargalo | Ação Recomendada |
|-----------------|------------------|
| Query lenta | Analisar query, adicionar índices |
| N+1 queries | Usar eager loading |
| HTTP externo | Implementar cache, timeout |
| Processing | Profiling de código |

### 5. Validar Melhoria
- Após correção, comparar P99 antes/depois
- Usar benchmark script: `scripts/benchmark-otel.sh`
```

### 7.7 Guia de Uso para Desenvolvedores

Criar `docs/developer-guide-telemetry.md`:

```markdown
# Guia de Telemetria para Desenvolvedores

## Visão Geral
Este guia explica como usar a telemetria OpenTelemetry no GestAuto.

## Para Desenvolvedores Backend (.NET)

### Adicionar Log com Contexto
```csharp
_logger.LogInformation("Lead criado: {LeadId}", lead.Id);
```
O trace_id é automaticamente incluído pelo Serilog.

### Criar Span Manual (raro)
```csharp
using var activity = ActivitySource.StartActivity("ProcessarLead");
activity?.SetTag("lead.id", leadId);
// ... código
```

## Para Desenvolvedores Backend (Java)

### Adicionar Log com Contexto
```java
log.info("Avaliação criada: {}", avaliacao.getId());
```
O trace_id é automaticamente incluído pelo MDC.

## Para Desenvolvedores Frontend

### Usar Hook useTracing
```typescript
const { withSpan } = useTracing();

await withSpan({ spanName: 'submitLeadForm' }, async () => {
  await api.post('/leads', formData);
});
```

## Acessar Traces
1. Grafana: https://grafana.tasso.dev.br
2. Dashboard: "GestAuto - Service Overview"
3. Buscar por endpoint ou trace_id

## Acessar Logs
1. OpenSearch: https://dash.tasso.dev.br
2. Buscar por `trace.trace_id` ou `service.name`

## Boas Práticas
- ✅ Usar logs estruturados (não concatenar strings)
- ✅ Incluir IDs relevantes nos logs (lead_id, user_id)
- ❌ Não logar dados sensíveis (CPF, senha, token)
- ❌ Não criar spans para cada linha de código
```

## Critérios de Sucesso

- [ ] Dashboard "GestAuto - Service Overview" criado e funcional
- [ ] Link Grafana → OpenSearch funcionando (trace_id)
- [ ] Index pattern configurado no OpenSearch Dashboards
- [ ] Saved searches criadas
- [ ] Runbook de investigação documentado
- [ ] Runbook de performance documentado
- [ ] Guia de desenvolvedor documentado
- [ ] Todos os links testados e funcionando

## Sequenciamento

- **Bloqueado por:** 6.0 (Validação E2E)
- **Desbloqueia:** Nenhuma (tarefa final)
- **Paralelizável:** Não (tarefa final)

## Arquivos Criados

| Arquivo | Descrição |
|---------|-----------|
| `docs/runbooks/incident-investigation.md` | Runbook de incidentes |
| `docs/runbooks/performance-analysis.md` | Runbook de performance |
| `docs/developer-guide-telemetry.md` | Guia para desenvolvedores |
| `grafana/dashboards/gestauto-service-overview.json` | Dashboard exportado |

## Entregáveis

1. Dashboard Grafana "GestAuto - Service Overview"
2. Configuração de datasource links
3. Index patterns e saved searches no OpenSearch
4. Runbooks operacionais
5. Guia de telemetria para desenvolvedores
6. Screenshots de dashboards funcionando

## Checklist Final de Entrega

- [ ] Dashboard acessível em https://grafana.tasso.dev.br
- [ ] Link trace→logs funcionando
- [ ] OpenSearch Dashboards com dados de logs
- [ ] Documentação publicada em `/docs/`
- [ ] Equipe treinada no uso básico
- [ ] Runbooks validados com cenário simulado
