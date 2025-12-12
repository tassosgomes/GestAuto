package com.gestauto.vehicleevaluation.application.command;

import com.gestauto.vehicleevaluation.domain.entity.VehicleEvaluation;
import com.gestauto.vehicleevaluation.domain.exception.EvaluationNotFoundException;
import com.gestauto.vehicleevaluation.domain.exception.DomainException;
import com.gestauto.vehicleevaluation.domain.repository.VehicleEvaluationRepository;
import com.gestauto.vehicleevaluation.domain.service.NotificationService;
import com.gestauto.vehicleevaluation.domain.value.EvaluationId;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.stereotype.Component;
import org.springframework.transaction.annotation.Transactional;

/**
 * Handler responsável por rejeitar avaliações de veículo.
 *
 * Este handler implementa a rejeição com justificativa obrigatória
 * e envia notificações ao avaliador.
 */
@Component
@RequiredArgsConstructor
@Slf4j
public class RejectEvaluationHandler implements CommandHandler<RejectEvaluationCommand, Void> {

    private final VehicleEvaluationRepository evaluationRepository;
    private final NotificationService notificationService;

    @Override
    @Transactional
    public Void handle(RejectEvaluationCommand command) {
        log.info("Rejeitando avaliação: evaluationId={}, reason={}",
                command.evaluationId(), command.reason());

        try {
            // 1. Buscar avaliação
            VehicleEvaluation evaluation = evaluationRepository.findById(
                EvaluationId.from(command.evaluationId().toString())
            ).orElseThrow(() -> new EvaluationNotFoundException(command.evaluationId().toString()));

            // 2. Validar status
            if (!evaluation.getStatus().canBeRejected()) {
                throw new DomainException("Evaluation is not pending approval");
            }

            // 3. Obter reviewer do contexto de segurança
            String reviewerId = getCurrentReviewerId();

            // 4. Rejeitar avaliação
            evaluation.reject(reviewerId, command.reason());

            // 5. Salvar
            evaluationRepository.save(evaluation);

            // 6. Enviar notificações
            notificationService.notifyEvaluator(evaluation.getEvaluatorId(),
                "Evaluation rejected",
                String.format("Your evaluation has been rejected. Reason: %s", command.reason()));

            // 7. Publicar eventos (se houver event publisher)
            // eventPublisher.publishEvent(new EvaluationRejectedEvent(evaluation.getId(), ...));

            log.info("Avaliação rejeitada com sucesso: evaluationId={}", command.evaluationId());

            return null;

        } catch (Exception e) {
            log.error("Erro ao rejeitar avaliação: {}", command, e);
            throw e;
        }
    }

    /**
     * Obtém o ID do reviewer atual do contexto de segurança.
     *
     * @return ID do reviewer
     */
    private String getCurrentReviewerId() {
        // TODO: implementar obtenção do usuário atual via Spring Security
        return "current-user-id"; // Mock
    }
}