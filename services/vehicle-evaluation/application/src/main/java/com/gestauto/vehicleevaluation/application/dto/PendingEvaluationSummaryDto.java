package com.gestauto.vehicleevaluation.application.dto;

import io.swagger.v3.oas.annotations.media.Schema;

import java.math.BigDecimal;
import java.time.LocalDateTime;
import java.util.UUID;

/**
 * DTO para resumo de avaliação pendente de aprovação.
 * Usado no dashboard de pendências gerenciais.
 */
@Schema(description = "DTO resumido de avaliação pendente de aprovação")
public record PendingEvaluationSummaryDto(

    @Schema(description = "ID da avaliação")
    UUID id,

    @Schema(description = "Placa do veículo")
    String plate,

    @Schema(description = "Informações do veículo (marca/modelo/ano)")
    String vehicleInfo,

    @Schema(description = "Valor sugerido da avaliação")
    BigDecimal suggestedValue,

    @Schema(description = "Data de criação da avaliação")
    LocalDateTime createdAt,

    @Schema(description = "Nome do avaliador")
    String evaluatorName,

    @Schema(description = "Dias pendente de aprovação")
    Integer daysPending,

    @Schema(description = "Quantidade de fotos anexadas")
    Integer photoCount,

    @Schema(description = "Possui problemas críticos?")
    Boolean hasCriticalIssues

) {}