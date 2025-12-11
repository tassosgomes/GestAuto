package com.gestauto.vehicleevaluation.application.service;

import java.math.BigDecimal;
import java.util.Objects;

/**
 * Configuração de valoração com margens e percentuais ajustáveis.
 *
 * Encapsula os parâmetros de negócio para o cálculo de valoração,
 * permitindo ajustes sem alteração de código.
 */
public class ValuationConfig {
    private final Double safetyMarginPercentage;
    private final Double profitMarginPercentage;
    private final Integer maxManualAdjustmentPercentage;
    private final Boolean requireManagerApprovalForAdjustment;

    private ValuationConfig(
        Double safetyMarginPercentage,
        Double profitMarginPercentage,
        Integer maxManualAdjustmentPercentage,
        Boolean requireManagerApprovalForAdjustment
    ) {
        this.safetyMarginPercentage = Objects.requireNonNull(
            safetyMarginPercentage,
            "safetyMarginPercentage não pode ser nulo"
        );
        this.profitMarginPercentage = Objects.requireNonNull(
            profitMarginPercentage,
            "profitMarginPercentage não pode ser nulo"
        );
        this.maxManualAdjustmentPercentage = Objects.requireNonNull(
            maxManualAdjustmentPercentage,
            "maxManualAdjustmentPercentage não pode ser nulo"
        );
        this.requireManagerApprovalForAdjustment = Objects.requireNonNull(
            requireManagerApprovalForAdjustment,
            "requireManagerApprovalForAdjustment não pode ser nulo"
        );

        validate();
    }

    /**
     * Cria uma configuração de valoração com valores padrão.
     */
    public static ValuationConfig defaultConfig() {
        return new ValuationConfig(
            10.0,    // Safety margin: 10%
            15.0,    // Profit margin: 15%
            10,      // Max manual adjustment: 10%
            true     // Require manager approval
        );
    }

    /**
     * Factory method para criar configuração customizada.
     */
    public static ValuationConfig of(
        Double safetyMarginPercentage,
        Double profitMarginPercentage,
        Integer maxManualAdjustmentPercentage,
        Boolean requireManagerApprovalForAdjustment
    ) {
        return new ValuationConfig(
            safetyMarginPercentage,
            profitMarginPercentage,
            maxManualAdjustmentPercentage,
            requireManagerApprovalForAdjustment
        );
    }

    private void validate() {
        if (safetyMarginPercentage < 0) {
            throw new IllegalArgumentException("Safety margin percentage não pode ser negativa");
        }
        if (profitMarginPercentage < 0) {
            throw new IllegalArgumentException("Profit margin percentage não pode ser negativa");
        }
        if (maxManualAdjustmentPercentage < 0 || maxManualAdjustmentPercentage > 100) {
            throw new IllegalArgumentException(
                "Max manual adjustment percentage deve estar entre 0 e 100"
            );
        }
    }

    public Double getSafetyMarginPercentage() {
        return safetyMarginPercentage;
    }

    public Double getProfitMarginPercentage() {
        return profitMarginPercentage;
    }

    public Integer getMaxManualAdjustmentPercentage() {
        return maxManualAdjustmentPercentage;
    }

    public Boolean isRequireManagerApprovalForAdjustment() {
        return requireManagerApprovalForAdjustment;
    }

    /**
     * Converte percentual para BigDecimal para cálculos monetários.
     */
    public BigDecimal getSafetyMarginAsDecimal() {
        return BigDecimal.valueOf(safetyMarginPercentage).divide(
            BigDecimal.valueOf(100),
            4,
            java.math.RoundingMode.HALF_UP
        );
    }

    /**
     * Converte percentual para BigDecimal para cálculos monetários.
     */
    public BigDecimal getProfitMarginAsDecimal() {
        return BigDecimal.valueOf(profitMarginPercentage).divide(
            BigDecimal.valueOf(100),
            4,
            java.math.RoundingMode.HALF_UP
        );
    }

    @Override
    public String toString() {
        return "ValuationConfig{" +
                "safetyMarginPercentage=" + safetyMarginPercentage +
                ", profitMarginPercentage=" + profitMarginPercentage +
                ", maxManualAdjustmentPercentage=" + maxManualAdjustmentPercentage +
                ", requireManagerApprovalForAdjustment=" + requireManagerApprovalForAdjustment +
                '}';
    }
}
