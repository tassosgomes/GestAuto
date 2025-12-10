package com.gestauto.vehicleevaluation.domain.event;

import java.time.LocalDateTime;

/**
 * Evento de domínio emitido quando uma avaliação é criada.
 *
 * Este evento é utilizado para notificar outros bounded contexts
 * sobre a criação de uma nova avaliação de veículo.
 */
public final class EvaluationCreatedEvent implements DomainEvent {

    private final LocalDateTime occurredAt;
    private final String evaluationId;
    private final String evaluatorId;
    private final String plate;
    private final String brand;
    private final String model;

    /**
     * Constrói um novo evento de criação de avaliação.
     *
     * @param evaluationId ID da avaliação
     * @param evaluatorId ID do avaliador
     * @param plate placa do veículo
     * @param brand marca do veículo
     * @param model modelo do veículo
     */
    public EvaluationCreatedEvent(String evaluationId, String evaluatorId,
                                  String plate, String brand, String model) {
        this.occurredAt = LocalDateTime.now();
        this.evaluationId = evaluationId;
        this.evaluatorId = evaluatorId;
        this.plate = plate;
        this.brand = brand;
        this.model = model;
    }

    @Override
    public LocalDateTime getOccurredAt() {
        return occurredAt;
    }

    @Override
    public String getEventType() {
        return "EvaluationCreated";
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
            "\"brand\":\"%s\"," +
            "\"model\":\"%s\"" +
            "}",
            evaluatorId, plate, brand, model
        );
    }

    // Getters
    public String getEvaluatorId() {
        return evaluatorId;
    }

    public String getPlate() {
        return plate;
    }

    public String getBrand() {
        return brand;
    }

    public String getModel() {
        return model;
    }
}