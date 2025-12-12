package com.gestauto.vehicleevaluation.domain.event;

import com.gestauto.vehicleevaluation.domain.value.Money;

import java.time.LocalDateTime;

/**
 * Evento de domínio emitido quando uma valoração é calculada.
 *
 * Este evento é utilizado para notificar outros bounded contexts
 * sobre o cálculo de valoração de um veículo, incluindo os valores
 * calculados e componentes do cálculo.
 */
public final class ValuationCalculatedEvent implements DomainEvent {

    private final LocalDateTime occurredAt;
    private final String evaluationId;
    private final Money fipePrice;
    private final Money totalDepreciation;
    private final Money suggestedValue;
    private final Money finalValue;
    private final Double liquidityPercentage;
    private final boolean hasManualAdjustment;

    /**
     * Constrói um novo evento de cálculo de valoração.
     *
     * @param evaluationId ID da avaliação
     * @param fipePrice preço FIPE do veículo
     * @param totalDepreciation total de depreciações aplicadas
     * @param suggestedValue valor sugerido calculado
     * @param finalValue valor final (com ajuste manual se aplicável)
     * @param liquidityPercentage percentual de liquidez aplicado
     * @param hasManualAdjustment indica se houve ajuste manual
     */
    public ValuationCalculatedEvent(
        String evaluationId,
        Money fipePrice,
        Money totalDepreciation,
        Money suggestedValue,
        Money finalValue,
        Double liquidityPercentage,
        boolean hasManualAdjustment
    ) {
        this.occurredAt = LocalDateTime.now();
        this.evaluationId = evaluationId;
        this.fipePrice = fipePrice;
        this.totalDepreciation = totalDepreciation;
        this.suggestedValue = suggestedValue;
        this.finalValue = finalValue;
        this.liquidityPercentage = liquidityPercentage;
        this.hasManualAdjustment = hasManualAdjustment;
    }

    @Override
    public LocalDateTime getOccurredAt() {
        return occurredAt;
    }

    @Override
    public String getEventType() {
        return "ValuationCalculated";
    }

    @Override
    public String getEvaluationId() {
        return evaluationId;
    }

    @Override
    public String getEventData() {
        return String.format(
            "{" +
            "\"fipePrice\":\"%s\"," +
            "\"totalDepreciation\":\"%s\"," +
            "\"suggestedValue\":\"%s\"," +
            "\"finalValue\":\"%s\"," +
            "\"liquidityPercentage\":%s," +
            "\"hasManualAdjustment\":%s" +
            "}",
            String.valueOf(fipePrice),
            String.valueOf(totalDepreciation),
            String.valueOf(suggestedValue),
            String.valueOf(finalValue),
            liquidityPercentage != null ? liquidityPercentage.toString() : "null",
            Boolean.toString(hasManualAdjustment)
        );
    }

    public Money getFipePrice() {
        return fipePrice;
    }

    public Money getTotalDepreciation() {
        return totalDepreciation;
    }

    public Money getSuggestedValue() {
        return suggestedValue;
    }

    public Money getFinalValue() {
        return finalValue;
    }

    public Double getLiquidityPercentage() {
        return liquidityPercentage;
    }

    public boolean hasManualAdjustment() {
        return hasManualAdjustment;
    }

    @Override
    public String toString() {
        return "ValuationCalculatedEvent{" +
                "occurredAt=" + occurredAt +
                ", evaluationId='" + evaluationId + '\'' +
                ", fipePrice=" + fipePrice +
                ", totalDepreciation=" + totalDepreciation +
                ", suggestedValue=" + suggestedValue +
                ", finalValue=" + finalValue +
                ", liquidityPercentage=" + liquidityPercentage +
                ", hasManualAdjustment=" + hasManualAdjustment +
                '}';
    }
}
