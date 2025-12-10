package com.gestauto.vehicleevaluation.application.dto;

import io.swagger.v3.oas.annotations.media.Schema;

import java.math.BigDecimal;
import java.util.UUID;

@Schema(description = "DTO de um item de depreciação")
public record DepreciationItemDto(

    @Schema(description = "ID do item de depreciação")
    UUID id,

    @Schema(description = "Categoria da depreciação")
    String category,

    @Schema(description = "Descrição da depreciação")
    String description,

    @Schema(description = "Valor da depreciação")
    BigDecimal depreciationValue,

    @Schema(description = "Percentual da depreciação")
    BigDecimal depreciationPercentage,

    @Schema(description = "Observações sobre a depreciação")
    String observations

) {}