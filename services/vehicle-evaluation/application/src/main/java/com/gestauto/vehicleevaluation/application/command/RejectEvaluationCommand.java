package com.gestauto.vehicleevaluation.application.command;

import io.swagger.v3.oas.annotations.media.Schema;

import java.util.UUID;

/**
 * Comando para rejeitar uma avaliação de veículo.
 */
@Schema(description = "Comando para rejeitar avaliação")
public record RejectEvaluationCommand(

    @Schema(description = "ID da avaliação a ser rejeitada")
    UUID evaluationId,

    @Schema(description = "Motivo da rejeição")
    String reason

) {

    public RejectEvaluationCommand {
        if (evaluationId == null) {
            throw new IllegalArgumentException("EvaluationId cannot be null");
        }
        if (reason == null || reason.trim().isEmpty()) {
            throw new IllegalArgumentException("Rejection reason cannot be null or empty");
        }
        if (reason.length() > 500) {
            throw new IllegalArgumentException("Rejection reason cannot exceed 500 characters");
        }
    }
}