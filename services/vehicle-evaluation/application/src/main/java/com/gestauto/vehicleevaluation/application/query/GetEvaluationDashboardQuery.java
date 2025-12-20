package com.gestauto.vehicleevaluation.application.query;

import com.gestauto.vehicleevaluation.domain.enums.EvaluationStatus;
import io.swagger.v3.oas.annotations.media.Schema;

import java.time.LocalDateTime;

@Schema(description = "Consulta para obter dashboard gerencial de avaliações")
public record GetEvaluationDashboardQuery(

    @Schema(description = "Data inicial (ISO-8601)")
    LocalDateTime startDate,

    @Schema(description = "Data final (ISO-8601)")
    LocalDateTime endDate,

    @Schema(description = "ID do avaliador para filtro")
    String evaluatorId,

    @Schema(description = "Status para filtro")
    EvaluationStatus status

) {
}
