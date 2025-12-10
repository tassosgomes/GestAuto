package com.gestauto.vehicleevaluation.domain.event;

import java.time.LocalDateTime;

/**
 * Interface base para todos os eventos de domínio.
 *
 * Eventos de domínio representam ocorrências significativas
 * no negócio que precisam ser comunicadas a outros bounded contexts
 * ou que precisam ser persistidas para auditoria.
 */
public interface DomainEvent {

    /**
     * Retorna o timestamp de quando o evento ocorreu.
     *
     * @return timestamp do evento
     */
    LocalDateTime getOccurredAt();

    /**
     * Retorna o tipo do evento para identificação.
     *
     * @return tipo do evento
     */
    String getEventType();

    /**
     * Retorna o ID da avaliação relacionada ao evento.
     *
     * @return ID da avaliação
     */
    String getEvaluationId();

    /**
     * Retorna dados adicionais do evento em formato JSON.
     *
     * @return dados adicionais como string JSON
     */
    String getEventData();
}