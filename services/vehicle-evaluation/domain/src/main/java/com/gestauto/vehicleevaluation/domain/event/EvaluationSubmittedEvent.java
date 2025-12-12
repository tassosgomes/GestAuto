package com.gestauto.vehicleevaluation.domain.event;

import java.time.LocalDateTime;

/**
 * Evento de domínio emitido quando uma avaliação é submetida para aprovação.
 *
 * Este evento é utilizado para notificar outros bounded contexts
 * sobre a submissão de uma avaliação para revisão gerencial.
 */
public final class EvaluationSubmittedEvent implements DomainEvent {

    private final LocalDateTime occurredAt;
    private final String evaluationId;
    private final String evaluatorId;
    private final String plate;
    private final Double suggestedValue;
    private final Double fipePrice;

    /**
     * Constrói um novo evento de submissão de avaliação.
     *
     * @param evaluationId ID da avaliação
     * @param evaluatorId ID do avaliador
     * @param plate placa do veículo
     * @param suggestedValue valor sugerido pela avaliação
     * @param fipePrice valor FIPE do veículo
     */
    public EvaluationSubmittedEvent(String evaluationId, String evaluatorId,
                                    String plate, Double suggestedValue, Double fipePrice) {
        this.occurredAt = LocalDateTime.now();
        this.evaluationId = evaluationId;
        this.evaluatorId = evaluatorId;
        this.plate = plate;
        this.suggestedValue = suggestedValue;
        this.fipePrice = fipePrice;
    }

    @Override
    public LocalDateTime getOccurredAt() {
        return occurredAt;
    }

    @Override
    public String getEventType() {
        return "EvaluationSubmitted";
    }

    @Override
    public String getEvaluationId() {
        return evaluationId;
    }

    @Override
    public String getEventData() {
        return String.format(
            "{" +
            "\"evaluatorId\":\"%s\"," +
            "\"plate\":\"%s\"," +
            "\"suggestedValue\":%.2f," +
            "\"fipePrice\":%.2f" +
            "}",
            evaluatorId, plate, suggestedValue, fipePrice
        );
    }

    // Getters
    public String getEvaluatorId() {
        return evaluatorId;
    }

    public String getPlate() {
        return plate;
    }

    public Double getSuggestedValue() {
        return suggestedValue;
    }

    public Double getFipePrice() {
        return fipePrice;
    }
}
