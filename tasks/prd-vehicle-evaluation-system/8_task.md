## markdown

## status: completed

<task_context>
<domain>engine</domain>
<type>integration</type>
<scope>middleware</scope>
<complexity>medium</complexity>
<dependencies>external_apis</dependencies>
</task_context>

# Tarefa 8.0: Integração com APIs Externas (FIPE, Cloudflare R2)

## Visão Geral

Implementar integrações robustas com APIs externas críticas: API FIPE para consulta de valores de mercado e Cloudflare R2 para armazenamento de fotos, incluindo retry exponencial, circuit breaker, rate limiting, e fallback strategies.

<requirements>
- Cliente FIPE com retry e caching
- SDK S3 para Cloudflare R2
- Tratamento de falhas gracefully
- Monitoramento de latência e erros
- Circuit breaker pattern
- Rate limiting para API externa
- Logs estruturados de integração
- Configuração de timeouts

</requirements>

## Subtarefas

- [ ] 8.1 Configurar WebClient com retry e circuit breaker
- [ ] 8.2 Implementar FipeApiClient com rate limiting
- [ ] 8.3 Configurar S3Client para Cloudflare R2
- [ ] 8.4 Implementar estratégias de fallback
- [ ] 8.5 Adicionar métricas de integração
- [ ] 8.6 Criar configurações específicas
- [ ] 8.7 Implementar health checks
- [ ] 8.8 Adicionar logging estruturado
- [ ] 8.9 Documentar limites da API

## Detalhes de Implementação

### Configuração WebClient com Resilience

```java
@Configuration
public class WebClientConfig {

    @Bean
    public WebClient webClient() {
        return WebClient.builder()
            .baseUrl("https://parallelum.com.br/fipe/api/v1")
            .clientConnector(new ReactorClientHttpConnector(
                HttpClient.create()
                    .responseTimeout(Duration.ofSeconds(5))
                    .option(ChannelOption.CONNECT_TIMEOUT_MILLIS, 2000)
            ))
            .build();
    }

    @Bean
    public ReactiveResilience4JCircuitBreakerFactory circuitBreakerFactory() {
        CircuitBreakerConfig circuitBreakerConfig = CircuitBreakerConfig.custom()
            .failureRateThreshold(50)
            .waitDurationInOpenState(Duration.ofSeconds(30))
            .slidingWindowSize(10)
            .build();

        return new ReactiveResilience4JCircuitBreakerFactory(
            () -> circuitBreakerConfig
        );
    }
}
```

### Cliente FIPE com Resilience

```java
@Service
@Slf4j
public class FipeApiClient {
    private final WebClient webClient;
    private final ReactiveResilience4JCircuitBreakerFactory circuitBreakerFactory;
    private final MeterRegistry meterRegistry;

    @Retry(value = 3, backoff = @Backoff(delay = 1000, multiplier = 2))
    public Mono<FipeVehicleResponse> getVehicleInfo(String fipeCode) {
        CircuitBreaker circuitBreaker = circuitBreakerFactory.create("fipe-api");

        return circuitBreaker.run(
            webClient.get()
                .uri("/carros/marcas")
                .retrieve()
                .bodyToMono(FipeVehicleResponse.class)
                .doOnSuccess(response -> {
                    meterRegistry.counter("fipe.api.calls", "status", "success").increment();
                    log.info("Successfully retrieved FIPE data for code: {}", fipeCode);
                })
                .doOnError(error -> {
                    meterRegistry.counter("fipe.api.calls", "status", "error").increment();
                    log.error("Error calling FIPE API for code: {}", fipeCode, error);
                }),
            throwable -> {
                log.warn("FIPE API circuit breaker is open, using fallback");
                return Mono.empty(); // Fallback para cache
            }
        );
    }
}
```

### Configuração S3 para Cloudflare R2

```java
@Configuration
public class S3Config {

    @Value("${app.cloudflare-r2.endpoint}")
    private String endpoint;

    @Value("${app.cloudflare-r2.access-key}")
    private String accessKey;

    @Value("${app.cloudflare-r2.secret-key}")
    private String secretKey;

    @Bean
    public S3Client s3Client() {
        return S3Client.builder()
            .endpointOverride(URI.create(endpoint))
            .region(Region.US_EAST_1) // R2 usa região global
            .credentialsProvider(StaticCredentialsProvider.create(
                AwsBasicCredentials.create(accessKey, secretKey)
            ))
            .build();
    }

    @Bean
    public S3Presigner s3Presigner() {
        return S3Presigner.builder()
            .endpointOverride(URI.create(endpoint))
            .region(Region.US_EAST_1)
            .credentialsProvider(StaticCredentialsProvider.create(
                AwsBasicCredentials.create(accessKey, secretKey)
            ))
            .build();
    }
}
```

### Rate Limiting e Monitoring

```java
@Component
public class RateLimiterService {
    private final Map<String, Bucket> buckets = new ConcurrentHashMap<>();
    private final MeterRegistry meterRegistry;

    public RateLimiterService(MeterRegistry meterRegistry) {
        this.meterRegistry = meterRegistry;
    }

    public boolean allowRequest(String apiKey) {
        Bucket bucket = buckets.computeIfAbsent(apiKey, key ->
            Bucket.builder()
                .addLimit(Bandwidth.classic(100, Refill.intervally(100, Duration.ofMinutes(1))))
                .build()
        );

        if (bucket.tryConsume(1)) {
            meterRegistry.counter("rate_limiter.requests", "status", "allowed").increment();
            return true;
        } else {
            meterRegistry.counter("rate_limiter.requests", "status", "denied").increment();
            return false;
        }
    }
}
```

### Health Checks

```java
@Component
public class ExternalApiHealthIndicator implements HealthIndicator {
    private final FipeApiClient fipeApiClient;
    private final S3Client s3Client;

    @Override
    public Health health() {
        try {
            // Testar FIPE API
            boolean fipeHealthy = fipeApiClient.testConnection();

            // Testar R2/S3
            boolean r2Healthy = testR2Connection();

            if (fipeHealthy && r2Healthy) {
                return Health.up()
                    .withDetail("fipe-api", "UP")
                    .withDetail("cloudflare-r2", "UP")
                    .build();
            } else {
                return Health.down()
                    .withDetail("fipe-api", fipeHealthy ? "UP" : "DOWN")
                    .withDetail("cloudflare-r2", r2Healthy ? "UP" : "DOWN")
                    .build();
            }
        } catch (Exception e) {
            return Health.down(e).build();
        }
    }
}
```

### Configurações de Application

```yaml
app:
  external-apis:
    fipe:
      base-url: ${FIPE_BASE_URL:https://parallelum.com.br/fipe/api/v1}
      timeout: 5s
      retry-attempts: 3
      retry-delay: 1s
      rate-limit-per-minute: 100
      circuit-breaker:
        failure-threshold: 50
        wait-duration: 30s
        sliding-window: 10

    cloudflare-r2:
      endpoint: ${CLOUDFLARE_R2_ENDPOINT}
      access-key: ${CLOUDFLARE_R2_ACCESS_KEY}
      secret-key: ${CLOUDFLARE_R2_SECRET_KEY}
      bucket-name: ${CLOUDFLARE_R2_BUCKET:vehicle-photos}
      timeout: 10s
      part-size: 5242880 # 5MB
      max-connections: 50
```

## Critérios de Sucesso

- [x] FIPE API com retry e circuit breaker
- [x] Cloudflare R2 integration funcionando
- [x] Rate limiting implementado
- [x] Métricas de disponibilidade coletadas
- [x] Health checks funcionando
- [x] Logs estruturados de integração
- [x] Fallback strategies implementadas
- [x] Configurações externalizadas
- [x] Performance < 500ms para chamadas cacheadas

## Sequenciamento

- Bloqueado por: 1.0, 4.0, 6.0
- Desbloqueia: 9.0, 10.0
- Paralelizável: Sim (com 9.0 e 10.0)

## Tempo Estimado

- WebClient setup: 4 horas
- FIPE integration: 6 horas
- R2/S3 setup: 6 horas
- Resilience patterns: 4 horas
- Testes: 4 horas
- Total: 24 horas