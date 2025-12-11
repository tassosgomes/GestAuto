package com.gestauto.vehicleevaluation.application.dto;

import io.swagger.v3.oas.annotations.media.Schema;
import jakarta.validation.constraints.NotNull;

@Schema(description = "DTO da seção de documentação do checklist")
public record DocumentsDto(

    @Schema(description = "CRLV presente", required = true)
    @NotNull(message = "CRLV presence is required")
    Boolean crvlPresent,

    @Schema(description = "Manual do proprietário presente")
    Boolean manualPresent,

    @Schema(description = "Chave reserva presente")
    Boolean spareKeyPresent,

    @Schema(description = "Histórico de manutenções presente")
    Boolean maintenanceRecords,

    @Schema(description = "Observações sobre documentação")
    String observations

) {}
