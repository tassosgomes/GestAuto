package com.gestauto.vehicleevaluation.application.query;

import com.gestauto.vehicleevaluation.domain.entity.VehicleEvaluation;
import com.gestauto.vehicleevaluation.domain.repository.VehicleEvaluationRepository;
import com.gestauto.vehicleevaluation.domain.value.Money;
import com.gestauto.vehicleevaluation.domain.value.Plate;
import com.gestauto.vehicleevaluation.domain.value.VehicleInfo;
import com.gestauto.vehicleevaluation.domain.enums.FuelType;
import com.gestauto.vehicleevaluation.domain.enums.EvaluationStatus;
import com.gestauto.vehicleevaluation.domain.value.EvaluationId;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.mockito.Mock;
import org.mockito.junit.jupiter.MockitoExtension;

import java.math.BigDecimal;
import java.time.LocalDateTime;
import java.util.List;
import java.util.Optional;

import static org.junit.jupiter.api.Assertions.*;
import static org.mockito.Mockito.when;

@ExtendWith(MockitoExtension.class)
@DisplayName("ValidateEvaluationPublicHandler Tests")
class ValidateEvaluationPublicHandlerTest {

    @Mock
    private VehicleEvaluationRepository evaluationRepository;

    private ValidateEvaluationPublicHandler handler;

    @BeforeEach
    void setUp() {
        handler = new ValidateEvaluationPublicHandler(evaluationRepository);
    }

    @Test
    @DisplayName("should return null when token not found")
    void shouldReturnNullWhenNotFound() {
        when(evaluationRepository.findByValidationToken("missing")).thenReturn(Optional.empty());
        var result = handler.handle(new ValidateEvaluationPublicQuery("missing"));
        assertNull(result);
    }

    @Test
    @DisplayName("should return dto when token is valid")
    void shouldReturnDtoWhenValid() {
        String token = "token-123";
        VehicleEvaluation evaluation = VehicleEvaluation.restore(
            EvaluationId.generate(),
            Plate.from("ABC1234"),
            "12345678901",
            VehicleInfo.create("Toyota", "Corolla", 2023, "White", FuelType.GASOLINE, "X"),
            Money.of(BigDecimal.valueOf(50000)),
            EvaluationStatus.APPROVED,
            Money.of(BigDecimal.valueOf(50000)),
            null,
            Money.of(BigDecimal.valueOf(45000)),
            Money.of(BigDecimal.valueOf(45000)),
            null,
            null,
            LocalDateTime.now().minusDays(1),
            LocalDateTime.now().minusHours(1),
            LocalDateTime.now().minusHours(2),
            LocalDateTime.now().minusHours(1),
            "EVALUATOR-001",
            "APPROVER-001",
            LocalDateTime.now().plusHours(72),
            token,
            List.of(),
            List.of(),
            null
        );
        when(evaluationRepository.findByValidationToken(token)).thenReturn(Optional.of(evaluation));

        var result = handler.handle(new ValidateEvaluationPublicQuery(token));

        assertNotNull(result);
        assertEquals("ABC-1234", result.plate());
        assertEquals("Toyota", result.brand());
        assertEquals("Corolla", result.model());
        assertEquals(2023, result.year());
        assertNotNull(result.validUntil());
    }
}
