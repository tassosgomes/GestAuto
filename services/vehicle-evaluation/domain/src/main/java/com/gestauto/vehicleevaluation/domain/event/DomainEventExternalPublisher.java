package com.gestauto.vehicleevaluation.domain.event;

/**
 * Porta para publicação de eventos de domínio em canais externos (ex.: mensageria).
 */
public interface DomainEventExternalPublisher {

    void publishEvent(DomainEvent event);
}
