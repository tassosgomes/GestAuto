package com.gestauto.vehicleevaluation.application.dto;

import io.swagger.v3.oas.annotations.media.Schema;

import java.util.List;

@Schema(description = "DTO de uma seção do checklist")
public record ChecklistSectionDto(

    @Schema(description = "Nome da seção")
    String sectionName,

    @Schema(description = "Itens da seção")
    List<ChecklistItemDto> items,

    @Schema(description = "Seção está completa?")
    boolean complete,

    @Schema(description = "Percentual de conclusão da seção")
    double completionPercentage

) {}