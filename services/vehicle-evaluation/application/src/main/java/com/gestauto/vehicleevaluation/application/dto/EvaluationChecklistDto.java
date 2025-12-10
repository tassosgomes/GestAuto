package com.gestauto.vehicleevaluation.application.dto;

import io.swagger.v3.oas.annotations.media.Schema;

import java.util.List;
import java.util.UUID;

@Schema(description = "DTO do checklist técnico da avaliação")
public record EvaluationChecklistDto(

    @Schema(description = "ID do checklist")
    UUID id,

    @Schema(description = "Itens do checklist por seção")
    List<ChecklistSectionDto> sections,

    @Schema(description = "Checklist está completo?")
    boolean complete,

    @Schema(description = "Total de itens")
    int totalItems,

    @Schema(description = "Itens verificados")
    int checkedItems,

    @Schema(description = "Percentual de conclusão")
    double completionPercentage,

    @Schema(description = "Observações gerais")
    String generalObservations

) {}