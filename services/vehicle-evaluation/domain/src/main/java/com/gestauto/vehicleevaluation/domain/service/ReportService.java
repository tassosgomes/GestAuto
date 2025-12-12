package com.gestauto.vehicleevaluation.domain.service;

/**
 * Serviço para geração de relatórios e laudos de avaliação.
 */
public interface ReportService {

    /**
     * Gera relatório PDF da avaliação.
     *
     * @param evaluation avaliação para gerar relatório
     * @return bytes do PDF gerado
     */
    byte[] generateEvaluationReport(com.gestauto.vehicleevaluation.domain.entity.VehicleEvaluation evaluation);

    /**
     * Gera URL de validação para o laudo.
     *
     * @param evaluationId ID da avaliação
     * @param accessToken token de acesso
     * @return URL de validação
     */
    String getValidationUrl(java.util.UUID evaluationId, String accessToken);
}