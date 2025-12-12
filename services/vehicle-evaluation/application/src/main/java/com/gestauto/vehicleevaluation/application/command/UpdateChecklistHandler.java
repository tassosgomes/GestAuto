package com.gestauto.vehicleevaluation.application.command;

import com.gestauto.vehicleevaluation.application.dto.UpdateChecklistCommand;
import com.gestauto.vehicleevaluation.application.service.DomainEventPublisherService;
import com.gestauto.vehicleevaluation.domain.entity.EvaluationChecklist;
import com.gestauto.vehicleevaluation.domain.entity.VehicleEvaluation;
import com.gestauto.vehicleevaluation.domain.enums.EvaluationStatus;
import com.gestauto.vehicleevaluation.domain.event.ChecklistCompletedEvent;
import com.gestauto.vehicleevaluation.domain.exception.EvaluationNotFoundException;
import com.gestauto.vehicleevaluation.domain.exception.InvalidEvaluationStatusException;
import com.gestauto.vehicleevaluation.domain.repository.EvaluationChecklistRepository;
import com.gestauto.vehicleevaluation.domain.repository.VehicleEvaluationRepository;
import com.gestauto.vehicleevaluation.domain.value.EvaluationId;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.stereotype.Component;
import org.springframework.transaction.annotation.Transactional;

import java.util.UUID;

/**
 * Handler para atualização do checklist técnico de uma avaliação.
 *
 * Este handler coordena a atualização do checklist, validações de negócio,
 * cálculo de score e publicação de eventos de domínio.
 */
@Component
@RequiredArgsConstructor
@Slf4j
public class UpdateChecklistHandler implements CommandHandler<UpdateChecklistCommand, Void> {

    private final VehicleEvaluationRepository evaluationRepository;
    private final EvaluationChecklistRepository checklistRepository;
    private final DomainEventPublisherService eventPublisher;

    @Override
    @Transactional
    public Void handle(UpdateChecklistCommand command) throws Exception {
        log.info("Updating checklist for evaluation: {}", command.evaluationId());

        // 1. Buscar avaliação
        EvaluationId evaluationId = EvaluationId.from(command.evaluationId());
        VehicleEvaluation evaluation = evaluationRepository.findById(evaluationId)
                .orElseThrow(() -> new EvaluationNotFoundException(
                        "Evaluation not found: " + command.evaluationId()
                ));

        // 2. Validar status (pode editar apenas em DRAFT ou IN_PROGRESS)
        if (evaluation.getStatus() != EvaluationStatus.DRAFT &&
            evaluation.getStatus() != EvaluationStatus.IN_PROGRESS) {
            throw new InvalidEvaluationStatusException(evaluation.getStatus(), "update checklist");
        }

        // 3. Buscar ou criar checklist
        EvaluationChecklist checklist = checklistRepository
                .findByEvaluationId(evaluationId)
                .orElseGet(() -> EvaluationChecklist.create(evaluationId));

        // 4. Mapear DTO para entidade de checklist
        mapCommandToChecklist(command, checklist);

        // 5. Calcular score de conservação
        int score = checklist.calculateScore();
        checklist.setConservationScore(score);
        log.info("Conservation score calculated: {} for evaluation: {}", 
                 score, command.evaluationId());

        // 6. Validar itens críticos
        if (checklist.hasBlockingIssues()) {
            log.warn("Blocking issues found in checklist for evaluation: {}. Issues: {}", 
                     command.evaluationId(), checklist.getCriticalIssues());
        }

        // 7. Salvar checklist
        EvaluationChecklist savedChecklist = checklistRepository.save(checklist);
        log.info("Checklist saved successfully for evaluation: {}", command.evaluationId());

        // 8. Atualizar avaliação com checklist
        evaluation.updateChecklist(savedChecklist);
        evaluationRepository.save(evaluation);

        // 9. Publicar ChecklistCompletedEvent
        ChecklistCompletedEvent event = new ChecklistCompletedEvent(
                command.evaluationId(),
                score,
                checklist.hasBlockingIssues(),
                checklist.getCriticalIssues()
        );
        eventPublisher.publish(event);
        log.info("ChecklistCompletedEvent published for evaluation: {}", command.evaluationId());

        return null;
    }

    /**
     * Mapeia o command para a entidade de checklist.
     */
    private void mapCommandToChecklist(UpdateChecklistCommand command, EvaluationChecklist checklist) {
        // Seção Bodywork
        if (command.bodywork() != null) {
            var bodywork = command.bodywork();
            if (bodywork.bodyCondition() != null) {
                checklist.setBodyCondition(bodywork.bodyCondition());
            }
            if (bodywork.paintCondition() != null) {
                checklist.setPaintCondition(bodywork.paintCondition());
            }
            if (bodywork.rustPresence() != null) {
                checklist.setRustPresence(bodywork.rustPresence());
            }
            if (bodywork.lightScratches() != null) {
                checklist.setLightScratches(bodywork.lightScratches());
            }
            if (bodywork.deepScratches() != null) {
                checklist.setDeepScratches(bodywork.deepScratches());
            }
            if (bodywork.smallDents() != null) {
                checklist.setSmallDents(bodywork.smallDents());
            }
            if (bodywork.largeDents() != null) {
                checklist.setLargeDents(bodywork.largeDents());
            }
            if (bodywork.doorRepairs() != null) {
                checklist.setDoorRepairs(bodywork.doorRepairs());
            }
            if (bodywork.fenderRepairs() != null) {
                checklist.setFenderRepairs(bodywork.fenderRepairs());
            }
            if (bodywork.hoodRepairs() != null) {
                checklist.setHoodRepairs(bodywork.hoodRepairs());
            }
            if (bodywork.trunkRepairs() != null) {
                checklist.setTrunkRepairs(bodywork.trunkRepairs());
            }
            if (bodywork.heavyBodywork() != null) {
                checklist.setHeavyBodywork(bodywork.heavyBodywork());
            }
            if (bodywork.observations() != null) {
                checklist.setAestheticNotes(bodywork.observations());
            }
        }

        // Seção Mechanical
        if (command.mechanical() != null) {
            var mechanical = command.mechanical();
            if (mechanical.engineCondition() != null) {
                checklist.setEngineCondition(mechanical.engineCondition());
            }
            if (mechanical.transmissionCondition() != null) {
                checklist.setTransmissionCondition(mechanical.transmissionCondition());
            }
            if (mechanical.suspensionCondition() != null) {
                checklist.setSuspensionCondition(mechanical.suspensionCondition());
            }
            if (mechanical.brakeCondition() != null) {
                checklist.setBrakeCondition(mechanical.brakeCondition());
            }
            if (mechanical.oilLeaks() != null) {
                checklist.setOilLeaks(mechanical.oilLeaks());
            }
            if (mechanical.waterLeaks() != null) {
                checklist.setWaterLeaks(mechanical.waterLeaks());
            }
            if (mechanical.timingBelt() != null) {
                checklist.setTimingBelt(mechanical.timingBelt());
            }
            if (mechanical.batteryCondition() != null) {
                checklist.setBatteryCondition(mechanical.batteryCondition());
            }
            if (mechanical.observations() != null) {
                checklist.setMechanicalNotes(mechanical.observations());
            }
        }

        // Seção Tires
        if (command.tires() != null) {
            var tires = command.tires();
            if (tires.tiresCondition() != null) {
                checklist.setTiresCondition(tires.tiresCondition());
            }
            if (tires.unevenWear() != null) {
                checklist.setUnevenWear(tires.unevenWear());
            }
            if (tires.lowTread() != null) {
                checklist.setLowTread(tires.lowTread());
            }
        }

        // Seção Interior
        if (command.interior() != null) {
            var interior = command.interior();
            if (interior.seatsCondition() != null) {
                checklist.setSeatsCondition(interior.seatsCondition());
            }
            if (interior.dashboardCondition() != null) {
                checklist.setDashboardCondition(interior.dashboardCondition());
            }
            if (interior.electronicsCondition() != null) {
                checklist.setElectronicsCondition(interior.electronicsCondition());
            }
            if (interior.seatDamage() != null) {
                checklist.setSeatDamage(interior.seatDamage());
            }
            if (interior.doorPanelDamage() != null) {
                checklist.setDoorPanelDamage(interior.doorPanelDamage());
            }
            if (interior.steeringWheelWear() != null) {
                checklist.setSteeringWheelWear(interior.steeringWheelWear());
            }
        }

        // Seção Documents (obrigatória)
        if (command.documents() != null) {
            var documents = command.documents();
            checklist.setCrvlPresent(documents.crvlPresent());
            if (documents.manualPresent() != null) {
                checklist.setManualPresent(documents.manualPresent());
            }
            if (documents.spareKeyPresent() != null) {
                checklist.setSpareKeyPresent(documents.spareKeyPresent());
            }
            if (documents.maintenanceRecords() != null) {
                checklist.setMaintenanceRecords(documents.maintenanceRecords());
            }
            if (documents.observations() != null) {
                checklist.setDocumentationNotes(documents.observations());
            }
        }
    }
}
