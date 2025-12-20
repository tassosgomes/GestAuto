package com.gestauto.vehicleevaluation.application.service;

import org.junit.jupiter.api.Test;

import java.math.BigDecimal;

import static org.assertj.core.api.Assertions.assertThat;
import static org.assertj.core.api.Assertions.assertThatThrownBy;

class ValuationConfigTest {

    @Test
    void defaultConfig_exposesExpectedDefaultsAndDecimals() {
        var cfg = ValuationConfig.defaultConfig();

        assertThat(cfg.getSafetyMarginPercentage()).isEqualTo(10.0);
        assertThat(cfg.getProfitMarginPercentage()).isEqualTo(15.0);
        assertThat(cfg.getMaxManualAdjustmentPercentage()).isEqualTo(10);
        assertThat(cfg.isRequireManagerApprovalForAdjustment()).isTrue();

        assertThat(cfg.getSafetyMarginAsDecimal()).isEqualTo(new BigDecimal("0.1000"));
        assertThat(cfg.getProfitMarginAsDecimal()).isEqualTo(new BigDecimal("0.1500"));
        assertThat(cfg.toString()).contains("safetyMarginPercentage=10.0");
    }

    @Test
    void of_validatesRanges() {
        assertThatThrownBy(() -> ValuationConfig.of(-0.1, 10.0, 10, true))
                .isInstanceOf(IllegalArgumentException.class)
                .hasMessageContaining("Safety margin percentage");

        assertThatThrownBy(() -> ValuationConfig.of(0.0, -1.0, 10, true))
                .isInstanceOf(IllegalArgumentException.class)
                .hasMessageContaining("Profit margin percentage");

        assertThatThrownBy(() -> ValuationConfig.of(0.0, 0.0, -1, true))
                .isInstanceOf(IllegalArgumentException.class)
                .hasMessageContaining("Max manual adjustment percentage");

        assertThatThrownBy(() -> ValuationConfig.of(0.0, 0.0, 101, true))
                .isInstanceOf(IllegalArgumentException.class)
                .hasMessageContaining("Max manual adjustment percentage");
    }

    @Test
    void of_rejectsNullsEarly() {
        assertThatThrownBy(() -> ValuationConfig.of(null, 1.0, 10, true))
                .isInstanceOf(NullPointerException.class)
                .hasMessageContaining("safetyMarginPercentage");

        assertThatThrownBy(() -> ValuationConfig.of(1.0, null, 10, true))
                .isInstanceOf(NullPointerException.class)
                .hasMessageContaining("profitMarginPercentage");

        assertThatThrownBy(() -> ValuationConfig.of(1.0, 1.0, null, true))
                .isInstanceOf(NullPointerException.class)
                .hasMessageContaining("maxManualAdjustmentPercentage");

        assertThatThrownBy(() -> ValuationConfig.of(1.0, 1.0, 10, null))
                .isInstanceOf(NullPointerException.class)
                .hasMessageContaining("requireManagerApprovalForAdjustment");
    }
}
