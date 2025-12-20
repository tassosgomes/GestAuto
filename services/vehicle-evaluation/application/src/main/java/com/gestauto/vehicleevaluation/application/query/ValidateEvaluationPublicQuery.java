package com.gestauto.vehicleevaluation.application.query;

import io.swagger.v3.oas.annotations.media.Schema;

@Schema(description = "Consulta para validação pública de laudo via token")
public record ValidateEvaluationPublicQuery(

    @Schema(description = "Token de validação")
    String token

) {
}
