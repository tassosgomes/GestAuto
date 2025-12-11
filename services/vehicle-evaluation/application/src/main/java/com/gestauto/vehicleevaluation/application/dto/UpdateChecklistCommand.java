package com.gestauto.vehicleevaluation.application.dto;

import io.swagger.v3.oas.annotations.media.Schema;
import jakarta.validation.Valid;
import jakarta.validation.constraints.NotNull;

import java.util.UUID;

@Schema(description = "Command para atualizar o checklist técnico de uma avaliação")
public record UpdateChecklistCommand(

    @Schema(description = "ID da avaliação", required = true)
    @NotNull(message = "Evaluation ID is required")
    UUID evaluationId,

    @Schema(description = "Seção de lataria e pintura")
    @Valid
    BodyworkDto bodywork,

    @Schema(description = "Seção mecânica")
    @Valid
    MechanicalDto mechanical,

    @Schema(description = "Seção de pneus")
    @Valid
    TiresDto tires,

    @Schema(description = "Seção de interior")
    @Valid
    InteriorDto interior,

    @Schema(description = "Seção de documentação", required = true)
    @NotNull(message = "Documents section is required")
    @Valid
    DocumentsDto documents

) {}
