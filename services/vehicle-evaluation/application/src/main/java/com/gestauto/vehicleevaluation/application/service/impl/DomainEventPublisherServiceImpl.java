package com.gestauto.vehicleevaluation.application.service.impl;

import com.gestauto.vehicleevaluation.application.service.DomainEventPublisherService;
import com.gestauto.vehicleevaluation.domain.event.DomainEvent;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.context.ApplicationEventPublisher;
import org.springframework.stereotype.Service;

import java.util.ArrayList;
import java.util.List;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.atomic.AtomicLong;

/**
 * Implementação do serviço de publicação de eventos de domínio.
 *
 * Esta implementação publica eventos de duas formas:
 * 1. Spring ApplicationEventPublisher (para listeners locais síncronos)
 * 2. RabbitMQ (para integração assíncrona entre bounded contexts)
 *
 * Também mantém um registro em memória para testes e auditoria.
 */
@Service
public class DomainEventPublisherServiceImpl implements DomainEventPublisherService {

    private static final Logger log = LoggerFactory.getLogger(DomainEventPublisherServiceImpl.class);

    private final ApplicationEventPublisher springEventPublisher;
    private Object rabbitMQEventPublisher; // Injeção opcional para evitar dependência circular

    private final AtomicLong eventCounter = new AtomicLong(0);
    private final ConcurrentHashMap<Long, DomainEvent> publishedEvents = new ConcurrentHashMap<>();
    private boolean available = true;

    public DomainEventPublisherServiceImpl(ApplicationEventPublisher springEventPublisher) {
        this.springEventPublisher = springEventPublisher;
    }

    /**
     * Setter para injeção do RabbitMQEventPublisher (opcional).
     * Usado para evitar dependência circular com o módulo de infraestrutura.
     */
    @Autowired(required = false)
    public void setRabbitMQEventPublisher(Object rabbitMQEventPublisher) {
        this.rabbitMQEventPublisher = rabbitMQEventPublisher;
        log.info("RabbitMQ event publisher configured for domain events");
    }

    @Override
    public void publish(DomainEvent event) {
        if (!isAvailable()) {
            throw new IllegalStateException("Event publisher service is not available");
        }

        if (event == null) {
            throw new IllegalArgumentException("Event cannot be null");
        }

        long eventId = eventCounter.incrementAndGet();
        publishedEvents.put(eventId, event);

        // Publica evento localmente via Spring
        springEventPublisher.publishEvent(event);

        // Publica evento no RabbitMQ se disponível
        if (rabbitMQEventPublisher != null) {
            try {
                // Usa reflexão para evitar dependência direta
                rabbitMQEventPublisher.getClass()
                    .getMethod("publishEvent", DomainEvent.class)
                    .invoke(rabbitMQEventPublisher, event);
                
                log.debug("Event published to RabbitMQ: type={}, evaluationId={}", 
                         event.getEventType(), event.getEvaluationId());
            } catch (Exception e) {
                log.error("Failed to publish event to RabbitMQ: type={}, evaluationId={}, error={}", 
                         event.getEventType(), event.getEvaluationId(), e.getMessage(), e);
                // Não lança exceção para não interromper o fluxo principal
            }
        }

        log.debug("Domain event published: type={}, evaluationId={}, eventId={}", 
                 event.getEventType(), event.getEvaluationId(), eventId);
    }

    @Override
    public void publishBatch(List<DomainEvent> events) {
        if (!isAvailable()) {
            throw new IllegalStateException("Event publisher service is not available");
        }

        if (events == null || events.isEmpty()) {
            return;
        }

        log.debug("Publishing batch of {} events", events.size());

        for (DomainEvent event : events) {
            publish(event);
        }
    }

    @Override
    public boolean isAvailable() {
        return available;
    }

    /**
     * Simula falha no serviço para testes.
     */
    public void simulateFailure() {
        this.available = false;
        log.warn("Event publisher service simulated as unavailable");
    }

    /**
     * Restaura o serviço para estado normal.
     */
    public void restoreService() {
        this.available = true;
        log.info("Event publisher service restored to available state");
    }

    /**
     * Retorna todos os eventos publicados.
     *
     * @return lista de eventos publicados
     */
    public List<DomainEvent> getPublishedEvents() {
        return new ArrayList<>(publishedEvents.values());
    }

    /**
     * Limpa todos os eventos publicados.
     */
    public void clearEvents() {
        publishedEvents.clear();
        eventCounter.set(0);
        log.debug("Published events cleared");
    }

    /**
     * Retorna o número de eventos publicados.
     *
     * @return contador de eventos
     */
    public long getPublishedEventCount() {
        return eventCounter.get();
    }
}