package com.gestauto.vehicleevaluation.infra.client.fipe;

import com.gestauto.vehicleevaluation.infra.client.fipe.dto.FipeBrandResponseDto;
import com.gestauto.vehicleevaluation.infra.client.fipe.dto.FipeModelResponseDto;
import com.gestauto.vehicleevaluation.infra.client.fipe.dto.FipeVehicleResponseDto;
import com.gestauto.vehicleevaluation.infra.service.ratelimiter.RateLimiterService;
import com.github.tomakehurst.wiremock.junit5.WireMockExtension;
import io.github.resilience4j.circuitbreaker.CircuitBreakerRegistry;
import io.micrometer.core.instrument.MeterRegistry;
import io.micrometer.core.instrument.simple.SimpleMeterRegistry;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;
import org.springframework.web.reactive.function.client.WebClient;

import static com.github.tomakehurst.wiremock.core.WireMockConfiguration.wireMockConfig;
import org.junit.jupiter.api.extension.RegisterExtension;

import java.math.BigDecimal;
import java.util.List;
import java.util.Optional;

import static org.assertj.core.api.Assertions.assertThat;

/**
 * Testes de integração para FipeApiClient usando WireMock.
 *
 * Objetivo: validar parsing + contrato básico de chamadas HTTP sem depender
 * de chamadas reais para a API FIPE durante o build.
 */
@DisplayName("FipeApiClient Integration Tests")
public class FipeApiClientIT {

    @RegisterExtension
    static WireMockExtension wireMock = WireMockExtension.newInstance()
        .options(wireMockConfig()
            .dynamicPort()
            .usingFilesUnderClasspath("wiremock"))
        .build();

    private FipeApiClient fipeApiClient;

    private MeterRegistry meterRegistry;

    @BeforeEach
    void setUp() {
    meterRegistry = new SimpleMeterRegistry();

    CircuitBreakerRegistry circuitBreakerRegistry = CircuitBreakerRegistry.ofDefaults();
    io.github.resilience4j.ratelimiter.RateLimiterRegistry rateLimiterRegistry =
        io.github.resilience4j.ratelimiter.RateLimiterRegistry.ofDefaults();

    WebClient webClient = WebClient.builder()
        .baseUrl(wireMock.getRuntimeInfo().getHttpBaseUrl() + "/fipe/api/v1")
        .build();

    RateLimiterService rateLimiterService = new RateLimiterService(meterRegistry, rateLimiterRegistry);
    fipeApiClient = new FipeApiClient(webClient, rateLimiterService, meterRegistry);
    }

    @Test
    @DisplayName("Should fetch brands from FIPE API with caching")
    void testGetBrands() {
    List<FipeBrandResponseDto> brands = fipeApiClient.getBrands();
    assertThat(brands)
        .isNotNull()
        .hasSize(2);
    assertThat(brands.get(0).getId()).isEqualTo("59");
    assertThat(brands.get(0).getName()).isEqualTo("Volkswagen");
    }

    @Test
    @DisplayName("Should fetch models for a brand")
    void testGetModels() {
    List<FipeModelResponseDto> models = fipeApiClient.getModels("59");
    assertThat(models)
        .isNotNull()
        .hasSize(2);
    assertThat(models.get(0).getId()).isEqualTo("5940");
    assertThat(models.get(0).getName()).contains("Gol");
    }

    @Test
    @DisplayName("Should fetch vehicle info")
    void testGetVehicleInfo() {
    Optional<FipeVehicleResponseDto> vehicle = fipeApiClient.getVehicleInfo("59", "5940", "2023-1");
    assertThat(vehicle).isPresent();
    assertThat(vehicle.get().getBrand()).isEqualTo("Volkswagen");
    assertThat(vehicle.get().getModel()).contains("Gol");
    assertThat(vehicle.get().getValue()).contains("R$");
    }

    @Test
    @DisplayName("Should parse price correctly")
    void testParsePrice() {
        BigDecimal price1 = fipeApiClient.parsePrice("R$ 25.000,00");
        assertThat(price1).isEqualTo(new BigDecimal("25000.00"));

        BigDecimal price2 = fipeApiClient.parsePrice("R$ 1.250.500,50");
        assertThat(price2).isEqualTo(new BigDecimal("1250500.50"));

        BigDecimal price3 = fipeApiClient.parsePrice("");
        assertThat(price3).isEqualTo(BigDecimal.ZERO);
    }

    @Test
    @DisplayName("Should return fallback for brands when circuit breaker is open")
    void testGetBrandsFallback() {
        List<FipeBrandResponseDto> brands = fipeApiClient.getBrandsFallback(
                new RuntimeException("Test exception"));
        assertThat(brands).isNotNull().isEmpty();
    }

    @Test
    @DisplayName("Should return fallback for models when circuit breaker is open")
    void testGetModelsFallback() {
        List<FipeModelResponseDto> models = fipeApiClient.getModelsFallback("1",
                new RuntimeException("Test exception"));
        assertThat(models).isNotNull().isEmpty();
    }

    @Test
    @DisplayName("Should return empty Optional for vehicle info when circuit breaker is open")
    void testGetVehicleInfoFallback() {
        Optional<FipeVehicleResponseDto> vehicle = fipeApiClient.getVehicleInfoFallback("1", "6", "2023",
                new RuntimeException("Test exception"));
        assertThat(vehicle).isNotNull().isEmpty();
    }
}
