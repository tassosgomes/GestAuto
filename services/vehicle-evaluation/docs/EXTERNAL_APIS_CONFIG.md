# Documentação de Limites e Configurações de APIs Externas

## API FIPE (Parallelum)

### Limites de Taxa (Rate Limiting)
- **Limite**: 100 requisições por minuto
- **Estratégia**: Token Bucket com Bucket4j
- **Comportamento**: Requisições excedentes são rejeitadas com erro de rate limit

### Timeouts
- **Timeout de Resposta**: 5 segundos
- **Timeout de Conexão**: 2 segundos
- **Timeout de Escrita**: 5 segundos
- **Timeout de Leitura**: 5 segundos

### Circuit Breaker
- **Limiar de Falha**: 50% (se 5 de 10 requisições falharem)
- **Sliding Window**: 10 requisições
- **Mínimo de Chamadas**: 3 (precisa de 3 chamadas antes de avaliar taxa de falha)
- **Estado Aberto**: 30 segundos antes de tentar recuperação

### Retry
- **Tentativas Máximas**: 3 (1 original + 2 retries)
- **Delay Inicial**: 1 segundo
- **Multiplicador**: 2x (exponencial: 1s, 2s, 4s)

### Endpoints

#### GET /carros/marcas
Obter lista de marcas de veículos

**Exemplo de Resposta:**
```json
[
  {
    "id": "1",
    "nome": "Fiat"
  },
  {
    "id": "2",
    "nome": "Chevrolet"
  }
]
```

**Caching**: 24 horas
**Rate Limit**: 1 requisição por minuto (recomendado)

#### GET /carros/marcas/{id}/modelos
Obter modelos de uma marca específica

**Parâmetros:**
- `id` (path): ID da marca

**Exemplo:**
```
GET /carros/marcas/1/modelos
```

**Exemplo de Resposta:**
```json
[
  {
    "id": "6",
    "nome": "Uno"
  },
  {
    "id": "8",
    "nome": "Palio"
  }
]
```

**Caching**: 24 horas
**Rate Limit**: Contabilizado no limite geral (100/min)

#### GET /carros/marcas/{brandId}/modelos/{modelId}/anos/{year}
Obter informações de preço de um veículo

**Parâmetros:**
- `brandId` (path): ID da marca
- `modelId` (path): ID do modelo
- `year` (path): Ano do veículo (ex: 2023 ou 2023-1 para versões específicas)

**Exemplo:**
```
GET /carros/marcas/1/modelos/6/anos/2023
```

**Exemplo de Resposta:**
```json
{
  "valor": "R$ 45.250,00",
  "marca": "Fiat",
  "modelo": "Uno",
  "anoModelo": "2023",
  "combustivel": "Gasolina",
  "mesReferencia": "janeiro de 2025",
  "tipoVeiculo": 1
}
```

**Caching**: 24 horas
**Rate Limit**: Contabilizado no limite geral (100/min)

### Códigos de Erro Comuns

| Código | Descrição |
|--------|-----------|
| 400 | Bad Request - Parâmetros inválidos |
| 404 | Not Found - Marca, modelo ou ano não encontrado |
| 429 | Too Many Requests - Rate limit excedido |
| 503 | Service Unavailable - API em manutenção |

### Fallback Strategies

Quando a API FIPE está indisponível:
1. **Circuit Breaker Aberto**: Retorna lista vazia (fallback)
2. **Cache**: Tenta usar dados cacheados (até 24 horas)
3. **Erro Controlado**: Lança `FipeApiException` com código de erro específico

### Monitoramento e Métricas

#### Métricas Disponíveis
- `fipe.api.calls` - Contador de chamadas (com tags: endpoint, status)
- `fipe.api.fallback` - Contador de fallbacks acionados
- `rate_limiter.requests` - Contador de requisições (com tags: client, status)

#### Health Check
Endpoint: `GET /health`

```json
{
  "status": "UP",
  "components": {
    "externalApis": {
      "status": "UP",
      "details": {
        "fipe-api": "UP",
        "cloudflare-r2": "UP"
      }
    }
  }
}
```

---

## Cloudflare R2 (S3-Compatible Object Storage)

### Configuração Necessária

```yaml
app:
  external-apis:
    cloudflare-r2:
      endpoint: https://your-account-id.r2.cloudflarestorage.com
      access-key: ${CLOUDFLARE_R2_ACCESS_KEY}
      secret-key: ${CLOUDFLARE_R2_SECRET_KEY}
      bucket-name: vehicle-evaluation-photos
      timeout: 10
      part-size: 5242880  # 5MB
      max-connections: 50
```

### Limites e Quotas

| Item | Limite |
|------|--------|
| Tamanho Máximo de Arquivo | 5GB por arquivo |
| Tamanho Total de Upload | Ilimitado (por conta) |
| Conexões Simultâneas | 50 |
| Timeout de Operação | 10 segundos |
| Tamanho de Parte (Multipart) | 5MB |

### Operações Suportadas

#### 1. Upload de Imagem
```java
String url = imageStorageService.uploadImage(
    inputStream,
    "photo-1.jpg",
    "image/jpeg"
);
```

**Retorna**: URL pública da imagem armazenada

#### 2. Download de Imagem
```java
String presignedUrl = imageStorageService.generatePresignedUrl(
    imageUrl,
    60  // válido por 60 minutos
);
```

**Retorna**: URL pré-assinada para download direto

#### 3. Deleção de Imagem
```java
boolean deleted = imageStorageService.deleteImage(imageUrl);
```

**Retorna**: true se sucesso, false caso contrário

### Estrutura de Pastas em R2

```
vehicle-evaluation-photos/
├── evaluations/
│   ├── {uuid-1}/
│   │   ├── {timestamp}-photo-1.jpg
│   │   ├── {timestamp}-photo-2.jpg
│   │   └── ...
│   ├── {uuid-2}/
│   │   └── ...
```

### Tratamento de Erros

| Exceção | Significado |
|---------|-----------|
| `ImageStorageException` | Erro genérico de armazenamento |
| `S3Exception` | Erro da API S3/R2 |
| `IOException` | Erro de leitura de stream |

### Métricas de Armazenamento

- `image_storage.upload.duration` - Duração do upload (em milissegundos)
- `image_storage.uploads` - Contador de uploads (com status)
- `image_storage.deletes` - Contador de deleções (com status)

---

## Configuração em Diferentes Ambientes

### Desenvolvimento
```yaml
# application-dev.yml
app:
  external-apis:
    fipe:
      base-url: https://parallelum.com.br/fipe/api/v1
      timeout: 5
    cloudflare-r2:
      endpoint: http://minio:9000  # Mock local
      bucket-name: vehicle-evaluation-local
```

### Produção
```yaml
# application-prod.yml
app:
  external-apis:
    fipe:
      base-url: https://parallelum.com.br/fipe/api/v1
      timeout: 5
      rate-limit-per-minute: 100
    cloudflare-r2:
      endpoint: https://{account-id}.r2.cloudflarestorage.com
      bucket-name: vehicle-evaluation-production
```

---

## Troubleshooting

### FIPE API Retornando 429 (Rate Limited)
1. **Verificar**: Número de chamadas simultâneas
2. **Solução**: Implementar fila de requisições
3. **Fallback**: Sistema retorna dados em cache

### Cloudflare R2 Lento
1. **Verificar**: Tamanho das imagens
2. **Otimização**: Comprimir imagens antes do upload
3. **Alternativa**: Usar presigned URLs para downloads diretos

### Circuit Breaker Aberto (API Indisponível)
1. **Status**: `GET /health` mostra `DEGRADED`
2. **Duração**: Estado aberto por 30 segundos
3. **Recuperação**: Tenta reconectar automaticamente após 30s

---

## Best Practices

### Para FIPE API
1. ✅ Cachear respostas por 24 horas
2. ✅ Implementar rate limiting de 100 req/min
3. ✅ Usar circuit breaker com timeout de 30s
4. ✅ Implementar retry com backoff exponencial
5. ❌ NÃO fazer chamadas síncronas em loop

### Para Cloudflare R2
1. ✅ Comprimir imagens antes do upload
2. ✅ Usar multipart upload para arquivos grandes
3. ✅ Gerar URLs pré-assinadas para downloads
4. ✅ Implementar limpeza de arquivos antigos
5. ❌ NÃO armazenar em memória antes do upload

### Gerais
1. ✅ Monitorar métricas via Prometheus/Grafana
2. ✅ Configurar alertas para circuit breaker
3. ✅ Registrar logs de erros de API
4. ✅ Testar fallbacks regularmente
5. ✅ Documentar integrações de terceiros
