package com.gestauto.vehicleevaluation.infra.config;

import org.springframework.boot.autoconfigure.condition.ConditionalOnProperty;
import org.springframework.cache.CacheManager;
import org.springframework.cache.annotation.EnableCaching;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;
import org.springframework.data.redis.cache.RedisCacheConfiguration;
import org.springframework.data.redis.cache.RedisCacheManager;
import org.springframework.data.redis.connection.RedisConnectionFactory;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.context.annotation.Primary;

import java.time.Duration;

/**
 * Configuração de cache com Redis para o serviço de avaliação.
 *
 * Utiliza Spring Cache abstraction com Redis como backend,
 * configurando cache para FIPE com TTL de 24 horas.
 */
@Configuration
@EnableCaching
public class CacheConfig {

    @Value("${app.cache.dashboard.ttl-seconds:60}")
    private long dashboardTtlSeconds;

    /**
     * Configuração de cache manager com Redis.
     *
     * Define:
     * - TTL padrão de 24 horas para cache FIPE
     * - Serialização usando Jackson
     * - Suporte a null values para evitar thundering herd
     */
    @Bean
    @Primary
    public CacheManager redisCacheManager(RedisConnectionFactory connectionFactory) {
        RedisCacheConfiguration config = RedisCacheConfiguration.defaultCacheConfig()
            // TTL de 24 horas para cache FIPE
            .entryTtl(Duration.ofHours(24))
            // Não cachear null values
            .disableCachingNullValues();

        return RedisCacheManager.builder(connectionFactory)
            .cacheDefaults(config)
            .build();
    }

    @Bean(name = "dashboardCacheManager")
    public CacheManager dashboardCacheManager(RedisConnectionFactory connectionFactory) {
        RedisCacheConfiguration config = RedisCacheConfiguration.defaultCacheConfig()
            .entryTtl(Duration.ofSeconds(dashboardTtlSeconds))
            .disableCachingNullValues();

        return RedisCacheManager.builder(connectionFactory)
            .cacheDefaults(config)
            .build();
    }

    /**
     * Configuração alternativa de cache manager com configuração mais detalhada.
     * Pode ser usada quando custom serializers são necessários.
     */
    @Bean
    @ConditionalOnProperty(name = "app.cache.redis.detailed-config", havingValue = "true")
    public CacheManager detailedRedisCacheManager(RedisConnectionFactory connectionFactory) {
        RedisCacheConfiguration defaultConfig = RedisCacheConfiguration.defaultCacheConfig()
            .entryTtl(Duration.ofHours(24))
            .disableCachingNullValues();

        RedisCacheConfiguration fipePriceConfig = RedisCacheConfiguration.defaultCacheConfig()
            .entryTtl(Duration.ofHours(24))
            .disableCachingNullValues();

        return RedisCacheManager.builder(connectionFactory)
            .cacheDefaults(defaultConfig)
            .withCacheConfiguration("fipe-prices", fipePriceConfig)
            .withCacheConfiguration("fipe-vehicles", fipePriceConfig)
            .build();
    }
}
