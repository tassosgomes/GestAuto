package com.gestauto.vehicleevaluation.application.command;

import io.swagger.v3.oas.annotations.media.Schema;

import java.math.BigDecimal;
import java.util.UUID;

/**
 * Comando para aprovar uma avaliação de veículo.
 */
@Schema(description = "Comando para aprovar avaliação")
public record ApproveEvaluationCommand(

    @Schema(description = "ID da avaliação a ser aprovada")
    UUID evaluationId,

    @Schema(description = "Valor ajustado (opcional, para aprovação parcial)")
    BigDecimal adjustedValue

) {

    public ApproveEvaluationCommand {
        if (evaluationId == null) {
            throw new IllegalArgumentException("EvaluationId cannot be null");
        }
        if (adjustedValue != null && adjustedValue.compareTo(BigDecimal.ZERO) <= 0) {
            throw new IllegalArgumentException("Adjusted value must be positive");
        }
    }
}