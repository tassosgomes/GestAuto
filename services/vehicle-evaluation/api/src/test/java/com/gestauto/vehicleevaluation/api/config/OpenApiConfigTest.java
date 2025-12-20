package com.gestauto.vehicleevaluation.api.config;

import static org.assertj.core.api.Assertions.assertThat;

import org.junit.jupiter.api.Test;

class OpenApiConfigTest {

    @Test
    void customOpenAPICreatesInfoAndBearerAuthScheme() {
        OpenApiConfig config = new OpenApiConfig();

        var api = config.customOpenAPI();

        assertThat(api).isNotNull();
        assertThat(api.getInfo()).isNotNull();
        assertThat(api.getInfo().getTitle()).isEqualTo("GestAuto - Vehicle Evaluation API");

        assertThat(api.getComponents()).isNotNull();
        assertThat(api.getComponents().getSecuritySchemes()).containsKey("bearerAuth");
        assertThat(api.getComponents().getSecuritySchemes().get("bearerAuth").getScheme()).isEqualTo("bearer");
        assertThat(api.getComponents().getSecuritySchemes().get("bearerAuth").getBearerFormat()).isEqualTo("JWT");
    }
}
