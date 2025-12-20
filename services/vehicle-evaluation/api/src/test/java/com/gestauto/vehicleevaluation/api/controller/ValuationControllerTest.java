package com.gestauto.vehicleevaluation.api.controller;

import com.gestauto.vehicleevaluation.application.command.CalculateValuationHandler;
import com.gestauto.vehicleevaluation.application.dto.ValuationResultDto;
import com.gestauto.vehicleevaluation.domain.value.Money;
import java.util.List;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;

import static org.mockito.ArgumentMatchers.any;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;
import static org.assertj.core.api.Assertions.assertThat;

class ValuationControllerTest {

    CalculateValuationHandler calculateValuationHandler;

    ValuationController controller;

    @BeforeEach
    void setUp() {
        calculateValuationHandler = mock(CalculateValuationHandler.class);
        controller = new ValuationController(calculateValuationHandler);
    }

    @Test
    void calculateValuationReturns200WithoutRequestBody() throws Exception {
        when(calculateValuationHandler.handle(any())).thenReturn(minimalResult());

        var response = controller.calculateValuation("eval-1", null);
        assertThat(response.getStatusCode().value()).isEqualTo(200);
    }

    @Test
    void calculateValuationReturns200WithManualAdjustment() throws Exception {
        when(calculateValuationHandler.handle(any())).thenReturn(minimalResult());

        var request = new ValuationController.CalculateValuationRequest(5.0);
        var response = controller.calculateValuation("eval-1", request);
        assertThat(response.getStatusCode().value()).isEqualTo(200);
    }

    @Test
    void calculateValuationReturns400OnIllegalArgumentException() throws Exception {
        when(calculateValuationHandler.handle(any())).thenThrow(new IllegalArgumentException("bad"));

        var response = controller.calculateValuation("eval-1", null);
        assertThat(response.getStatusCode().value()).isEqualTo(400);
    }

    @Test
    void calculateValuationReturns500OnUnexpectedException() throws Exception {
        when(calculateValuationHandler.handle(any())).thenThrow(new RuntimeException("boom"));

        var response = controller.calculateValuation("eval-1", null);
        assertThat(response.getStatusCode().value()).isEqualTo(500);
    }

    private static ValuationResultDto minimalResult() {
        return new ValuationResultDto.Builder()
            .evaluationId("eval-1")
            .fipePrice(Money.of(1000))
            .totalDepreciation(Money.of(100))
            .safetyMargin(Money.of(10))
            .profitMargin(Money.of(10))
            .suggestedValue(Money.of(920))
            .depreciationDetails(List.of())
            .liquidityPercentage(0.8)
            .finalValue(Money.of(920))
            .build();
    }
}
