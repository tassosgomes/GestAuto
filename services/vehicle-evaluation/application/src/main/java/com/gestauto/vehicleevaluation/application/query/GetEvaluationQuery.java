package com.gestauto.vehicleevaluation.application.query;

import io.swagger.v3.oas.annotations.media.Schema;

import java.util.UUID;

/**
 * Query para buscar uma avaliação específica por ID.
 */
@Schema(description = "Consulta para buscar avaliação por ID")
public record GetEvaluationQuery(

    @Schema(description = "ID da avaliação", example = "123e4567-e89b-12d3-a456-426614174000")
    UUID evaluationId

) {}