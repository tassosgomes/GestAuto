package com.gestauto.vehicleevaluation.application.service;

import com.gestauto.vehicleevaluation.domain.event.DomainEvent;

/**
 * Serviço para publicação de eventos de domínio.
 *
 * Responsável por publicar eventos de domínio em message brokers
 * como RabbitMQ, Kafka ou outros sistemas de mensageria.
 */
public interface DomainEventPublisherService {

    /**
     * Publica um evento de domínio.
     *
     * @param event evento a ser publicado
     */
    void publish(DomainEvent event);

    /**
     * Publica múltiplos eventos de domínio em lote.
     *
     * @param events lista de eventos a serem publicados
     */
    void publishBatch(java.util.List<DomainEvent> events);

    /**
     * Verifica se o serviço de publicação está disponível.
     *
     * @return true se o serviço estiver ativo
     */
    boolean isAvailable();
}