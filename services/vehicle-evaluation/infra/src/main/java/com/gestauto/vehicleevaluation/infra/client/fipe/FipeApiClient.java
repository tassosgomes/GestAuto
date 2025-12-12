package com.gestauto.vehicleevaluation.infra.client.fipe;

import com.gestauto.vehicleevaluation.infra.client.fipe.dto.FipeBrandResponseDto;
import com.gestauto.vehicleevaluation.infra.client.fipe.dto.FipeModelResponseDto;
import com.gestauto.vehicleevaluation.infra.client.fipe.dto.FipeVehicleResponseDto;
import com.gestauto.vehicleevaluation.infra.service.ratelimiter.RateLimiterService;
import io.github.resilience4j.circuitbreaker.annotation.CircuitBreaker;
import io.github.resilience4j.retry.annotation.Retry;
import lombok.extern.slf4j.Slf4j;
import org.springframework.boot.actuate.metrics.MeterRegistry;
import org.springframework.cache.annotation.Cacheable;
import org.springframework.stereotype.Service;
import org.springframework.web.reactive.function.client.WebClient;
import org.springframework.web.reactive.function.client.WebClientResponseException;
import reactor.core.publisher.Mono;

import java.math.BigDecimal;
import java.time.Duration;
import java.util.ArrayList;
import java.util.List;
import java.util.Objects;
import java.util.Optional;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

/**
 * Cliente para consumo da API FIPE com resiliência
 * Implementa retry automático, circuit breaker, caching e rate limiting
 */
@Service
@Slf4j
public class FipeApiClient {

    private final WebClient fipeWebClient;
    private final RateLimiterService rateLimiterService;
    private final MeterRegistry meterRegistry;

    private static final String FIPE_CLIENT_ID = "fipe-api";
    private static final Pattern PRICE_PATTERN = Pattern.compile("R\\$\\s*([\\d.,]+)");

    public FipeApiClient(WebClient fipeWebClient,
                         RateLimiterService rateLimiterService,
                         MeterRegistry meterRegistry) {
        this.fipeWebClient = fipeWebClient;
        this.rateLimiterService = rateLimiterService;
        this.meterRegistry = meterRegistry;
    }

    /**
     * Obter lista de marcas de veículos da API FIPE
     * Com caching de 1 hora (marcas mudam raramente)
     */
    @Cacheable(cacheNames = "fipe-brands", unless = "#result == null || #result.isEmpty()")
    @CircuitBreaker(name = "fipe-api", fallbackMethod = "getBrandsFallback")
    @Retry(name = "fipe-api")
    public List<FipeBrandResponseDto> getBrands() {
        log.info("Fetching brands list from FIPE API");

        if (!rateLimiterService.allowFipeRequest(FIPE_CLIENT_ID)) {
            throw new FipeApiException("Rate limit exceeded for FIPE API", "RATE_LIMIT_EXCEEDED");
        }

        try {
            List<FipeBrandResponseDto> brands = fipeWebClient.get()
                    .uri("/carros/marcas")
                    .retrieve()
                    .bodyToFlux(FipeBrandResponseDto.class)
                    .collectList()
                    .timeout(Duration.ofSeconds(5))
                    .block();

            meterRegistry.counter("fipe.api.calls", "endpoint", "/carros/marcas", "status", "success").increment();
            log.info("Successfully fetched {} brands from FIPE API", Objects.requireNonNull(brands).size());

            return brands;
        } catch (WebClientResponseException e) {
            meterRegistry.counter("fipe.api.calls", "endpoint", "/carros/marcas", "status", "error").increment();
            log.error("Error fetching brands from FIPE API: {}", e.getStatusCode(), e);
            throw new FipeApiException("Failed to fetch brands from FIPE API", "API_ERROR", e);
        } catch (Exception e) {
            meterRegistry.counter("fipe.api.calls", "endpoint", "/carros/marcas", "status", "error").increment();
            log.error("Unexpected error fetching brands from FIPE API", e);
            throw new FipeApiException("Unexpected error fetching brands from FIPE API", "UNEXPECTED_ERROR", e);
        }
    }

    /**
     * Obter lista de modelos para uma marca específica
     */
    @Cacheable(cacheNames = "fipe-models", key = "#brandId", unless = "#result == null || #result.isEmpty()")
    @CircuitBreaker(name = "fipe-api", fallbackMethod = "getModelsFallback")
    @Retry(name = "fipe-api")
    public List<FipeModelResponseDto> getModels(String brandId) {
        log.info("Fetching models for brand {} from FIPE API", brandId);

        if (!rateLimiterService.allowFipeRequest(FIPE_CLIENT_ID)) {
            throw new FipeApiException("Rate limit exceeded for FIPE API", "RATE_LIMIT_EXCEEDED");
        }

        try {
            List<FipeModelResponseDto> models = fipeWebClient.get()
                    .uri("/carros/marcas/{brandId}/modelos", brandId)
                    .retrieve()
                    .bodyToFlux(FipeModelResponseDto.class)
                    .collectList()
                    .timeout(Duration.ofSeconds(5))
                    .block();

            meterRegistry.counter("fipe.api.calls", "endpoint", "/modelos", "status", "success").increment();
            log.info("Successfully fetched {} models for brand {}", Objects.requireNonNull(models).size(), brandId);

            return models;
        } catch (WebClientResponseException e) {
            meterRegistry.counter("fipe.api.calls", "endpoint", "/modelos", "status", "error").increment();
            log.error("Error fetching models for brand {} from FIPE API: {}", brandId, e.getStatusCode(), e);
            throw new FipeApiException("Failed to fetch models from FIPE API", "API_ERROR", e);
        } catch (Exception e) {
            meterRegistry.counter("fipe.api.calls", "endpoint", "/modelos", "status", "error").increment();
            log.error("Unexpected error fetching models for brand {}", brandId, e);
            throw new FipeApiException("Unexpected error fetching models from FIPE API", "UNEXPECTED_ERROR", e);
        }
    }

    /**
     * Obter informações de preço de um veículo específico
     * Formato: /carros/marcas/{brandId}/modelos/{modelId}/anos/{year}
     */
    @Cacheable(cacheNames = "fipe-vehicles", key = "#brandId + '-' + #modelId + '-' + #year",
            unless = "#result == null")
    @CircuitBreaker(name = "fipe-api", fallbackMethod = "getVehicleInfoFallback")
    @Retry(name = "fipe-api")
    public Optional<FipeVehicleResponseDto> getVehicleInfo(String brandId, String modelId, String year) {
        log.info("Fetching vehicle info from FIPE API: brand={}, model={}, year={}", brandId, modelId, year);

        if (!rateLimiterService.allowFipeRequest(FIPE_CLIENT_ID)) {
            throw new FipeApiException("Rate limit exceeded for FIPE API", "RATE_LIMIT_EXCEEDED");
        }

        try {
            FipeVehicleResponseDto vehicleInfo = fipeWebClient.get()
                    .uri("/carros/marcas/{brandId}/modelos/{modelId}/anos/{year}",
                            brandId, modelId, year)
                    .retrieve()
                    .bodyToMono(FipeVehicleResponseDto.class)
                    .timeout(Duration.ofSeconds(5))
                    .block();

            meterRegistry.counter("fipe.api.calls", "endpoint", "/anos", "status", "success").increment();

            if (vehicleInfo != null) {
                log.info("Successfully fetched vehicle info: {} {} ({})",
                        vehicleInfo.getBrand(), vehicleInfo.getModel(), vehicleInfo.getModelYear());
            }

            return Optional.ofNullable(vehicleInfo);
        } catch (WebClientResponseException e) {
            if (e.getStatusCode().is4xxClientError()) {
                log.warn("Vehicle not found in FIPE API: brand={}, model={}, year={}",
                        brandId, modelId, year);
                meterRegistry.counter("fipe.api.calls", "endpoint", "/anos", "status", "not_found").increment();
                return Optional.empty();
            } else {
                meterRegistry.counter("fipe.api.calls", "endpoint", "/anos", "status", "error").increment();
                log.error("Error fetching vehicle info from FIPE API: {}", e.getStatusCode(), e);
                throw new FipeApiException("Failed to fetch vehicle info from FIPE API", "API_ERROR", e);
            }
        } catch (Exception e) {
            meterRegistry.counter("fipe.api.calls", "endpoint", "/anos", "status", "error").increment();
            log.error("Unexpected error fetching vehicle info", e);
            throw new FipeApiException("Unexpected error fetching vehicle info from FIPE API", "UNEXPECTED_ERROR", e);
        }
    }

    /**
     * Extrair valor monetário da string da resposta da FIPE
     * Exemplo: "R$ 25.000,00" -> BigDecimal(25000.00)
     */
    public BigDecimal parsePrice(String priceString) {
        if (priceString == null || priceString.isEmpty()) {
            return BigDecimal.ZERO;
        }

        try {
            Matcher matcher = PRICE_PATTERN.matcher(priceString);
            if (matcher.find()) {
                String numberStr = matcher.group(1)
                        .replace(".", "") // Remove separador de milhares
                        .replace(",", "."); // Converte vírgula em ponto
                return new BigDecimal(numberStr);
            }
        } catch (Exception e) {
            log.warn("Failed to parse price: {}", priceString, e);
        }

        return BigDecimal.ZERO;
    }

    /**
     * Fallback para getBrands quando circuit breaker está aberto ou falha
     */
    public List<FipeBrandResponseDto> getBrandsFallback(Exception ex) {
        log.warn("Fallback for getBrands: using empty list. Reason: {}", ex.getMessage());
        meterRegistry.counter("fipe.api.fallback", "method", "getBrands").increment();
        return new ArrayList<>();
    }

    /**
     * Fallback para getModels quando circuit breaker está aberto ou falha
     */
    public List<FipeModelResponseDto> getModelsFallback(String brandId, Exception ex) {
        log.warn("Fallback for getModels (brand={}): using empty list. Reason: {}",
                brandId, ex.getMessage());
        meterRegistry.counter("fipe.api.fallback", "method", "getModels").increment();
        return new ArrayList<>();
    }

    /**
     * Fallback para getVehicleInfo quando circuit breaker está aberto ou falha
     */
    public Optional<FipeVehicleResponseDto> getVehicleInfoFallback(String brandId, String modelId, String year, Exception ex) {
        log.warn("Fallback for getVehicleInfo (brand={}, model={}, year={}): returning empty. Reason: {}",
                brandId, modelId, year, ex.getMessage());
        meterRegistry.counter("fipe.api.fallback", "method", "getVehicleInfo").increment();
        return Optional.empty();
    }
}
