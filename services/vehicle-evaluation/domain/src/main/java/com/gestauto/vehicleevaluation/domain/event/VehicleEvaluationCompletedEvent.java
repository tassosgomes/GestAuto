package com.gestauto.vehicleevaluation.domain.event;

import java.time.LocalDateTime;
import java.util.Map;

/**
 * Evento de domínio emitido quando uma avaliação de veículo é completada.
 *
 * Este evento é o evento final no ciclo de vida de uma avaliação,
 * indicando que o veículo foi totalmente avaliado, aprovado e está
 * pronto para integração com outros sistemas (estoque, propostas, etc).
 */
public final class VehicleEvaluationCompletedEvent implements DomainEvent {

    private final LocalDateTime occurredAt;
    private final String evaluationId;
    private final String plate;
    private final String brand;
    private final String model;
    private final Integer year;
    private final Double finalValue;
    private final LocalDateTime validUntil;
    private final Map<String, Object> evaluationData;

    /**
     * Constrói um novo evento de conclusão de avaliação.
     *
     * @param evaluationId ID da avaliação
     * @param plate placa do veículo
     * @param brand marca do veículo
     * @param model modelo do veículo
     * @param year ano do veículo
     * @param finalValue valor final aprovado
     * @param validUntil data de validade da avaliação
     * @param evaluationData dados adicionais da avaliação
     */
    public VehicleEvaluationCompletedEvent(String evaluationId, String plate, String brand,
                                          String model, Integer year, Double finalValue,
                                          LocalDateTime validUntil, Map<String, Object> evaluationData) {
        this.occurredAt = LocalDateTime.now();
        this.evaluationId = evaluationId;
        this.plate = plate;
        this.brand = brand;
        this.model = model;
        this.year = year;
        this.finalValue = finalValue;
        this.validUntil = validUntil;
        this.evaluationData = evaluationData;
    }

    @Override
    public LocalDateTime getOccurredAt() {
        return occurredAt;
    }

    @Override
    public String getEventType() {
        return "VehicleEvaluationCompleted";
    }

    @Override
    public String getEvaluationId() {
        return evaluationId;
    }

    @Override
    public String getEventData() {
        return String.format(
            "{" +
            "\"plate\":\"%s\"," +
            "\"brand\":\"%s\"," +
            "\"model\":\"%s\"," +
            "\"year\":%d," +
            "\"finalValue\":%.2f," +
            "\"validUntil\":\"%s\"" +
            "}",
            plate, brand, model, year, finalValue, validUntil.toString()
        );
    }

    // Getters
    public String getPlate() {
        return plate;
    }

    public String getBrand() {
        return brand;
    }

    public String getModel() {
        return model;
    }

    public Integer getYear() {
        return year;
    }

    public Double getFinalValue() {
        return finalValue;
    }

    public LocalDateTime getValidUntil() {
        return validUntil;
    }

    public Map<String, Object> getEvaluationData() {
        return evaluationData;
    }
}
