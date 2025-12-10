package com.gestauto.vehicleevaluation.application.dto;

import io.swagger.v3.oas.annotations.media.Schema;

@Schema(description = "DTO de um item do checklist")
public record ChecklistItemDto(

    @Schema(description = "ID do item")
    String id,

    @Schema(description = "Descrição do item")
    String description,

    @Schema(description = "Item está verificado?")
    boolean checked,

    @Schema(description = "Item é crítico?")
    boolean critical,

    @Schema(description = "Observações sobre o item")
    String observations,

    @Schema(description = "Status do item: OK, PENDENTE, IRREGULAR")
    String status

) {}