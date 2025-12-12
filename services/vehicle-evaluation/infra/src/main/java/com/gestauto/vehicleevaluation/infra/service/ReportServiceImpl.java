package com.gestauto.vehicleevaluation.infra.service;

import com.gestauto.vehicleevaluation.domain.entity.VehicleEvaluation;
import com.gestauto.vehicleevaluation.domain.service.ReportService;
import lombok.extern.slf4j.Slf4j;
import org.springframework.stereotype.Service;

import java.util.UUID;

/**
 * Implementação do serviço de geração de relatórios.
 */
@Service
@Slf4j
public class ReportServiceImpl implements ReportService {

    @Override
    public byte[] generateEvaluationReport(VehicleEvaluation evaluation) {
        log.info("Gerando relatório PDF para avaliação: {}", evaluation.getId());

        // TODO: implementar geração real de PDF
        // Por enquanto, retorna array vazio
        return new byte[0];
    }

    @Override
    public String getValidationUrl(UUID evaluationId, String accessToken) {
        // TODO: implementar geração de URL de validação
        return String.format("https://gestauto.com/validate/%s?token=%s", evaluationId, accessToken);
    }
}