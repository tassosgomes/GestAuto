package com.gestauto.vehicleevaluation.domain.event;

import java.time.LocalDateTime;

/**
 * Evento de domínio emitido quando uma avaliação é aprovada.
 *
 * Este evento é utilizado para notificar outros bounded contexts
 * sobre a aprovação de uma avaliação de veículo, permitindo
 * que processos downstream (como criação de propostas) sejam iniciados.
 */
public final class EvaluationApprovedEvent implements DomainEvent {

    private final LocalDateTime occurredAt;
    private final String evaluationId;
    private final String approverId;
    private final String plate;
    private final Double approvedValue;
    private final String validationToken;
    private final LocalDateTime validUntil;

    /**
     * Constrói um novo evento de aprovação de avaliação.
     *
     * @param evaluationId ID da avaliação
     * @param approverId ID do aprovador
     * @param plate placa do veículo
     * @param approvedValue valor aprovado para o veículo
     * @param validationToken token para validação externa
     * @param validUntil data de validade da avaliação
     */
    public EvaluationApprovedEvent(String evaluationId, String approverId, String plate,
                                   Double approvedValue, String validationToken, LocalDateTime validUntil) {
        this.occurredAt = LocalDateTime.now();
        this.evaluationId = evaluationId;
        this.approverId = approverId;
        this.plate = plate;
        this.approvedValue = approvedValue;
        this.validationToken = validationToken;
        this.validUntil = validUntil;
    }

    @Override
    public LocalDateTime getOccurredAt() {
        return occurredAt;
    }

    @Override
    public String getEventType() {
        return "EvaluationApproved";
    }

    @Override
    public String getEvaluationId() {
        return evaluationId;
    }

    @Override
    public String getEventData() {
        return String.format(
            "{" +
            "\"approverId\":\"%s\"," +
            "\"plate\":\"%s\"," +
            "\"approvedValue\":%.2f," +
            "\"validationToken\":\"%s\"," +
            "\"validUntil\":\"%s\"" +
            "}",
            approverId, plate, approvedValue, validationToken, validUntil.toString()
        );
    }

    // Getters
    public String getApproverId() {
        return approverId;
    }

    public String getPlate() {
        return plate;
    }

    public Double getApprovedValue() {
        return approvedValue;
    }

    public String getValidationToken() {
        return validationToken;
    }

    public LocalDateTime getValidUntil() {
        return validUntil;
    }
}
