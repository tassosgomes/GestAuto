package com.gestauto.vehicleevaluation.application.dto;

import io.swagger.v3.oas.annotations.media.Schema;
import jakarta.validation.constraints.Pattern;

@Schema(description = "DTO da seção mecânica do checklist")
public record MechanicalDto(

    @Schema(description = "Condição do motor", example = "GOOD")
    @Pattern(regexp = "EXCELLENT|GOOD|FAIR|POOR", message = "Must be EXCELLENT, GOOD, FAIR, or POOR")
    String engineCondition,

    @Schema(description = "Condição da transmissão", example = "GOOD")
    @Pattern(regexp = "EXCELLENT|GOOD|FAIR|POOR", message = "Must be EXCELLENT, GOOD, FAIR, or POOR")
    String transmissionCondition,

    @Schema(description = "Condição da suspensão", example = "GOOD")
    @Pattern(regexp = "EXCELLENT|GOOD|FAIR|POOR", message = "Must be EXCELLENT, GOOD, FAIR, or POOR")
    String suspensionCondition,

    @Schema(description = "Condição dos freios", example = "GOOD")
    @Pattern(regexp = "EXCELLENT|GOOD|FAIR|POOR", message = "Must be EXCELLENT, GOOD, FAIR, or POOR")
    String brakeCondition,

    @Schema(description = "Vazamentos de óleo detectados")
    Boolean oilLeaks,

    @Schema(description = "Vazamentos de água detectados")
    Boolean waterLeaks,

    @Schema(description = "Correia dentada em bom estado")
    Boolean timingBelt,

    @Schema(description = "Condição da bateria", example = "GOOD")
    @Pattern(regexp = "EXCELLENT|GOOD|FAIR|POOR", message = "Must be EXCELLENT, GOOD, FAIR, or POOR")
    String batteryCondition,

    @Schema(description = "Observações mecânicas")
    String observations

) {}
