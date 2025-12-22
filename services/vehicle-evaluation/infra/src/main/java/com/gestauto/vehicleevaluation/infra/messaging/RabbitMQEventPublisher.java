package com.gestauto.vehicleevaluation.infra.messaging;

import com.gestauto.vehicleevaluation.domain.event.*;
import com.gestauto.vehicleevaluation.domain.event.DomainEventExternalPublisher;
import io.micrometer.core.instrument.Counter;
import io.micrometer.core.instrument.MeterRegistry;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.amqp.AmqpException;
import org.springframework.amqp.core.Message;
import org.springframework.amqp.core.MessagePostProcessor;
import org.springframework.amqp.rabbit.core.RabbitTemplate;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.scheduling.annotation.Async;
import org.springframework.stereotype.Component;

import java.sql.Timestamp;
import java.time.ZoneId;
import java.util.HashMap;
import java.util.Map;
import java.util.UUID;

/**
 * Publisher de eventos de domínio para RabbitMQ.
 *
 * Responsável por publicar eventos de domínio no RabbitMQ com:
 * - Retry automático com exponential backoff
 * - Logging estruturado de eventos
 * - Métricas de publicação
 * - Headers customizados para rastreabilidade
 */
@Component
public class RabbitMQEventPublisher implements DomainEventExternalPublisher {

    private static final Logger log = LoggerFactory.getLogger(RabbitMQEventPublisher.class);

    private final RabbitTemplate rabbitTemplate;
    private final MeterRegistry meterRegistry;
    private final Map<String, Counter> eventCounters;

    @Value("${rabbitmq.exchange.name:gestauto.events}")
    private String exchangeName;

    public RabbitMQEventPublisher(RabbitTemplate rabbitTemplate, MeterRegistry meterRegistry) {
        this.rabbitTemplate = rabbitTemplate;
        this.meterRegistry = meterRegistry;
        this.eventCounters = new HashMap<>();
    }

    /**
     * Publica um evento de domínio de forma assíncrona.
     *
     * @param event evento a ser publicado
     */
    @Async
    @Override
    public void publishEvent(DomainEvent event) {
        if (event == null) {
            log.warn("Attempted to publish null event");
            return;
        }

        String routingKey = determineRoutingKey(event);
        String eventId = UUID.randomUUID().toString();

        try {
            log.info("Publishing event: type={}, evaluationId={}, eventId={}", 
                    event.getEventType(), event.getEvaluationId(), eventId);

            rabbitTemplate.convertAndSend(
                exchangeName,
                routingKey,
                event,
                createMessagePostProcessor(event, eventId)
            );

            // Registrar métrica de sucesso
            incrementCounter("events.published", event.getEventType(), "success");

            log.info("Event published successfully: type={}, evaluationId={}, eventId={}, routingKey={}", 
                    event.getEventType(), event.getEvaluationId(), eventId, routingKey);

        } catch (AmqpException e) {
            // Registrar métrica de falha
            incrementCounter("events.published", event.getEventType(), "failed");

            log.error("Failed to publish event: type={}, evaluationId={}, eventId={}, error={}", 
                    event.getEventType(), event.getEvaluationId(), eventId, e.getMessage(), e);

            throw new EventPublishingException(
                String.format("Failed to publish event: %s - %s", event.getEventType(), eventId), 
                e
            );
        }
    }

    /**
     * Determina a routing key baseada no tipo do evento.
     */
    private String determineRoutingKey(DomainEvent event) {
        return switch (event.getEventType()) {
            case "EvaluationCreated" -> "vehicle.evaluation.created";
            case "EvaluationSubmitted" -> "vehicle.evaluation.submitted";
            case "EvaluationApproved" -> "vehicle.evaluation.approved";
            case "EvaluationRejected" -> "vehicle.evaluation.rejected";
            case "VehicleEvaluationCompleted" -> "vehicle.evaluation.completed";
            case "ChecklistCompleted" -> "vehicle.evaluation.checklist.completed";
            case "ValuationCalculated" -> "vehicle.evaluation.valuation.calculated";
            default -> "vehicle.evaluation." + event.getEventType().toLowerCase();
        };
    }

    /**
     * Cria um MessagePostProcessor para adicionar headers customizados.
     */
    private MessagePostProcessor createMessagePostProcessor(DomainEvent event, String eventId) {
        return new MessagePostProcessor() {
            @Override
            public Message postProcessMessage(Message message) throws AmqpException {
                message.getMessageProperties().setContentType("application/json");
                message.getMessageProperties().setTimestamp(
                    Timestamp.from(event.getOccurredAt().atZone(ZoneId.systemDefault()).toInstant())
                );
                message.getMessageProperties().setHeader("eventId", eventId);
                message.getMessageProperties().setHeader("eventType", event.getEventType());
                message.getMessageProperties().setHeader("evaluationId", event.getEvaluationId());
                message.getMessageProperties().setHeader("publishedAt", System.currentTimeMillis());
                message.getMessageProperties().setHeader("source", "vehicle-evaluation-service");
                message.getMessageProperties().setMessageId(eventId);
                
                // Header para idempotência
                message.getMessageProperties().setHeader("idempotencyKey", 
                    event.getEvaluationId() + ":" + event.getEventType() + ":" + event.getOccurredAt().toString());
                
                return message;
            }
        };
    }

    /**
     * Incrementa contador de métricas de eventos.
     */
    private void incrementCounter(String counterName, String eventType, String status) {
        String key = counterName + "." + eventType + "." + status;
        
        Counter counter = eventCounters.computeIfAbsent(key, k -> 
            Counter.builder(counterName)
                .tag("type", eventType)
                .tag("status", status)
                .description("Count of published domain events")
                .register(meterRegistry)
        );
        
        counter.increment();
    }

    /**
     * Exception específica para falhas na publicação de eventos.
     */
    public static class EventPublishingException extends RuntimeException {
        public EventPublishingException(String message, Throwable cause) {
            super(message, cause);
        }
    }
}
