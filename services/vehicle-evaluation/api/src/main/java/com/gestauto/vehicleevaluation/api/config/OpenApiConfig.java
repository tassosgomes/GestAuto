package com.gestauto.vehicleevaluation.api.config;

import io.swagger.v3.oas.models.Components;
import io.swagger.v3.oas.models.OpenAPI;
import io.swagger.v3.oas.models.info.Contact;
import io.swagger.v3.oas.models.info.Info;
import io.swagger.v3.oas.models.info.License;
import io.swagger.v3.oas.models.security.SecurityRequirement;
import io.swagger.v3.oas.models.security.SecurityScheme;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;

/**
 * Configuração do OpenAPI/Swagger para documentação da API.
 *
 * Esta configuração define os metadados da API e o esquema
 * de autenticação JWT para serem exibidos na interface Swagger.
 */
@Configuration
public class OpenApiConfig {

    /**
     * Configuração principal do OpenAPI.
     *
     * @return OpenAPI configurado com metadados e segurança
     */
    @Bean
    public OpenAPI customOpenAPI() {
        return new OpenAPI()
                .info(apiInfo())
                .components(components());
    }

    /**
     * Informações sobre a API.
     *
     * @return Info com metadados da API
     */
    private Info apiInfo() {
        return new Info()
                .title("GestAuto - Vehicle Evaluation API")
                .description("API para gestão de avaliações de veículos seminovos")
                .version("1.0.0")
                .contact(new Contact()
                        .name("GestAuto Team")
                        .email("support@gestauto.com")
                        .url("https://www.gestauto.com"))
                .license(new License()
                        .name("MIT License")
                        .url("https://opensource.org/licenses/MIT"));
    }

    /**
     * Componentes da OpenAPI incluindo esquema de segurança.
     *
     * @return Components configurado
     */
    private Components components() {
        return new Components()
                .addSecuritySchemes("bearerAuth",
                        new SecurityScheme()
                                .type(SecurityScheme.Type.HTTP)
                                .scheme("bearer")
                                .bearerFormat("JWT")
                                .description("Token JWT de autenticação"));
    }
}