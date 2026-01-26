package com.gestauto.vehicleevaluation.api.observability;

import com.gestauto.vehicleevaluation.api.config.OpenTelemetryConfig;
import io.opentelemetry.api.OpenTelemetry;
import io.opentelemetry.api.trace.Span;
import io.opentelemetry.api.trace.SpanContext;
import io.opentelemetry.api.trace.Tracer;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.SpringBootConfiguration;
import org.springframework.boot.autoconfigure.EnableAutoConfiguration;
import org.springframework.boot.autoconfigure.data.jpa.JpaRepositoriesAutoConfiguration;
import org.springframework.boot.autoconfigure.flyway.FlywayAutoConfiguration;
import org.springframework.boot.autoconfigure.jdbc.DataSourceAutoConfiguration;
import org.springframework.boot.autoconfigure.orm.jpa.HibernateJpaAutoConfiguration;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.context.annotation.Import;

import static org.assertj.core.api.Assertions.assertThat;

@SpringBootTest(
    classes = OpenTelemetryConfigIntegrationTest.TestApp.class,
    properties = {
        "otel.traces.exporter=none",
        "otel.metrics.exporter=none",
        "otel.logs.exporter=none"
    }
)
class OpenTelemetryConfigIntegrationTest {

    @Autowired
    private OpenTelemetry openTelemetry;

    @Autowired
    private Tracer tracer;

    @Test
    void tracerShouldCreateValidSpanContext() {
        assertThat(openTelemetry).isNotNull();
        assertThat(tracer).isNotNull();

        Span span = tracer.spanBuilder("integration-test-span").startSpan();
        SpanContext spanContext = span.getSpanContext();

        assertThat(spanContext.isValid()).isTrue();
        assertThat(spanContext.getTraceId()).isNotBlank();
        assertThat(spanContext.getSpanId()).isNotBlank();

        span.end();
    }

    @SpringBootConfiguration
    @EnableAutoConfiguration(exclude = {
        DataSourceAutoConfiguration.class,
        HibernateJpaAutoConfiguration.class,
        JpaRepositoriesAutoConfiguration.class,
        FlywayAutoConfiguration.class
    })
    @Import(OpenTelemetryConfig.class)
    static class TestApp {
    }
}
