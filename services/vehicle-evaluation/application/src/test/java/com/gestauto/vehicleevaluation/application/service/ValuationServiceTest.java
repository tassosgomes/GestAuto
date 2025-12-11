package com.gestauto.vehicleevaluation.application.service;

import com.gestauto.vehicleevaluation.application.dto.ValuationResultDto;
import com.gestauto.vehicleevaluation.domain.entity.DepreciationItem;
import com.gestauto.vehicleevaluation.domain.entity.VehicleEvaluation;
import com.gestauto.vehicleevaluation.domain.enums.FuelType;
import com.gestauto.vehicleevaluation.domain.value.EvaluationId;
import com.gestauto.vehicleevaluation.domain.value.Money;
import com.gestauto.vehicleevaluation.domain.value.Plate;
import com.gestauto.vehicleevaluation.domain.value.VehicleInfo;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.mockito.Mock;
import org.mockito.junit.jupiter.MockitoExtension;

import java.math.BigDecimal;
import java.util.Optional;

import static org.junit.jupiter.api.Assertions.*;
import static org.mockito.ArgumentMatchers.*;
import static org.mockito.Mockito.lenient;
import static org.mockito.Mockito.when;

/**
 * Testes unitários para ValuationService.
 */
@ExtendWith(MockitoExtension.class)
class ValuationServiceTest {

    @Mock
    private FipeService fipeService;

    private ValuationService valuationService;
    private ValuationConfig defaultConfig;
    private Money fipePrice;

    @BeforeEach
    void setUp() {
        valuationService = new ValuationService(fipeService);
        defaultConfig = ValuationConfig.defaultConfig();
        fipePrice = Money.of(BigDecimal.valueOf(100000));
        
        // Setup common stubbing for all tests using lenient() to avoid UnnecessaryStubbing exceptions
        lenient().when(fipeService.getFipePrice("Toyota", "Corolla", 2023, FuelType.FLEX))
            .thenReturn(Optional.of(fipePrice));
        lenient().when(fipeService.calculateLiquidityPercentage("Toyota", "Corolla", 0))
            .thenReturn(0.85);
    }

    @Test
    void shouldCalculateValuationWithoutDepreciation() {
        // Arrange
        VehicleEvaluation evaluation = createTestEvaluation();

        // Act
        ValuationResultDto result = valuationService.calculateValuation(evaluation, defaultConfig);

        // Assert
        assertNotNull(result);
        assertEquals(fipePrice, result.getFipePrice());
        assertTrue(result.getTotalDepreciation().isZero());
        assertTrue(result.getSafetyMargin().isGreaterThan(Money.ZERO));
        assertTrue(result.getProfitMargin().isGreaterThan(Money.ZERO));
        assertTrue(result.getSuggestedValue().isGreaterThan(Money.ZERO));
    }

    @Test
    void shouldCalculateValuationWithDepreciation() {
        // Arrange
        VehicleEvaluation evaluation = createTestEvaluation();
        evaluation.addDepreciationItem(
            DepreciationItem.create(
                evaluation.getId(),
                "MECHANICAL",
                "Engine problem",
                Money.of(BigDecimal.valueOf(5000)),
                "Minor engine issue",
                "evaluator-1"
            )
        );

        // Act
        ValuationResultDto result = valuationService.calculateValuation(evaluation, defaultConfig);

        // Assert
        assertNotNull(result);
        assertEquals(Money.of(BigDecimal.valueOf(5000)), result.getTotalDepreciation());
        assertTrue(result.getSuggestedValue().isLessThan(
            Money.of(BigDecimal.valueOf(100000)).multiply(BigDecimal.valueOf(0.85))
        ));
    }

    @Test
    void shouldApplyManualAdjustmentPositive() {
        // Arrange
        VehicleEvaluation evaluation = createTestEvaluation();

        // Act
        ValuationResultDto result = valuationService.calculateValuationWithManualAdjustment(
            evaluation,
            defaultConfig,
            5.0 // 5% positive adjustment
        );

        // Assert
        assertNotNull(result);
        assertEquals(5.0, result.getManualAdjustmentPercentage());
        assertNotNull(result.getManualAdjustmentAmount());
        assertTrue(result.getFinalValue().isGreaterThan(result.getSuggestedValue()));
    }

    @Test
    void shouldApplyManualAdjustmentNegative() {
        // Arrange
        VehicleEvaluation evaluation = createTestEvaluation();

        // Act
        ValuationResultDto result = valuationService.calculateValuationWithManualAdjustment(
            evaluation,
            defaultConfig,
            -5.0 // 5% negative adjustment
        );

        // Assert
        assertNotNull(result);
        assertEquals(-5.0, result.getManualAdjustmentPercentage());
        assertNotNull(result.getManualAdjustmentAmount());
        assertTrue(result.getFinalValue().isLessThan(result.getSuggestedValue()));
    }

    @Test
    void shouldThrowExceptionForExcessivePositiveAdjustment() {
        // Arrange
        VehicleEvaluation evaluation = createTestEvaluation();

        // Act & Assert
        assertThrows(IllegalArgumentException.class, () ->
            valuationService.calculateValuationWithManualAdjustment(
                evaluation,
                defaultConfig,
                15.0 // Exceeds 10% limit
            )
        );
    }

    @Test
    void shouldThrowExceptionForExcessiveNegativeAdjustment() {
        // Arrange
        VehicleEvaluation evaluation = createTestEvaluation();

        // Act & Assert
        assertThrows(IllegalArgumentException.class, () ->
            valuationService.calculateValuationWithManualAdjustment(
                evaluation,
                defaultConfig,
                -15.0 // Exceeds -10% limit
            )
        );
    }

    @Test
    void shouldThrowExceptionWhenFipePriceNotAvailable() {
        // Arrange
        VehicleEvaluation evaluation = createTestEvaluation();

        when(fipeService.getFipePrice("Toyota", "Corolla", 2023, FuelType.FLEX))
            .thenReturn(Optional.empty());

        // Act & Assert
        assertThrows(IllegalArgumentException.class, () ->
            valuationService.calculateValuation(evaluation, defaultConfig)
        );
    }

    @Test
    void shouldReturnNullAdjustmentWhenNullParameterProvided() {
        // Arrange
        VehicleEvaluation evaluation = createTestEvaluation();

        // Act
        ValuationResultDto result = valuationService.calculateValuationWithManualAdjustment(
            evaluation,
            defaultConfig,
            null
        );

        // Assert
        assertNotNull(result);
        assertNull(result.getManualAdjustmentPercentage());
        assertNull(result.getManualAdjustmentAmount());
    }

    // Helper methods
    private VehicleEvaluation createTestEvaluation() {
        Plate plate = Plate.of("ABC1234");
        VehicleInfo vehicleInfo = VehicleInfo.of(
            "Toyota",
            "Corolla",
            "2.0 XEI",
            2023,
            2023,
            "Prata",
            FuelType.FLEX
        );
        Money mileage = Money.of(BigDecimal.valueOf(10000));

        return VehicleEvaluation.create(
            plate,
            "12345678901234",
            vehicleInfo,
            mileage,
            "evaluator-1"
        );
    }
}
