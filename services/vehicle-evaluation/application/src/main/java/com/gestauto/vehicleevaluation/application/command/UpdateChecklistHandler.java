package com.gestauto.vehicleevaluation.application.command;

import com.gestauto.vehicleevaluation.domain.entity.EvaluationChecklist;
import com.gestauto.vehicleevaluation.domain.entity.VehicleEvaluation;
import com.gestauto.vehicleevaluation.domain.repository.EvaluationChecklistRepository;
import com.gestauto.vehicleevaluation.domain.repository.VehicleEvaluationRepository;
import com.gestauto.vehicleevaluation.domain.value.EvaluationId;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.stereotype.Component;
import org.springframework.transaction.annotation.Transactional;

/**
 * Handler para processar comando de atualização de checklist.
 *
 * Este handler é responsável por:
 * - Buscar a avaliação existente
 * - Validar se é possível editar o checklist
 * - Mapear os dados do comando para a entidade
 * - Calcular score de conservação
 * - Identificar questões críticas
 * - Persistir as alterações
 */
@Component
@RequiredArgsConstructor
@Slf4j
public class UpdateChecklistHandler implements CommandHandler<UpdateChecklistCommand, Void> {

    private final VehicleEvaluationRepository evaluationRepository;
    private final EvaluationChecklistRepository checklistRepository;

    @Override
    @Transactional
    public Void handle(UpdateChecklistCommand command) {
        EvaluationId evaluationId = EvaluationId.from(command.evaluationId());

        // 1. Buscar avaliação
        VehicleEvaluation evaluation = evaluationRepository.findById(evaluationId)
                .orElseThrow(() -> new RuntimeException("Evaluation not found: " + evaluationId));

        log.info("Atualizar checklist da avaliação: {}", evaluationId);

        // 2. Buscar ou criar checklist
        EvaluationChecklist checklist = checklistRepository.findByEvaluationId(evaluationId)
                .orElseGet(() -> EvaluationChecklist.create(evaluationId));

        // 3. Atualizar campos da lataria e pintura
        if (command.bodyCondition() != null) {
            checklist.setBodyCondition(command.bodyCondition());
        }
        if (command.paintCondition() != null) {
            checklist.setPaintCondition(command.paintCondition());
        }
        if (command.rustPresence() != null) {
            checklist.setRustPresence(command.rustPresence());
        }
        if (command.lightScratches() != null) {
            checklist.setLightScratches(command.lightScratches());
        }
        if (command.deepScratches() != null) {
            checklist.setDeepScratches(command.deepScratches());
        }
        if (command.smallDents() != null) {
            checklist.setSmallDents(command.smallDents());
        }
        if (command.largeDents() != null) {
            checklist.setLargeDents(command.largeDents());
        }
        if (command.doorRepairs() != null) {
            checklist.setDoorRepairs(command.doorRepairs());
        }
        if (command.fenderRepairs() != null) {
            checklist.setFenderRepairs(command.fenderRepairs());
        }
        if (command.hoodRepairs() != null) {
            checklist.setHoodRepairs(command.hoodRepairs());
        }
        if (command.trunkRepairs() != null) {
            checklist.setTrunkRepairs(command.trunkRepairs());
        }
        if (command.heavyBodywork() != null) {
            checklist.setHeavyBodywork(command.heavyBodywork());
        }

        // 4. Atualizar campos mecânicos
        if (command.engineCondition() != null) {
            checklist.setEngineCondition(command.engineCondition());
        }
        if (command.transmissionCondition() != null) {
            checklist.setTransmissionCondition(command.transmissionCondition());
        }
        if (command.suspensionCondition() != null) {
            checklist.setSuspensionCondition(command.suspensionCondition());
        }
        if (command.brakeCondition() != null) {
            checklist.setBrakeCondition(command.brakeCondition());
        }
        if (command.oilLeaks() != null) {
            checklist.setOilLeaks(command.oilLeaks());
        }
        if (command.waterLeaks() != null) {
            checklist.setWaterLeaks(command.waterLeaks());
        }
        if (command.timingBelt() != null) {
            checklist.setTimingBelt(command.timingBelt());
        }
        if (command.batteryCondition() != null) {
            checklist.setBatteryCondition(command.batteryCondition());
        }

        // 5. Atualizar campos de pneus
        if (command.tiresCondition() != null) {
            checklist.setTiresCondition(command.tiresCondition());
        }
        if (command.unevenWear() != null) {
            checklist.setUnevenWear(command.unevenWear());
        }
        if (command.lowTread() != null) {
            checklist.setLowTread(command.lowTread());
        }

        // 6. Atualizar campos do interior
        if (command.seatsCondition() != null) {
            checklist.setSeatsCondition(command.seatsCondition());
        }
        if (command.dashboardCondition() != null) {
            checklist.setDashboardCondition(command.dashboardCondition());
        }
        if (command.electronicsCondition() != null) {
            checklist.setElectronicsCondition(command.electronicsCondition());
        }
        if (command.seatDamage() != null) {
            checklist.setSeatDamage(command.seatDamage());
        }
        if (command.doorPanelDamage() != null) {
            checklist.setDoorPanelDamage(command.doorPanelDamage());
        }
        if (command.steeringWheelWear() != null) {
            checklist.setSteeringWheelWear(command.steeringWheelWear());
        }

        // 7. Atualizar campos de documentação
        if (command.crvlPresent() != null) {
            checklist.setCrvlPresent(command.crvlPresent());
        }
        if (command.manualPresent() != null) {
            checklist.setManualPresent(command.manualPresent());
        }
        if (command.spareKeyPresent() != null) {
            checklist.setSpareKeyPresent(command.spareKeyPresent());
        }
        if (command.maintenanceRecords() != null) {
            checklist.setMaintenanceRecords(command.maintenanceRecords());
        }

        // 8. Atualizar observações
        if (command.mechanicalNotes() != null) {
            checklist.setMechanicalNotes(command.mechanicalNotes());
        }
        if (command.aestheticNotes() != null) {
            checklist.setAestheticNotes(command.aestheticNotes());
        }
        if (command.documentationNotes() != null) {
            checklist.setDocumentationNotes(command.documentationNotes());
        }

        // 9. Validar questões críticas
        checklist.clearCriticalIssues();
        validateCriticalIssues(checklist);

        // 10. Calcular score de conservação
        int conservationScore = checklist.calculateScore();
        checklist.setConservationScore(conservationScore);

        log.info("Checklist atualizado - Score: {}, Questões críticas: {}", 
                conservationScore, checklist.getCriticalIssues().size());

        // 11. Validar se há questões bloqueantes
        if (checklist.hasBlockingIssues()) {
            log.warn("Checklist tem questões bloqueantes que impedem aprovação");
        }

        // 12. Salvar checklist
        checklistRepository.save(checklist);

        log.info("Checklist salvo com sucesso para avaliação: {}", evaluationId);

        return null;
    }

    /**
     * Valida e identifica questões críticas do checklist.
     *
     * @param checklist checklist a validar
     */
    private void validateCriticalIssues(EvaluationChecklist checklist) {
        // Documentação crítica
        if (!checklist.isCrvlPresent()) {
            checklist.addCriticalIssue("CRVL ausente ou inválido");
        }

        // Problemas mecânicos críticos
        if ("POOR".equals(checklist.getEngineCondition())) {
            checklist.addCriticalIssue("Motor com problemas significativos");
        }

        if ("POOR".equals(checklist.getBrakeCondition())) {
            checklist.addCriticalIssue("Sistema de freios com problemas");
        }

        if ("POOR".equals(checklist.getTransmissionCondition())) {
            checklist.addCriticalIssue("Transmissão com problemas significativos");
        }

        // Problemas estruturais críticos
        if (checklist.isHeavyBodywork()) {
            checklist.addCriticalIssue("Trabalho de lataria pesada detectado");
        }

        // Vazamentos críticos
        if (checklist.isOilLeaks() && checklist.isWaterLeaks()) {
            checklist.addCriticalIssue("Vazamentos múltiplos (óleo e água)");
        }

        // Múltiplos reparos em estrutura
        int totalStructuralRepairs = checklist.getDoorRepairs() + 
                                     checklist.getFenderRepairs() + 
                                     checklist.getHoodRepairs() + 
                                     checklist.getTrunkRepairs();
        
        if (totalStructuralRepairs > 5) {
            checklist.addCriticalIssue("Múltiplos reparos estruturais detectados");
        }

        // Pneus críticos
        if ("POOR".equals(checklist.getTiresCondition())) {
            checklist.addCriticalIssue("Pneus em estado crítico");
        }

        if (checklist.isLowTread() && checklist.isUnevenWear()) {
            checklist.addCriticalIssue("Pneus com profundidade baixa e desgaste irregular");
        }

        // Interior crítico
        if ("POOR".equals(checklist.getElectronicsCondition())) {
            checklist.addCriticalIssue("Eletrônicos com problemas críticos");
        }
    }
}
