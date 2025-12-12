package com.gestauto.vehicleevaluation.domain.event;

import com.fasterxml.jackson.annotation.JsonSubTypes;
import com.fasterxml.jackson.annotation.JsonTypeInfo;

import java.time.LocalDateTime;

/**
 * Interface base para todos os eventos de domínio.
 *
 * Eventos de domínio representam ocorrências significativas
 * no negócio que precisam ser comunicadas a outros bounded contexts
 * ou que precisam ser persistidas para auditoria.
 *
 * Configurada com anotações Jackson para serialização polimórfica.
 */
@JsonTypeInfo(
    use = JsonTypeInfo.Id.NAME,
    include = JsonTypeInfo.As.PROPERTY,
    property = "eventType"
)
@JsonSubTypes({
    @JsonSubTypes.Type(value = EvaluationCreatedEvent.class, name = "EvaluationCreated"),
    @JsonSubTypes.Type(value = EvaluationSubmittedEvent.class, name = "EvaluationSubmitted"),
    @JsonSubTypes.Type(value = EvaluationApprovedEvent.class, name = "EvaluationApproved"),
    @JsonSubTypes.Type(value = EvaluationRejectedEvent.class, name = "EvaluationRejected"),
    @JsonSubTypes.Type(value = VehicleEvaluationCompletedEvent.class, name = "VehicleEvaluationCompleted"),
    @JsonSubTypes.Type(value = ChecklistCompletedEvent.class, name = "ChecklistCompleted"),
    @JsonSubTypes.Type(value = ValuationCalculatedEvent.class, name = "ValuationCalculated")
})
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