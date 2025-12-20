package com.gestauto.vehicleevaluation.infra.client.fipe;

import au.com.dius.pact.consumer.MockServer;
import au.com.dius.pact.consumer.dsl.PactBuilder;
import au.com.dius.pact.consumer.junit5.PactConsumerTest;
import au.com.dius.pact.consumer.junit5.PactTestFor;
import au.com.dius.pact.core.model.annotations.Pact;
import au.com.dius.pact.core.model.V4Pact;
import com.gestauto.vehicleevaluation.infra.client.fipe.dto.FipeBrandResponseDto;
import com.gestauto.vehicleevaluation.infra.client.fipe.dto.FipeModelResponseDto;
import com.gestauto.vehicleevaluation.infra.service.ratelimiter.RateLimiterService;
import io.github.resilience4j.ratelimiter.RateLimiterRegistry;
import io.micrometer.core.instrument.simple.SimpleMeterRegistry;
import org.junit.jupiter.api.Test;
import org.springframework.web.reactive.function.client.WebClient;

import java.util.List;
import java.util.Map;

import static org.assertj.core.api.Assertions.assertThat;

/**
 * Teste de contrato (Pact) do consumo da API FIPE.
 *
 * Observação: aqui tratamos a FIPE como "provider" externo.
 * O objetivo é capturar o contrato esperado (paths + payload mínimo) para o client.
 */
@PactTestFor(providerName = "FIPE_API")
@PactConsumerTest
class FipeApiClientPactTest {

    @Pact(provider = "FIPE_API", consumer = "GestAuto.VehicleEvaluation")
    public V4Pact shouldGetBrands(PactBuilder builder) {
        String body = "[{\"id\":\"59\",\"nome\":\"Volkswagen\"}]";

    builder
        .expectsToReceiveHttpInteraction("GET brands", interaction -> interaction
            .withRequest(req -> req
                .method("GET")
                .path("/fipe/api/v1/carros/marcas"))
            .willRespondWith(res -> res
                .status(200)
                .headers(Map.of("Content-Type", "application/json"))
                .body(body))
        );

    return builder.toPact();
    }

    @Test
    @PactTestFor(pactMethod = "shouldGetBrands")
    void getBrands_shouldMatchContract(MockServer mockServer) {
        FipeApiClient client = createClient(mockServer.getUrl());
        List<FipeBrandResponseDto> brands = client.getBrands();

        assertThat(brands).isNotNull().isNotEmpty();
        assertThat(brands.get(0).getId()).isNotBlank();
        assertThat(brands.get(0).getName()).isNotBlank();
    }

    @Pact(provider = "FIPE_API", consumer = "GestAuto.VehicleEvaluation")
        public V4Pact shouldGetModelsByBrand(PactBuilder builder) {
        String body = "[{\"id\":\"5940\",\"nome\":\"Gol 1.0\"}]";

        builder
            .expectsToReceiveHttpInteraction("GET models by brand", interaction -> interaction
                .withRequest(req -> req
                    .method("GET")
                    .path("/fipe/api/v1/carros/marcas/59/modelos"))
                .willRespondWith(res -> res
                    .status(200)
                    .headers(Map.of("Content-Type", "application/json"))
                    .body(body))
            );

        return builder.toPact();
    }

    @Test
    @PactTestFor(pactMethod = "shouldGetModelsByBrand")
    void getModels_shouldMatchContract(MockServer mockServer) {
        FipeApiClient client = createClient(mockServer.getUrl());
        List<FipeModelResponseDto> models = client.getModels("59");

        assertThat(models).isNotNull().isNotEmpty();
        assertThat(models.get(0).getId()).isNotBlank();
        assertThat(models.get(0).getName()).isNotBlank();
    }

    private static FipeApiClient createClient(String mockServerBaseUrl) {
        // Nosso client usa baseUrl + "/fipe/api/v1" e paths relativos.
        WebClient webClient = WebClient.builder()
                .baseUrl(mockServerBaseUrl + "/fipe/api/v1")
                .build();

        SimpleMeterRegistry meterRegistry = new SimpleMeterRegistry();
        RateLimiterService rateLimiterService = new RateLimiterService(meterRegistry, RateLimiterRegistry.ofDefaults());
        return new FipeApiClient(webClient, rateLimiterService, meterRegistry);
    }
}
