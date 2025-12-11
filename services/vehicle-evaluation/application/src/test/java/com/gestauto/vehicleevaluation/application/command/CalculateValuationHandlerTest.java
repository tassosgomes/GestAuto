package com.gestauto.vehicleevaluation.application.command;

import com.gestauto.vehicleevaluation.application.dto.ValuationResultDto;
import com.gestauto.vehicleevaluation.application.service.DomainEventPublisherService;
import com.gestauto.vehicleevaluation.application.service.ValuationConfig;
import com.gestauto.vehicleevaluation.application.service.ValuationService;
import com.gestauto.vehicleevaluation.domain.entity.VehicleEvaluation;
import com.gestauto.vehicleevaluation.domain.enums.EvaluationStatus;
import com.gestauto.vehicleevaluation.domain.enums.FuelType;
import com.gestauto.vehicleevaluation.domain.event.ValuationCalculatedEvent;
import com.gestauto.vehicleevaluation.domain.repository.VehicleEvaluationRepository;
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
import static org.mockito.ArgumentMatchers.any;
import static org.mockito.ArgumentMatchers.anyString;
import static org.mockito.Mockito.*;

/**
 * Testes unitários para CalculateValuationHandler.
 */
@ExtendWith(MockitoExtension.class)
class CalculateValuationHandlerTest {

    @Mock
    private VehicleEvaluationRepository evaluationRepository;

    @Mock
    private ValuationService valuationService;

    @Mock
    private DomainEventPublisherService eventPublisher;

    private CalculateValuationHandler handler;

    @BeforeEach
    void setUp() {
        handler = new CalculateValuationHandler(evaluationRepository, valuationService, eventPublisher);
    }

    @Test
    void shouldCalculateValuationSuccessfully() {
        // Arrange
        String evaluationId = "eval-123";
        VehicleEvaluation evaluation = createTestEvaluation(evaluationId);
        ValuationResultDto expectedResult = createTestValuationResult(evaluationId);

        when(evaluationRepository.findById(any(EvaluationId.class)))
            .thenReturn(Optional.of(evaluation));
        when(valuationService.calculateValuation(eq(evaluation), any(ValuationConfig.class)))
            .thenReturn(expectedResult);
        when(evaluationRepository.save(any(VehicleEvaluation.class)))
            .thenReturn(evaluation);

        CalculateValuationCommand command = CalculateValuationCommand.withoutManualAdjustment(evaluationId);

        // Act
        ValuationResultDto result = handler.handle(command);

        // Assert
        assertNotNull(result);
        assertEquals(expectedResult.getEvaluationId(), result.getEvaluationId());
        assertEquals(expectedResult.getSuggestedValue(), result.getSuggestedValue());
        verify(evaluationRepository, times(1)).save(any(VehicleEvaluation.class));
        verify(eventPublisher, times(1)).publish(any(ValuationCalculatedEvent.class));
    }

    @Test
    void shouldThrowExceptionWhenEvaluationNotFound() {
        // Arrange
        String evaluationId = "eval-nonexistent";
        when(evaluationRepository.findById(any(EvaluationId.class)))
            .thenReturn(Optional.empty());

        CalculateValuationCommand command = CalculateValuationCommand.withoutManualAdjustment(evaluationId);

        // Act & Assert
        assertThrows(IllegalArgumentException.class, () -> handler.handle(command));
        verify(evaluationRepository, never()).save(any());
        verify(eventPublisher, never()).publish(any());
    }

    @Test
    void shouldThrowExceptionWhenEvaluationInInvalidStatus() {
        // Arrange
        String evaluationId = "eval-123";
        VehicleEvaluation evaluation = createTestEvaluation(evaluationId);
        evaluation.approve("approver-1");

        when(evaluationRepository.findById(any(EvaluationId.class)))
            .thenReturn(Optional.of(evaluation));

        CalculateValuationCommand command = CalculateValuationCommand.withoutManualAdjustment(evaluationId);

        // Act & Assert
        assertThrows(IllegalArgumentException.class, () -> handler.handle(command));
        verify(evaluationRepository, never()).save(any());
        verify(eventPublisher, never()).publish(any());
    }

    @Test
    void shouldCalculateValuationWithManualAdjustment() {
        // Arrange
        String evaluationId = "eval-123";
        Double adjustmentPercentage = 5.0;
        VehicleEvaluation evaluation = createTestEvaluation(evaluationId);
        ValuationResultDto expectedResult = createTestValuationResult(evaluationId);

        when(evaluationRepository.findById(any(EvaluationId.class)))
            .thenReturn(Optional.of(evaluation));
        when(valuationService.calculateValuationWithManualAdjustment(
            eq(evaluation),
            any(ValuationConfig.class),
            eq(adjustmentPercentage)
        )).thenReturn(expectedResult);
        when(evaluationRepository.save(any(VehicleEvaluation.class)))
            .thenReturn(evaluation);

        CalculateValuationCommand command = new CalculateValuationCommand(evaluationId, adjustmentPercentage);

        // Act
        ValuationResultDto result = handler.handle(command);

        // Assert
        assertNotNull(result);
        verify(evaluationRepository, times(1)).save(any(VehicleEvaluation.class));
    }

    @Test
    void shouldUpdateEvaluationWithCalculatedValues() {
        // Arrange
        String evaluationId = "eval-123";
        VehicleEvaluation evaluation = createTestEvaluation(evaluationId);
        ValuationResultDto expectedResult = createTestValuationResult(evaluationId);

        when(evaluationRepository.findById(any(EvaluationId.class)))
            .thenReturn(Optional.of(evaluation));
        when(valuationService.calculateValuation(eq(evaluation), any(ValuationConfig.class)))
            .thenReturn(expectedResult);
        when(evaluationRepository.save(any(VehicleEvaluation.class)))
            .thenReturn(evaluation);

        CalculateValuationCommand command = CalculateValuationCommand.withoutManualAdjustment(evaluationId);

        // Act
        handler.handle(command);

        // Assert
        verify(evaluationRepository, times(1)).save(argThat(savedEval ->
            savedEval.getFipePrice() != null &&
            savedEval.getBaseValue() != null &&
            savedEval.getFinalValue() != null
        ));
    }

    // Helper methods
    private VehicleEvaluation createTestEvaluation(String evaluationId) {
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

        VehicleEvaluation evaluation = VehicleEvaluation.create(
            plate,
            "12345678901234",
            vehicleInfo,
            mileage,
            "evaluator-1"
        );

        // Return evaluation with mocked ID (would normally be generated)
        return evaluation;
    }

    private ValuationResultDto createTestValuationResult(String evaluationId) {
        return new ValuationResultDto.Builder()
            .evaluationId(evaluationId)
            .fipePrice(Money.of(BigDecimal.valueOf(100000)))
            .totalDepreciation(Money.of(BigDecimal.valueOf(5000)))
            .safetyMargin(Money.of(BigDecimal.valueOf(10000)))
            .profitMargin(Money.of(BigDecimal.valueOf(15000)))
            .suggestedValue(Money.of(BigDecimal.valueOf(120000)))
            .depreciationDetails(java.util.List.of())
            .liquidityPercentage(0.85)
            .finalValue(Money.of(BigDecimal.valueOf(120000)))
            .build();
    }
}
