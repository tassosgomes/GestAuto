package com.gestauto.vehicleevaluation.application.command;

import com.gestauto.vehicleevaluation.domain.entity.VehicleEvaluation;
import com.gestauto.vehicleevaluation.domain.exception.EvaluationNotFoundException;
import com.gestauto.vehicleevaluation.domain.exception.DomainException;
import com.gestauto.vehicleevaluation.domain.repository.VehicleEvaluationRepository;
import com.gestauto.vehicleevaluation.domain.service.ReportService;
import com.gestauto.vehicleevaluation.domain.service.NotificationService;
import com.gestauto.vehicleevaluation.domain.value.EvaluationId;
import com.gestauto.vehicleevaluation.domain.value.Money;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.context.ApplicationEventPublisher;
import org.springframework.security.core.Authentication;
import org.springframework.security.core.context.SecurityContextHolder;
import org.springframework.stereotype.Component;
import org.springframework.transaction.annotation.Transactional;

import java.math.BigDecimal;

/**
 * Handler responsável por aprovar avaliações de veículo.
 *
 * Este handler implementa a aprovação seguindo regras de negócio,
 * gera laudos PDF e envia notificações.
 */
@Component
@RequiredArgsConstructor
@Slf4j
public class ApproveEvaluationHandler implements CommandHandler<ApproveEvaluationCommand, Void> {

    private final VehicleEvaluationRepository evaluationRepository;
    private final ReportService reportService;
    private final NotificationService notificationService;
    private final ApplicationEventPublisher eventPublisher;

    @Override
    @Transactional
    public Void handle(ApproveEvaluationCommand command) {
        log.info("Aprovando avaliação: evaluationId={}, adjustedValue={}",
                command.evaluationId(), command.adjustedValue());

        try {
            // 1. Buscar avaliação
            VehicleEvaluation evaluation = evaluationRepository.findById(
                EvaluationId.from(command.evaluationId().toString())
            ).orElseThrow(() -> new EvaluationNotFoundException(command.evaluationId().toString()));

            // 2. Validar status
            if (!evaluation.getStatus().canBeApproved()) {
                throw new DomainException("Evaluation is not pending approval");
            }

            // 3. Obter reviewer do contexto de segurança
            String reviewerId = getCurrentReviewerId();

            // 4. Validar ajuste manual se houver
            Money adjustedValue = null;
            if (command.adjustedValue() != null) {
                validateManualAdjustment(evaluation.getFinalValue(), command.adjustedValue());
                adjustedValue = Money.of(command.adjustedValue());
            }

            // 5. Aprovar avaliação
            evaluation.approve(reviewerId, adjustedValue);

            // 6. Gerar laudo PDF
            byte[] report = reportService.generateEvaluationReport(evaluation);

            // 7. Salvar
            evaluationRepository.save(evaluation);

            // 8. Enviar notificações
            notificationService.notifyEvaluator(evaluation.getEvaluatorId(),
                "Evaluation approved", "Your evaluation has been approved");

            // 9. Publicar eventos de domínio
            EvaluationApprovedEvent event = new EvaluationApprovedEvent(
                evaluation.getId().getValueAsString(),
                reviewerId,
                evaluation.getApprovedValue(),
                evaluation.getApprovedAt()
            );
            eventPublisher.publishEvent(event);

            log.info("Avaliação aprovada com sucesso: evaluationId={}", command.evaluationId());

            return null;

        } catch (Exception e) {
            log.error("Erro ao aprovar avaliação: {}", command, e);
            throw e;
        }
    }

    /**
     * Obtém o ID do reviewer atual do contexto de segurança.
     *
     * @return ID do reviewer
     * @throws SecurityException se usuário não autenticado
     */
    private String getCurrentReviewerId() {
        Authentication authentication = SecurityContextHolder.getContext().getAuthentication();
        if (authentication == null || !authentication.isAuthenticated()) {
            throw new SecurityException("User not authenticated");
        }
        return authentication.getName();
    }

    /**
     * Valida ajuste manual do valor.
     *
     * @param originalValue valor original
     * @param adjustedValue valor ajustado
     */
    private void validateManualAdjustment(Money originalValue, BigDecimal adjustedValue) {
        if (originalValue == null) {
            throw new DomainException("Cannot adjust value for evaluation without final value");
        }

        BigDecimal originalAmount = originalValue.getAmount();
        BigDecimal difference = adjustedValue.subtract(originalAmount).abs();
        BigDecimal percentageChange = difference.divide(originalAmount, 4, BigDecimal.ROUND_HALF_UP)
            .multiply(BigDecimal.valueOf(100));

        // Se ajuste > 10%, requer admin
        if (percentageChange.compareTo(BigDecimal.valueOf(10)) > 0) {
            if (!isCurrentUserAdmin()) {
                throw new DomainException("Adjustment over 10% requires admin approval");
            }
        }
    }

    /**
     * Verifica se o usuário atual possui role ADMIN.
     *
     * @return true se é admin, false caso contrário
     */
    private boolean isCurrentUserAdmin() {
        Authentication authentication = SecurityContextHolder.getContext().getAuthentication();
        if (authentication == null) {
            return false;
        }
        return authentication.getAuthorities().stream()
            .anyMatch(auth -> auth.getAuthority().equals("ROLE_ADMIN"));
    }
}