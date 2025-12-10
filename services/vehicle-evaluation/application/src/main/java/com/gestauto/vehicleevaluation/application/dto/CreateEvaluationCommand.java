package com.gestauto.vehicleevaluation.application.dto;

import com.fasterxml.jackson.annotation.JsonFormat;
import io.swagger.v3.oas.annotations.media.Schema;

import jakarta.validation.constraints.NotBlank;
import jakarta.validation.constraints.NotNull;
import jakarta.validation.constraints.Min;
import jakarta.validation.constraints.Max;
import jakarta.validation.constraints.Pattern;
import java.time.Year;
import java.util.List;

@Schema(description = "Comando para criar uma nova avaliação de veículo")
public record CreateEvaluationCommand(

    @Schema(description = "Placa do veículo (formato Mercosul ou antigo)", example = "ABC1D34")
    @NotBlank(message = "Placa é obrigatória")
    @Pattern(regexp = "^[A-Z]{3}[0-9][A-Z][0-9]{2}$|^[A-Z]{3}[0-9]{4}$",
             message = "Placa deve estar no formato Mercosul (ABC1D34) ou antigo (ABC1234)")
    String plate,

    @Schema(description = "Ano do veículo", example = "2022")
    @NotNull(message = "Ano é obrigatório")
    @Min(value = 1900, message = "Ano deve ser igual ou superior a 1900")
    @Max(value = 2100, message = "Ano não pode ser superior a 2100")
    Integer year,

    @Schema(description = "Quilometragem do veículo", example = "45000")
    @NotNull(message = "Quilometragem é obrigatória")
    @Min(value = 0, message = "Quilometragem não pode ser negativa")
    Integer mileage,

    @Schema(description = "Cor do veículo", example = "Prata")
    String color,

    @Schema(description = "Versão do veículo", example = "EXL 1.6 Flex")
    String version,

    @Schema(description = "Tipo de combustível", example = "GASOLINE")
    String fuelType,

    @Schema(description = "Tipo de câmbio", example = "MANUAL")
    String gearbox,

    @Schema(description = "Lista de acessórios do veículo")
    List<String> accessories,

    @Schema(description = "Observações internas do avaliador")
    String internalNotes

) {}