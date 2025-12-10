package com.gestauto.vehicleevaluation.application.dto;

import io.swagger.v3.oas.annotations.media.Schema;

import java.time.LocalDateTime;
import java.util.UUID;

@Schema(description = "DTO de uma foto da avaliação")
public record EvaluationPhotoDto(

    @Schema(description = "ID da foto")
    UUID photoId,

    @Schema(description = "Tipo da foto")
    String photoType,

    @Schema(description = "URL da foto no Cloudflare R2")
    String url,

    @Schema(description = "Nome do arquivo")
    String filename,

    @Schema(description = "Tamanho do arquivo em bytes")
    Long fileSize,

    @Schema(description = "Data de upload")
    LocalDateTime uploadedAt,

    @Schema(description = "URL temporária para upload")
    String uploadUrl,

    @Schema(description = "Validade da URL de upload")
    LocalDateTime uploadUrlExpiresAt

) {}