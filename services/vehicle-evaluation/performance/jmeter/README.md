# Load tests (JMeter)

Este diretório contém um teste de carga **mínimo** em JMeter para validação de performance do serviço.

## Pré-requisitos

- Java 21+
- Apache JMeter instalado (CLI `jmeter` disponível)
- Serviço rodando localmente (por padrão em `http://localhost:8081`)

## Executar

```bash
cd services/vehicle-evaluation
jmeter -n \
  -t performance/jmeter/vehicle-evaluation-smoke.jmx \
  -JHOST=localhost \
  -JPORT=8081 \
  -JPROTOCOL=http \
  -l performance/jmeter/results.jtl
```

## O que o plano faz

- Faz requisições HTTP para endpoints simples (ex.: actuator health)
- Serve como base para evoluir cenários reais (criação/consulta de avaliações) quando necessário
