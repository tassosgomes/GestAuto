package com.gestauto.vehicleevaluation.api.controller;

import com.gestauto.vehicleevaluation.application.dto.EvaluationValidationDto;
import com.gestauto.vehicleevaluation.application.query.ValidateEvaluationPublicHandler;
import java.math.BigDecimal;
import java.time.LocalDateTime;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;

import static org.mockito.ArgumentMatchers.any;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;
import static org.assertj.core.api.Assertions.assertThat;

class PublicValidationControllerTest {

    ValidateEvaluationPublicHandler validateHandler;

    PublicValidationController controller;

    @BeforeEach
    void setUp() {
        validateHandler = mock(ValidateEvaluationPublicHandler.class);
        controller = new PublicValidationController(validateHandler);
    }

    @Test
    void validateReturns404WhenTokenNotFound() throws Exception {
        when(validateHandler.handle(any())).thenReturn(null);

        var response = controller.validate("missing-token");
        assertThat(response.getStatusCode().value()).isEqualTo(404);
    }

    @Test
    void validateReturns200WhenTokenIsValid() throws Exception {
        when(validateHandler.handle(any())).thenReturn(new EvaluationValidationDto(
            "ABC1234",
            "Volkswagen",
            "Gol",
            2022,
            "APPROVED",
            new BigDecimal("45000.00"),
            LocalDateTime.now(),
            LocalDateTime.now().plusHours(24)
        ));

        var response = controller.validate("token-123");
        assertThat(response.getStatusCode().value()).isEqualTo(200);
        assertThat(response.getBody()).isNotNull();
        assertThat(response.getBody().plate()).isEqualTo("ABC1234");
    }
}
