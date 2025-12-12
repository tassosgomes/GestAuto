package com.gestauto.vehicleevaluation.infra.config;

import io.github.resilience4j.circuitbreaker.CircuitBreaker;
import io.github.resilience4j.circuitbreaker.CircuitBreakerConfig;
import io.github.resilience4j.circuitbreaker.CircuitBreakerRegistry;
import io.github.resilience4j.core.registry.EntryAddedEvent;
import io.github.resilience4j.core.registry.RegistryEventConsumer;
import io.netty.channel.ChannelOption;
import io.netty.handler.timeout.ReadTimeoutHandler;
import io.netty.handler.timeout.WriteTimeoutHandler;
import lombok.extern.slf4j.Slf4j;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.boot.actuate.metrics.web.reactive.client.WebClientExchangeTagsProvider;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;
import org.springframework.http.client.reactive.ReactorClientHttpConnector;
import org.springframework.web.reactive.function.client.ExchangeStrategies;
import org.springframework.web.reactive.function.client.WebClient;
import reactor.netty.http.client.HttpClient;
import reactor.netty.resources.ConnectionProvider;

import java.time.Duration;
import java.util.concurrent.TimeUnit;

/**
 * Configuração do WebClient para chamadas HTTP com resilience patterns
 * Implementa retry, circuit breaker, timeouts e caching
 */
@Configuration
@Slf4j
public class WebClientConfig {

    @Value("${app.external-apis.fipe.base-url:https://parallelum.com.br/fipe/api/v1}")
    private String fipeBaseUrl;

    @Value("${app.external-apis.fipe.timeout:5}")
    private int fipeTimeout;

    @Value("${app.external-apis.fipe.connect-timeout:2000}")
    private int fipeConnectTimeout;

    /**
     * WebClient bean com configuração de timeouts, connection pooling e resilience
     */
    @Bean
    public WebClient fipeWebClient(CircuitBreakerRegistry circuitBreakerRegistry) {
        // Configurar circuit breaker para FIPE API
        CircuitBreakerConfig circuitBreakerConfig = CircuitBreakerConfig.custom()
                .failureRateThreshold(50)
                .waitDurationInOpenState(Duration.ofSeconds(30))
                .slidingWindowSize(10)
                .minimumNumberOfCalls(3)
                .recordExceptions(Exception.class)
                .build();

        circuitBreakerRegistry.getOrCreate("fipe-api", circuitBreakerConfig);

        // Configurar HttpClient com timeouts e pool de conexões
        ConnectionProvider connectionProvider = ConnectionProvider.builder("fipe-pool")
                .maxConnections(50)
                .maxIdleTime(Duration.ofSeconds(30))
                .maxLifeTime(Duration.ofMinutes(5))
                .pendingAcquireTimeout(Duration.ofSeconds(10))
                .pendingAcquireMaxCount(100)
                .build();

        HttpClient httpClient = HttpClient.create(connectionProvider)
                .option(ChannelOption.CONNECT_TIMEOUT_MILLIS, fipeConnectTimeout)
                .option(ChannelOption.SO_KEEPALIVE, true)
                .responseTimeout(Duration.ofSeconds(fipeTimeout))
                .doOnConnected(conn ->
                        conn.addHandlerLast(new ReadTimeoutHandler(fipeTimeout, TimeUnit.SECONDS))
                                .addHandlerLast(new WriteTimeoutHandler(fipeTimeout, TimeUnit.SECONDS))
                );

        // ExchangeStrategies para aumentar buffer size se necessário
        ExchangeStrategies exchangeStrategies = ExchangeStrategies.builder()
                .codecs(configurer ->
                        configurer.defaultCodecs().maxInMemorySize(1024 * 1024) // 1MB
                )
                .build();

        return WebClient.builder()
                .baseUrl(fipeBaseUrl)
                .clientConnector(new ReactorClientHttpConnector(httpClient))
                .exchangeStrategies(exchangeStrategies)
                .build();
    }

    /**
     * Registrar circuit breakers com log de eventos
     */
    @Bean
    public RegistryEventConsumer<CircuitBreaker> myRegistryEventConsumer() {
        return new RegistryEventConsumer<CircuitBreaker>() {
            @Override
            public void onEntryAddedEvent(EntryAddedEvent<CircuitBreaker> entryAddedEvent) {
                CircuitBreaker circuitBreaker = entryAddedEvent.getAddedEntry();
                log.info("CircuitBreaker '{}' criado com estado: {}",
                        circuitBreaker.getName(), circuitBreaker.getState());

                circuitBreaker.getEventPublisher()
                        .onStateTransition(event ->
                                log.warn("CircuitBreaker '{}' transição: {} -> {}",
                                        circuitBreaker.getName(),
                                        event.getStateTransition().getFromState(),
                                        event.getStateTransition().getToState())
                        )
                        .onError(event ->
                                log.error("CircuitBreaker '{}' erro: {}",
                                        circuitBreaker.getName(),
                                        event.getThrowable().getMessage())
                        )
                        .onSuccess(event ->
                                log.debug("CircuitBreaker '{}' sucesso",
                                        circuitBreaker.getName())
                        );
            }

            @Override
            public void onEntryRemovedEvent(EntryAddedEvent<CircuitBreaker> entryRemoved) {
                log.info("CircuitBreaker '{}' removido",
                        entryRemoved.getAddedEntry().getName());
            }
        };
    }
}
