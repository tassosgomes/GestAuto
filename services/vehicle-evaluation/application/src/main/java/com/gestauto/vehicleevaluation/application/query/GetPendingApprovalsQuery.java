package com.gestauto.vehicleevaluation.application.query;

import io.swagger.v3.oas.annotations.media.Schema;

/**
 * Query para obter avaliações pendentes de aprovação com paginação e ordenação.
 */
@Schema(description = "Consulta para obter avaliações pendentes de aprovação")
public record GetPendingApprovalsQuery(

    @Schema(description = "Número da página (base 0)", example = "0")
    Integer page,

    @Schema(description = "Tamanho da página", example = "20")
    Integer size,

    @Schema(description = "Campo de ordenação", example = "finalValue")
    String sortBy,

    @Schema(description = "Ordenação decrescente", example = "true")
    Boolean sortDescending

) {

    public GetPendingApprovalsQuery {
        // Valores padrão
        if (page == null) page = 0;
        if (size == null) size = 20;
        if (size > 100) size = 100; // Limite máximo
        if (page < 0) page = 0;
        if (sortBy == null) sortBy = "finalValue";
        if (sortDescending == null) sortDescending = true;
    }
}