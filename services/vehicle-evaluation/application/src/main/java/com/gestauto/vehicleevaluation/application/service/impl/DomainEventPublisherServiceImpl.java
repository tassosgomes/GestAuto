package com.gestauto.vehicleevaluation.application.service.impl;

import com.gestauto.vehicleevaluation.application.service.DomainEventPublisherService;
import com.gestauto.vehicleevaluation.domain.event.DomainEvent;
import org.springframework.stereotype.Service;

import java.util.ArrayList;
import java.util.List;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.atomic.AtomicLong;

/**
 * Implementação mock do serviço de publicação de eventos de domínio.
 *
 * Para desenvolvimento, esta implementação apenas armazena os eventos
 * em memória para simular a publicação em message brokers.
 */
@Service
public class DomainEventPublisherServiceImpl implements DomainEventPublisherService {

    private final AtomicLong eventCounter = new AtomicLong(0);
    private final ConcurrentHashMap<Long, DomainEvent> publishedEvents = new ConcurrentHashMap<>();
    private boolean available = true;

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

        // Simula publicação assíncrona
        simulateAsyncPublish(eventId, event);
    }

    @Override
    public void publishBatch(List<DomainEvent> events) {
        if (!isAvailable()) {
            throw new IllegalStateException("Event publisher service is not available");
        }

        if (events == null || events.isEmpty()) {
            return;
        }

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
    }

    /**
     * Restaura o serviço para estado normal.
     */
    public void restoreService() {
        this.available = true;
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
    }

    /**
     * Retorna o número de eventos publicados.
     *
     * @return contador de eventos
     */
    public long getPublishedEventCount() {
        return eventCounter.get();
    }

    /**
     * Simula publicação assíncrona com delay.
     */
    private void simulateAsyncPublish(long eventId, DomainEvent event) {
        // Em uma implementação real, isto seria publicado em RabbitMQ, Kafka, etc.
        try {
            Thread.sleep(10); // Simula delay de rede
        } catch (InterruptedException e) {
            Thread.currentThread().interrupt();
            throw new RuntimeException("Publishing interrupted", e);
        }
    }
}