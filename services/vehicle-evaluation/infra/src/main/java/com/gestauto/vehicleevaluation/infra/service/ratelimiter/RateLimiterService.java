package com.gestauto.vehicleevaluation.infra.service.ratelimiter;

import io.github.resilience4j.ratelimiter.RateLimiter;
import io.github.resilience4j.ratelimiter.RateLimiterConfig;
import io.github.resilience4j.ratelimiter.RateLimiterRegistry;
import io.micrometer.core.instrument.MeterRegistry;
import lombok.extern.slf4j.Slf4j;
import org.springframework.stereotype.Component;

import java.time.Duration;
import java.util.concurrent.atomic.AtomicInteger;

/**
 * Serviço de rate limiting para APIs externas usando Resilience4j RateLimiter
 * Implementa token bucket algorithm para controle de requisições
 */
@Component
@Slf4j
public class RateLimiterService {

    private final MeterRegistry meterRegistry;
    private final RateLimiterRegistry rateLimiterRegistry;
    private final java.util.concurrent.ConcurrentHashMap<String, AtomicInteger> requestCounts;

    public RateLimiterService(MeterRegistry meterRegistry, RateLimiterRegistry rateLimiterRegistry) {
        this.meterRegistry = meterRegistry;
        this.rateLimiterRegistry = rateLimiterRegistry;
        this.requestCounts = new java.util.concurrent.ConcurrentHashMap<>();
    }

    /**
     * Verificar se a requisição é permitida pelo rate limiter
     *
     * @param clientId ID único do cliente/aplicação
     * @param maxRequests Número máximo de requisições
     * @param duration Duração da janela de tempo
     * @return true se a requisição é permitida, false caso contrário
     */
    public boolean allowRequest(String clientId, int maxRequests, Duration duration) {
        RateLimiter rateLimiter = rateLimiterRegistry.rateLimiter(clientId,
                RateLimiterConfig.custom()
                        .limitRefreshPeriod(duration)
                        .limitForPeriod(maxRequests)
                        .timeoutDuration(Duration.ofSeconds(1))
                        .build()
        );

        if (rateLimiter.acquirePermission()) {
            meterRegistry.counter("rate_limiter.requests",
                    "client", clientId,
                    "status", "allowed").increment();

            AtomicInteger count = requestCounts.computeIfAbsent(clientId, k -> new AtomicInteger(0));
            count.incrementAndGet();

            log.debug("Rate limiter: requisição permitida para cliente '{}' (total: {})",
                    clientId, count.get());
            return true;
        } else {
            meterRegistry.counter("rate_limiter.requests",
                    "client", clientId,
                    "status", "denied").increment();

            log.warn("Rate limiter: requisição bloqueada para cliente '{}'", clientId);
            return false;
        }
    }

    /**
     * Verificar se a requisição é permitida usando padrão padrão da FIPE (100 req/min)
     */
    public boolean allowFipeRequest(String clientId) {
        return allowRequest(clientId, 100, Duration.ofMinutes(1));
    }

    /**
     * Resetar o rate limiter para um cliente
     */
    public void resetClient(String clientId) {
        rateLimiterRegistry.remove(clientId);
        requestCounts.remove(clientId);
        log.info("Rate limiter: cliente '{}' resetado", clientId);
    }

    /**
     * Obter número de requisições permitidas restantes para um cliente
     */
    public int getRemainingRequests(String clientId, int maxRequests, Duration duration) {
        RateLimiter rateLimiter = rateLimiterRegistry.rateLimiter(clientId,
                RateLimiterConfig.custom()
                        .limitRefreshPeriod(duration)
                        .limitForPeriod(maxRequests)
                        .timeoutDuration(Duration.ofSeconds(1))
                        .build()
        );

        return rateLimiter.getMetrics().getAvailablePermissions();
    }

    /**
     * Obter número total de requisições realizadas por um cliente
     */
    public int getTotalRequests(String clientId) {
        AtomicInteger count = requestCounts.get(clientId);
        return count != null ? count.get() : 0;
    }
}
