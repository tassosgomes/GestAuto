package com.gestauto.vehicleevaluation.application.command;

import com.gestauto.vehicleevaluation.domain.entity.VehicleEvaluation;
import com.gestauto.vehicleevaluation.domain.enums.EvaluationStatus;
import com.gestauto.vehicleevaluation.domain.exception.EvaluationNotFoundException;
import com.gestauto.vehicleevaluation.domain.repository.VehicleEvaluationRepository;
import com.gestauto.vehicleevaluation.domain.service.ReportService;
import com.gestauto.vehicleevaluation.domain.value.EvaluationId;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.stereotype.Component;
import org.springframework.transaction.annotation.Transactional;

import java.time.LocalDateTime;

/**
 * Handler responsável por gerar relatórios PDF de avaliações.
 *
 * Este handler implementa a geração de laudos completos com:
 * - Informações do veículo
 * - Todas as 15 fotos organizadas
 * - Checklist técnico
 * - Cálculo de valoração
 * - QR code para validação online
 * - Marca d'água dinâmica
 * - Performance otimizada < 30 segundos
 */
@Component
@RequiredArgsConstructor
@Slf4j
public class GenerateReportHandler implements CommandHandler<GenerateReportCommand, byte[]> {

    private final VehicleEvaluationRepository evaluationRepository;
    private final ReportService reportService;

    @Override
    @Transactional(readOnly = true)
    public byte[] handle(GenerateReportCommand command) {
        log.info("Gerando relatório para avaliação: evaluationId={}", command.evaluationId());

        try {
            // 1. Buscar avaliação
            VehicleEvaluation evaluation = evaluationRepository.findById(
                EvaluationId.from(command.evaluationId().toString())
            ).orElseThrow(() -> new EvaluationNotFoundException(
                command.evaluationId().toString()
            ));

            // 2. Validar status - não permitir relatórios para avaliações em DRAFT
            if (evaluation.getStatus() == EvaluationStatus.DRAFT) {
                throw new IllegalStateException(
                    "Cannot generate report for evaluation in DRAFT status. " +
                    "Evaluation must be submitted for approval first."
                );
            }

            // 3. Validar validade do laudo (72 horas)
            validateReportValidity(evaluation);

            // 4. Validar se tem dados mínimos
            validateEvaluationData(evaluation);

            // 5. Gerar PDF
            byte[] report = reportService.generateEvaluationReport(evaluation);

            log.info("Relatório gerado com sucesso. Tamanho: {} bytes", report.length);

            return report;
        } catch (IllegalStateException e) {
            log.warn("Erro ao validar avaliação: {}", e.getMessage());
            throw e;
        } catch (Exception e) {
            log.error("Erro ao gerar relatório para avaliação: {}", command.evaluationId(), e);
            throw new RuntimeException("Falha ao gerar relatório", e);
        }
    }

    /**
     * Valida se o laudo ainda está dentro do prazo de validade de 72 horas.
     *
     * @param evaluation avaliação a ser validada
     * @throws IllegalStateException se o laudo estiver expirado
     */
    private void validateReportValidity(VehicleEvaluation evaluation) {
        if (evaluation.getValidUntil() != null) {
            LocalDateTime now = LocalDateTime.now();
            if (evaluation.getValidUntil().isBefore(now)) {
                log.warn("Laudo expirado para avaliação: {}. Válido até: {}, Agora: {}",
                        evaluation.getId(), evaluation.getValidUntil(), now);
                throw new IllegalStateException(
                    String.format("Report validation expired. Valid until: %s, Current time: %s",
                        evaluation.getValidUntil(), now)
                );
            }
            log.debug("Laudo válido até: {}", evaluation.getValidUntil());
        }
    }

    /**
     * Valida se a avaliação tem os dados mínimos para gerar um relatório.
     *
     * @param evaluation avaliação a ser validada
     * @throws IllegalStateException se dados essenciais estiverem faltando
     */
    private void validateEvaluationData(VehicleEvaluation evaluation) {
        // Verificar fotos
        if (evaluation.getPhotos().isEmpty()) {
            log.warn("Avaliação sem fotos: {}", evaluation.getId());
            // Não bloqueia a geração do PDF, mas avisa
        }

        // Verificar checklist
        if (evaluation.getChecklist() == null) {
            log.warn("Avaliação sem checklist: {}", evaluation.getId());
            // Não bloqueia a geração do PDF, mas avisa
        }

        // Verificar valores - pelo menos um deve estar preenchido
        if (evaluation.getFipePrice() == null && evaluation.getFinalValue() == null) {
            throw new IllegalStateException(
                "Evaluation must have at least FIPE price or final value calculated"
            );
        }
    }
}
