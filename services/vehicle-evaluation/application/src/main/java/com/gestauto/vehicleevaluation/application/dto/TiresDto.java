package com.gestauto.vehicleevaluation.application.dto;

import io.swagger.v3.oas.annotations.media.Schema;
import jakarta.validation.constraints.Pattern;

@Schema(description = "DTO da seção de pneus do checklist")
public record TiresDto(

    @Schema(description = "Condição dos pneus", example = "GOOD")
    @Pattern(regexp = "EXCELLENT|GOOD|FAIR|POOR", message = "Must be EXCELLENT, GOOD, FAIR, or POOR")
    String tiresCondition,

    @Schema(description = "Desgaste irregular detectado")
    Boolean unevenWear,

    @Schema(description = "Banda de rodagem baixa")
    Boolean lowTread,

    @Schema(description = "Observações sobre pneus")
    String observations

) {}
