package com.gestauto.vehicleevaluation.api.config;

import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.get;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.status;

import java.util.List;
import java.util.Map;

import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.SpringBootConfiguration;
import org.springframework.boot.autoconfigure.EnableAutoConfiguration;
import org.springframework.boot.autoconfigure.amqp.RabbitAutoConfiguration;
import org.springframework.boot.autoconfigure.data.jpa.JpaRepositoriesAutoConfiguration;
import org.springframework.boot.autoconfigure.flyway.FlywayAutoConfiguration;
import org.springframework.boot.autoconfigure.jdbc.DataSourceAutoConfiguration;
import org.springframework.boot.autoconfigure.orm.jpa.HibernateJpaAutoConfiguration;
import org.springframework.boot.test.autoconfigure.web.servlet.AutoConfigureMockMvc;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Import;
import org.springframework.http.MediaType;
import org.springframework.security.oauth2.jwt.Jwt;
import org.springframework.security.oauth2.jwt.JwtDecoder;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RestController;
import org.springframework.test.web.servlet.MockMvc;

@SpringBootTest(
    classes = SecurityConfigTest.TestApp.class,
    webEnvironment = SpringBootTest.WebEnvironment.MOCK
)
@AutoConfigureMockMvc
class SecurityConfigTest {

    @Autowired
    private MockMvc mockMvc;

    @Test
    void actuatorHealthIsPublic() throws Exception {
        mockMvc.perform(get("/actuator/health").accept(MediaType.APPLICATION_JSON))
            .andExpect(status().isOk());
    }

    @Test
    void publicValidationIsPublic() throws Exception {
        mockMvc.perform(get("/api/v1/evaluations/public/hello").accept(MediaType.TEXT_PLAIN))
            .andExpect(status().isOk());
    }

    @Test
    void securedEndpointWithoutTokenReturns401() throws Exception {
        mockMvc.perform(get("/api/v1/evaluations/secure"))
            .andExpect(status().isUnauthorized());
    }

    @Test
    void securedEndpointWithAdminRoleReturns200() throws Exception {
        mockMvc.perform(
                get("/api/v1/evaluations/secure")
                    .header("Authorization", "Bearer dummy")
            )
            .andExpect(status().isOk());
    }

    @SpringBootConfiguration
    @EnableAutoConfiguration(exclude = {
        RabbitAutoConfiguration.class,
        DataSourceAutoConfiguration.class,
        HibernateJpaAutoConfiguration.class,
        JpaRepositoriesAutoConfiguration.class,
        FlywayAutoConfiguration.class
    })
    @Import(SecurityConfig.class)
    static class TestApp {

        @Bean
        JwtDecoder jwtDecoder() {
            return token -> Jwt.withTokenValue(token)
                .header("alg", "none")
                .claim("sub", "test-user")
                // SecurityConfig reads roles from claim "roles" and prefixes them with ROLE_
                .claim("roles", List.of("ADMIN"))
                .build();
        }

        @RestController
        static class TestController {

            @GetMapping("/actuator/health")
            Map<String, String> health() {
                return Map.of("status", "UP");
            }

            @GetMapping("/api/v1/evaluations/public/hello")
            String publicHello() {
                return "ok";
            }

            @GetMapping("/api/v1/evaluations/secure")
            String secure() {
                return "ok";
            }
        }
    }
}
