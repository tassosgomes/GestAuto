package com.gestauto.vehicleevaluation.application.dto;

import io.swagger.v3.oas.annotations.media.Schema;
import jakarta.validation.constraints.Max;
import jakarta.validation.constraints.Min;
import jakarta.validation.constraints.Pattern;

@Schema(description = "DTO da seção de lataria e pintura do checklist")
public record BodyworkDto(

    @Schema(description = "Condição da lataria", example = "GOOD")
    @Pattern(regexp = "EXCELLENT|GOOD|FAIR|POOR", message = "Must be EXCELLENT, GOOD, FAIR, or POOR")
    String bodyCondition,

    @Schema(description = "Condição da pintura", example = "GOOD")
    @Pattern(regexp = "EXCELLENT|GOOD|FAIR|POOR", message = "Must be EXCELLENT, GOOD, FAIR, or POOR")
    String paintCondition,

    @Schema(description = "Presença de ferrugem")
    Boolean rustPresence,

    @Schema(description = "Arranhões leves")
    Boolean lightScratches,

    @Schema(description = "Arranhões profundos")
    Boolean deepScratches,

    @Schema(description = "Amassados pequenos")
    Boolean smallDents,

    @Schema(description = "Amassados grandes")
    Boolean largeDents,

    @Schema(description = "Número de reparos em portas", example = "0")
    @Min(value = 0, message = "Must be between 0 and 10")
    @Max(value = 10, message = "Must be between 0 and 10")
    Integer doorRepairs,

    @Schema(description = "Número de reparos em para-lamas", example = "0")
    @Min(value = 0, message = "Must be between 0 and 10")
    @Max(value = 10, message = "Must be between 0 and 10")
    Integer fenderRepairs,

    @Schema(description = "Número de reparos no capô", example = "0")
    @Min(value = 0, message = "Must be between 0 and 10")
    @Max(value = 10, message = "Must be between 0 and 10")
    Integer hoodRepairs,

    @Schema(description = "Número de reparos no porta-malas", example = "0")
    @Min(value = 0, message = "Must be between 0 and 10")
    @Max(value = 10, message = "Must be between 0 and 10")
    Integer trunkRepairs,

    @Schema(description = "Funilaria pesada realizada")
    Boolean heavyBodywork,

    @Schema(description = "Observações sobre lataria e pintura")
    String observations

) {}
