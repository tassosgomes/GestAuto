package com.gestauto.vehicleevaluation.domain.event;

import java.time.LocalDateTime;

/**
 * Evento de domínio emitido quando uma avaliação é rejeitada.
 *
 * Este evento é utilizado para notificar outros bounded contexts
 * sobre a rejeição de uma avaliação de veículo, permitindo
 * que notificações e processos de retrabalho sejam iniciados.
 */
public final class EvaluationRejectedEvent implements DomainEvent {

    private final LocalDateTime occurredAt;
    private final String evaluationId;
    private final String reviewerId;
    private final String plate;
    private final String rejectionReason;

    /**
     * Constrói um novo evento de rejeição de avaliação.
     *
     * @param evaluationId ID da avaliação
     * @param reviewerId ID do revisor
     * @param plate placa do veículo
     * @param rejectionReason motivo da rejeição
     */
    public EvaluationRejectedEvent(String evaluationId, String reviewerId,
                                   String plate, String rejectionReason) {
        this.occurredAt = LocalDateTime.now();
        this.evaluationId = evaluationId;
        this.reviewerId = reviewerId;
        this.plate = plate;
        this.rejectionReason = rejectionReason;
    }

    @Override
    public LocalDateTime getOccurredAt() {
        return occurredAt;
    }

    @Override
    public String getEventType() {
        return "EvaluationRejected";
    }

    @Override
    public String getEvaluationId() {
        return evaluationId;
    }

    @Override
    public String getEventData() {
        return String.format(
            "{" +
            "\"reviewerId\":\"%s\"," +
            "\"plate\":\"%s\"," +
            "\"rejectionReason\":\"%s\"" +
            "}",
            reviewerId, plate, rejectionReason != null ? rejectionReason : ""
        );
    }

    // Getters
    public String getReviewerId() {
        return reviewerId;
    }

    public String getPlate() {
        return plate;
    }

    public String getRejectionReason() {
        return rejectionReason;
    }
}
