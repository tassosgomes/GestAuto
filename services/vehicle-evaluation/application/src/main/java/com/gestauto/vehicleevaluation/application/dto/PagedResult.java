package com.gestauto.vehicleevaluation.application.dto;

import io.swagger.v3.oas.annotations.media.Schema;

import java.util.List;

@Schema(description = "Resultado paginado de uma consulta")
public record PagedResult<T>(

    @Schema(description = "Lista de itens na página atual")
    List<T> content,

    @Schema(description = "Número da página atual (base 0)")
    int page,

    @Schema(description = "Número de itens por página")
    int size,

    @Schema(description = "Total de elementos disponíveis")
    long totalElements,

    @Schema(description = "Total de páginas disponíveis")
    int totalPages,

    @Schema(description = "Indica se existe página anterior")
    boolean first,

    @Schema(description = "Indica se existe próxima página")
    boolean last,

    @Schema(description = "Número de elementos na página atual")
    int numberOfElements

) {}