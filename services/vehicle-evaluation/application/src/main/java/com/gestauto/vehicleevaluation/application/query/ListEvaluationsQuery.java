package com.gestauto.vehicleevaluation.application.query;

import com.gestauto.vehicleevaluation.domain.enums.EvaluationStatus;
import io.swagger.v3.oas.annotations.media.Schema;

import java.util.UUID;

/**
 * Query para listar avaliações com filtros e paginação.
 */
@Schema(description = "Consulta para listar avaliações com filtros e paginação")
public record ListEvaluationsQuery(

    @Schema(description = "ID do avaliador para filtro")
    UUID evaluatorId,

    @Schema(description = "Status da avaliação para filtro")
    EvaluationStatus status,

    @Schema(description = "Placa do veículo para filtro (parcial)")
    String plate,

    @Schema(description = "Número da página (base 0)", example = "0")
    Integer page,

    @Schema(description = "Tamanho da página", example = "20")
    Integer size,

    @Schema(description = "Campo de ordenação", example = "createdAt")
    String sortBy,

    @Schema(description = "Direção da ordenação", example = "DESC")
    String sortDirection

) {

    public ListEvaluationsQuery {
        // Valores padrão
        if (page == null) page = 0;
        if (size == null) size = 20;
        if (size > 100) size = 100; // Limite máximo
        if (page < 0) page = 0;
        if (sortBy == null) sortBy = "createdAt";
        if (sortDirection == null) sortDirection = "DESC";
    }
}