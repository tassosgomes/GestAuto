---
status: pending
parallelizable: false
blocked_by: []
---

<task_context>
<domain>infra/observability</domain>
<type>configuration</type>
<scope>infrastructure</scope>
<complexity>low</complexity>
<dependencies>otel-collector, tempo, opensearch, traefik</dependencies>
<unblocks>2.0, 4.0</unblocks>
</task_context>

# Tarefa 1.0: Validação de Infraestrutura e Conectividade OTel

## Visão Geral

Antes de iniciar a instrumentação das aplicações, é necessário validar que toda a infraestrutura de observabilidade está funcional e acessível. Esta tarefa garante que o OpenTelemetry Collector, Tempo, OpenSearch e Grafana estão operacionais e prontos para receber telemetria.

<requirements>
- Verificar conectividade com OpenTelemetry Collector (gRPC e HTTP)
- Confirmar que Tempo está recebendo e armazenando traces
- Confirmar que OpenSearch está recebendo logs
- Validar configuração CORS para frontend (otel.tasso.dev.br)
- Verificar datasources no Grafana
- Documentar endpoints e configurações validadas
</requirements>

## Subtarefas

- [x] 1.1 Verificar conectividade com OTel Collector via gRPC (porta 4317 interna)
- [x] 1.2 Verificar conectividade com OTel Collector via HTTP (https://otel.tasso.dev.br)
- [x] 1.3 Testar envio de trace de teste e verificar no Tempo/Grafana
- [x] 1.4 Verificar configuração CORS do Collector para requisições do frontend
- [x] 1.5 Validar que services estão na mesma rede Docker do otel-collector
- [x] 1.6 Documentar endpoints validados e criar checklist de pré-requisitos

## Detalhes de Implementação

### 1.1 Teste de Conectividade gRPC

```bash
# A partir de um container na rede Docker
docker run --rm --network <network_name> curlimages/curl \
  grpcurl -plaintext otel-collector:4317 list
```

### 1.2 Teste de Conectividade HTTP

```bash
# Teste externo via Traefik
curl -v https://otel.tasso.dev.br/v1/traces \
  -H "Content-Type: application/json" \
  -d '{"resourceSpans":[]}'
```

### 1.3 Verificação de CORS

```bash
# Verificar headers CORS
curl -v -X OPTIONS https://otel.tasso.dev.br/v1/traces \
  -H "Origin: https://gestauto.tasso.dev.br" \
  -H "Access-Control-Request-Method: POST"
```

**Headers esperados:**
- `Access-Control-Allow-Origin: *` ou domínio específico
- `Access-Control-Allow-Methods: POST, OPTIONS`
- `Access-Control-Allow-Headers: Content-Type`

### 1.4 Verificação de Rede Docker

```bash
# Verificar que todos os serviços estão na mesma rede
docker network inspect <network_name> | jq '.[0].Containers'
```

### 1.5 Verificação de Datasources Grafana

1. Acessar https://grafana.tasso.dev.br
2. Navegar para Configuration → Data Sources
3. Verificar:
   - Datasource "Tempo" configurado e testado
   - Datasource "OpenSearch" configurado e testado
   - Links entre datasources configurados (trace → logs)

## Critérios de Sucesso

- [ ] Comando gRPC retorna lista de serviços do Collector
- [x] Endpoint HTTP retorna 200 OK para requisição vazia
- [x] Headers CORS presentes na resposta OPTIONS
- [x] Todos os serviços (commercial, stock, vehicle-evaluation) visíveis na rede do Collector
- [x] Datasources Tempo e OpenSearch testados com sucesso no Grafana
- [x] Checklist de pré-requisitos documentado

## Sequenciamento

- **Bloqueado por:** Nenhuma tarefa
- **Desbloqueia:** 2.0 (commercial-api), 4.0 (vehicle-evaluation)
- **Paralelizável:** Não (primeira tarefa do fluxo)

## Riscos e Mitigações

| Risco | Mitigação |
|-------|-----------|
| Collector não acessível via gRPC | Verificar configuração de portas no docker-compose |
| CORS bloqueando frontend | Adicionar configuração CORS no Collector ou Traefik |
| Rede Docker isolada | Verificar/criar rede compartilhada entre serviços |

## Entregáveis

1. Documento de validação com resultados dos testes
2. Atualização do `docker-compose.yml` se necessário
3. Checklist de pré-requisitos para instrumentação
