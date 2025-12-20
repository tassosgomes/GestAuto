package com.gestauto.vehicleevaluation.api.support;

import org.springframework.test.context.DynamicPropertyRegistry;
import org.springframework.test.context.DynamicPropertySource;
import org.testcontainers.containers.PostgreSQLContainer;
import org.testcontainers.containers.RabbitMQContainer;
import org.testcontainers.utility.DockerImageName;
import org.junit.jupiter.api.extension.ExtendWith;

@ExtendWith(DockerAvailableCondition.class)
public abstract class IntegrationTestContainers {

    static final PostgreSQLContainer<?> postgres = new PostgreSQLContainer<>(DockerImageName.parse("postgres:15-alpine"))
        .withDatabaseName("gestauto")
        .withUsername("gestauto")
        .withPassword("gestauto123");

    static final RabbitMQContainer rabbitmq = new RabbitMQContainer(DockerImageName.parse("rabbitmq:3.11-alpine"))
        .withUser("gestauto", "gestauto123");

    private static final Object START_LOCK = new Object();
    private static volatile boolean started = false;

    @DynamicPropertySource
    static void registerProperties(DynamicPropertyRegistry registry) {
        startContainersIfNeeded();

        String datasourceUrl = withCurrentSchema(postgres.getJdbcUrl(), "vehicle_evaluation");
        registry.add("spring.datasource.url", () -> datasourceUrl);
        registry.add("spring.datasource.username", postgres::getUsername);
        registry.add("spring.datasource.password", postgres::getPassword);

        registry.add("spring.flyway.url", () -> datasourceUrl);
        registry.add("spring.flyway.user", postgres::getUsername);
        registry.add("spring.flyway.password", postgres::getPassword);
        registry.add("spring.flyway.schemas", () -> "vehicle_evaluation");
        registry.add("spring.flyway.default-schema", () -> "vehicle_evaluation");
        registry.add("spring.flyway.create-schemas", () -> "true");

        registry.add("spring.rabbitmq.host", rabbitmq::getHost);
        registry.add("spring.rabbitmq.port", rabbitmq::getAmqpPort);
        registry.add("spring.rabbitmq.username", rabbitmq::getAdminUsername);
        registry.add("spring.rabbitmq.password", rabbitmq::getAdminPassword);

        registry.add("spring.cache.type", () -> "none");

        registry.add("app.external-apis.cloudflare-r2.access-key", () -> "test");
        registry.add("app.external-apis.cloudflare-r2.secret-key", () -> "test");
        registry.add("app.external-apis.cloudflare-r2.endpoint", () -> "http://localhost:12345");
        registry.add("app.external-apis.cloudflare-r2.bucket-name", () -> "test-bucket");
    }

    private static void startContainersIfNeeded() {
        if (started) {
            return;
        }

        synchronized (START_LOCK) {
            if (started) {
                return;
            }

            postgres.start();
            rabbitmq.start();
            started = true;

            Runtime.getRuntime().addShutdownHook(new Thread(() -> {
                try {
                    rabbitmq.stop();
                } catch (Exception ignored) {
                    // ignored
                }
                try {
                    postgres.stop();
                } catch (Exception ignored) {
                    // ignored
                }
            }));
        }
    }

    private static String withCurrentSchema(String jdbcUrl, String schema) {
        if (jdbcUrl.contains("?")) {
            return jdbcUrl + "&currentSchema=" + schema;
        }
        return jdbcUrl + "?currentSchema=" + schema;
    }
}
