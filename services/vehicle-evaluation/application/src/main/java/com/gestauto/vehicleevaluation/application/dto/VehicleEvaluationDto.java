package com.gestauto.vehicleevaluation.application.dto;

import com.fasterxml.jackson.annotation.JsonFormat;
import io.swagger.v3.oas.annotations.media.Schema;

import java.math.BigDecimal;
import java.time.LocalDateTime;
import java.util.List;
import java.util.UUID;

@Schema(description = "DTO completo de uma avaliação de veículo")
public record VehicleEvaluationDto(

    @Schema(description = "ID da avaliação")
    UUID id,

    @Schema(description = "Placa do veículo")
    String plate,

    @Schema(description = "RENAVAM do veículo")
    String renavam,

    @Schema(description = "Marca do veículo")
    String brand,

    @Schema(description = "Modelo do veículo")
    String model,

    @Schema(description = "Ano do veículo")
    Integer year,

    @Schema(description = "Quilometragem do veículo")
    Integer mileage,

    @Schema(description = "Cor do veículo")
    String color,

    @Schema(description = "Versão do veículo")
    String version,

    @Schema(description = "Tipo de combustível")
    String fuelType,

    @Schema(description = "Tipo de câmbio")
    String gearbox,

    @Schema(description = "Lista de acessórios")
    List<String> accessories,

    @Schema(description = "Status da avaliação")
    String status,

    @Schema(description = "Preço FIPE")
    BigDecimal fipePrice,

    @Schema(description = "Valor base calculado")
    BigDecimal baseValue,

    @Schema(description = "Valor final após depreciações")
    BigDecimal finalValue,

    @Schema(description = "Valor aprovado (se aplicável)")
    BigDecimal approvedValue,

    @Schema(description = "Observações")
    String observations,

    @Schema(description = "Justificativa (se rejeitado)")
    String justification,

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

    @Schema(description = "Data de submissão para aprovação")
    @JsonFormat(pattern = "yyyy-MM-dd'T'HH:mm:ss")
    LocalDateTime submittedAt,

    @Schema(description = "Data de aprovação")
    @JsonFormat(pattern = "yyyy-MM-dd'T'HH:mm:ss")
    LocalDateTime approvedAt,

    @Schema(description = "Validade da avaliação (para aprovações)")
    @JsonFormat(pattern = "yyyy-MM-dd'T'HH:mm:ss")
    LocalDateTime validUntil,

    @Schema(description = "Token de validação (para avaliações aprovadas)")
    String validationToken,

    @Schema(description = "Fotos da avaliação")
    List<EvaluationPhotoDto> photos,

    @Schema(description = "Itens de depreciação")
    List<DepreciationItemDto> depreciationItems,

    @Schema(description = "Checklist técnico")
    EvaluationChecklistDto checklist

) {}