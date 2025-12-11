package com.gestauto.vehicleevaluation.application.dto;

import com.gestauto.vehicleevaluation.domain.value.Money;
import java.util.List;
import java.util.Objects;

/**
 * DTO que contém o resultado completo do cálculo de valoração.
 *
 * Encapsula todas as informações sobre o cálculo de valoração,
 * incluindo componentes individuais, detalhes de depreciações
 * e o valor final sugerido.
 */
public class ValuationResultDto {
    private final String evaluationId;
    private final Money fipePrice;
    private final Money totalDepreciation;
    private final Money safetyMargin;
    private final Money profitMargin;
    private final Money suggestedValue;
    private final List<DepreciationDetailDto> depreciationDetails;
    private final Double liquidityPercentage;
    private final Double manualAdjustmentPercentage;
    private final Money manualAdjustmentAmount;
    private final Money finalValue;

    private ValuationResultDto(
        String evaluationId,
        Money fipePrice,
        Money totalDepreciation,
        Money safetyMargin,
        Money profitMargin,
        Money suggestedValue,
        List<DepreciationDetailDto> depreciationDetails,
        Double liquidityPercentage,
        Double manualAdjustmentPercentage,
        Money manualAdjustmentAmount,
        Money finalValue
    ) {
        this.evaluationId = Objects.requireNonNull(evaluationId, "evaluationId não pode ser nulo");
        this.fipePrice = Objects.requireNonNull(fipePrice, "fipePrice não pode ser nulo");
        this.totalDepreciation = Objects.requireNonNull(totalDepreciation, "totalDepreciation não pode ser nulo");
        this.safetyMargin = Objects.requireNonNull(safetyMargin, "safetyMargin não pode ser nulo");
        this.profitMargin = Objects.requireNonNull(profitMargin, "profitMargin não pode ser nulo");
        this.suggestedValue = Objects.requireNonNull(suggestedValue, "suggestedValue não pode ser nulo");
        this.depreciationDetails = Objects.requireNonNull(depreciationDetails, "depreciationDetails não pode ser nulo");
        this.liquidityPercentage = Objects.requireNonNull(liquidityPercentage, "liquidityPercentage não pode ser nulo");
        this.manualAdjustmentPercentage = manualAdjustmentPercentage;
        this.manualAdjustmentAmount = manualAdjustmentAmount;
        this.finalValue = Objects.requireNonNull(finalValue, "finalValue não pode ser nulo");
    }

    // Getters
    public String getEvaluationId() {
        return evaluationId;
    }

    public Money getFipePrice() {
        return fipePrice;
    }

    public Money getTotalDepreciation() {
        return totalDepreciation;
    }

    public Money getSafetyMargin() {
        return safetyMargin;
    }

    public Money getProfitMargin() {
        return profitMargin;
    }

    public Money getSuggestedValue() {
        return suggestedValue;
    }

    public List<DepreciationDetailDto> getDepreciationDetails() {
        return depreciationDetails;
    }

    public Double getLiquidityPercentage() {
        return liquidityPercentage;
    }

    public Double getManualAdjustmentPercentage() {
        return manualAdjustmentPercentage;
    }

    public Money getManualAdjustmentAmount() {
        return manualAdjustmentAmount;
    }

    public Money getFinalValue() {
        return finalValue;
    }

    /**
     * Builder para criação fluente de ValuationResultDto.
     */
    public static class Builder {
        private String evaluationId;
        private Money fipePrice;
        private Money totalDepreciation;
        private Money safetyMargin;
        private Money profitMargin;
        private Money suggestedValue;
        private List<DepreciationDetailDto> depreciationDetails;
        private Double liquidityPercentage;
        private Double manualAdjustmentPercentage;
        private Money manualAdjustmentAmount;
        private Money finalValue;

        public Builder evaluationId(String evaluationId) {
            this.evaluationId = evaluationId;
            return this;
        }

        public Builder fipePrice(Money fipePrice) {
            this.fipePrice = fipePrice;
            return this;
        }

        public Builder totalDepreciation(Money totalDepreciation) {
            this.totalDepreciation = totalDepreciation;
            return this;
        }

        public Builder safetyMargin(Money safetyMargin) {
            this.safetyMargin = safetyMargin;
            return this;
        }

        public Builder profitMargin(Money profitMargin) {
            this.profitMargin = profitMargin;
            return this;
        }

        public Builder suggestedValue(Money suggestedValue) {
            this.suggestedValue = suggestedValue;
            return this;
        }

        public Builder depreciationDetails(List<DepreciationDetailDto> depreciationDetails) {
            this.depreciationDetails = depreciationDetails;
            return this;
        }

        public Builder liquidityPercentage(Double liquidityPercentage) {
            this.liquidityPercentage = liquidityPercentage;
            return this;
        }

        public Builder manualAdjustmentPercentage(Double manualAdjustmentPercentage) {
            this.manualAdjustmentPercentage = manualAdjustmentPercentage;
            return this;
        }

        public Builder manualAdjustmentAmount(Money manualAdjustmentAmount) {
            this.manualAdjustmentAmount = manualAdjustmentAmount;
            return this;
        }

        public Builder finalValue(Money finalValue) {
            this.finalValue = finalValue;
            return this;
        }

        public ValuationResultDto build() {
            return new ValuationResultDto(
                evaluationId,
                fipePrice,
                totalDepreciation,
                safetyMargin,
                profitMargin,
                suggestedValue,
                depreciationDetails,
                liquidityPercentage,
                manualAdjustmentPercentage,
                manualAdjustmentAmount,
                finalValue
            );
        }
    }
}
