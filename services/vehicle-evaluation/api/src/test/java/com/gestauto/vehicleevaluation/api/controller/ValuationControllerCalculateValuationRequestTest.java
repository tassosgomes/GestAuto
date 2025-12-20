package com.gestauto.vehicleevaluation.api.controller;

import static org.assertj.core.api.Assertions.assertThat;

import org.junit.jupiter.api.Test;

class ValuationControllerCalculateValuationRequestTest {

    @Test
    void lombokGeneratedMethodsAreCovered() {
        var empty = new ValuationController.CalculateValuationRequest();
        assertThat(empty.getManualAdjustmentPercentage()).isNull();

        empty.setManualAdjustmentPercentage(5.0);
        assertThat(empty.getManualAdjustmentPercentage()).isEqualTo(5.0);

        var same = new ValuationController.CalculateValuationRequest(5.0);
        var different = new ValuationController.CalculateValuationRequest(2.0);

        assertThat(empty).isEqualTo(same);
        assertThat(empty).isNotEqualTo(different);
        assertThat(empty.hashCode()).isEqualTo(same.hashCode());
        assertThat(empty.toString()).contains("manualAdjustmentPercentage");
    }
}
