package com.gestauto.vehicleevaluation.application.dto;

import io.swagger.v3.oas.annotations.media.Schema;
import jakarta.validation.constraints.Pattern;

@Schema(description = "DTO da seção de interior do checklist")
public record InteriorDto(

    @Schema(description = "Condição dos bancos", example = "GOOD")
    @Pattern(regexp = "EXCELLENT|GOOD|FAIR|POOR", message = "Must be EXCELLENT, GOOD, FAIR, or POOR")
    String seatsCondition,

    @Schema(description = "Condição do painel", example = "GOOD")
    @Pattern(regexp = "EXCELLENT|GOOD|FAIR|POOR", message = "Must be EXCELLENT, GOOD, FAIR, or POOR")
    String dashboardCondition,

    @Schema(description = "Condição da parte elétrica", example = "GOOD")
    @Pattern(regexp = "EXCELLENT|GOOD|FAIR|POOR", message = "Must be EXCELLENT, GOOD, FAIR, or POOR")
    String electronicsCondition,

    @Schema(description = "Danos nos bancos")
    Boolean seatDamage,

    @Schema(description = "Danos nos forros das portas")
    Boolean doorPanelDamage,

    @Schema(description = "Desgaste no volante")
    Boolean steeringWheelWear,

    @Schema(description = "Observações sobre o interior")
    String observations

) {}
