package com.gestauto.vehicleevaluation.application.dto;

import com.fasterxml.jackson.annotation.JsonFormat;
import io.swagger.v3.oas.annotations.media.Schema;

import java.math.BigDecimal;
import java.time.LocalDateTime;
import java.util.UUID;

@Schema(description = "DTO resumido de uma avaliação de veículo para listagens")
public record VehicleEvaluationSummaryDto(

    @Schema(description = "ID da avaliação")
    UUID id,

    @Schema(description = "Placa do veículo")
    String plate,

    @Schema(description = "Marca do veículo")
    String brand,

    @Schema(description = "Modelo do veículo")
    String model,

    @Schema(description = "Ano do veículo")
    Integer year,

    @Schema(description = "Quilometragem do veículo")
    Integer mileage,

    @Schema(description = "Status da avaliação")
    String status,

    @Schema(description = "Valor final calculado")
    BigDecimal finalValue,

    @Schema(description = "ID do avaliador")
    String evaluatorId,

    @Schema(description = "ID do aprovador (se aplicável)")
    String approverId,

    @Schema(description = "Data de criação")
    @JsonFormat(pattern = "yyyy-MM-dd'T'HH:mm:ss")
    LocalDateTime createdAt,

    @Schema(description = "Data da última atualização")
    @JsonFormat(pattern = "yyyy-MM-dd'T'HH:mm:ss")
    LocalDateTime updatedAt,

    @Schema(description = "Avaliação está expirada?")
    boolean expired

) {}