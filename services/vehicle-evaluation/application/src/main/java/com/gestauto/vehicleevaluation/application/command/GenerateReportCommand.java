package com.gestauto.vehicleevaluation.application.command;

import io.swagger.v3.oas.annotations.media.Schema;

import java.util.UUID;

/**
 * Comando para gerar relatório PDF de avaliação.
 * 
 * Contém os parâmetros necessários para gerar um laudo completo
 * em PDF com todas as informações da avaliação.
 */
@Schema(description = "Comando para gerar relatório PDF de avaliação")
public record GenerateReportCommand(

    @Schema(description = "ID da avaliação para gerar o relatório")
    UUID evaluationId

) {

    public GenerateReportCommand {
        if (evaluationId == null) {
            throw new IllegalArgumentException("EvaluationId cannot be null");
        }
    }
}
