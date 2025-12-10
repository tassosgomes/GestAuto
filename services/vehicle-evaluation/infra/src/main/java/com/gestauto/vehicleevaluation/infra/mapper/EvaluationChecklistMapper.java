package com.gestauto.vehicleevaluation.infra.mapper;

import com.gestauto.vehicleevaluation.domain.entity.EvaluationChecklist;
import com.gestauto.vehicleevaluation.domain.value.EvaluationId;
import com.gestauto.vehicleevaluation.infra.entity.EvaluationChecklistJpaEntity;
import com.gestauto.vehicleevaluation.infra.entity.VehicleEvaluationJpaEntity;
import java.util.ArrayList;
import java.util.List;
import java.util.UUID;

/**
 * Mapper para checklists de avaliação.
 */
public final class EvaluationChecklistMapper {

    private EvaluationChecklistMapper() {
    }

    public static EvaluationChecklistJpaEntity toEntity(EvaluationChecklist checklist, VehicleEvaluationJpaEntity evaluationJpa) {
        if (checklist == null) {
            return null;
        }
        EvaluationChecklistJpaEntity entity = new EvaluationChecklistJpaEntity();
        entity.setChecklistId(UUID.fromString(checklist.getChecklistId()));
        entity.setEvaluation(evaluationJpa);
        entity.setBodyCondition(checklist.getBodyCondition());
        entity.setPaintCondition(checklist.getPaintCondition());
        entity.setRustPresence(checklist.isRustPresence());
        entity.setLightScratches(checklist.isLightScratches());
        entity.setDeepScratches(checklist.isDeepScratches());
        entity.setSmallDents(checklist.isSmallDents());
        entity.setLargeDents(checklist.isLargeDents());
        entity.setDoorRepairs(checklist.getDoorRepairs());
        entity.setFenderRepairs(checklist.getFenderRepairs());
        entity.setHoodRepairs(checklist.getHoodRepairs());
        entity.setTrunkRepairs(checklist.getTrunkRepairs());
        entity.setHeavyBodywork(checklist.isHeavyBodywork());
        entity.setEngineCondition(checklist.getEngineCondition());
        entity.setTransmissionCondition(checklist.getTransmissionCondition());
        entity.setSuspensionCondition(checklist.getSuspensionCondition());
        entity.setBrakeCondition(checklist.getBrakeCondition());
        entity.setOilLeaks(checklist.isOilLeaks());
        entity.setWaterLeaks(checklist.isWaterLeaks());
        entity.setTimingBelt(checklist.isTimingBelt());
        entity.setBatteryCondition(checklist.getBatteryCondition());
        entity.setTiresCondition(checklist.getTiresCondition());
        entity.setUnevenWear(checklist.isUnevenWear());
        entity.setLowTread(checklist.isLowTread());
        entity.setSeatsCondition(checklist.getSeatsCondition());
        entity.setDashboardCondition(checklist.getDashboardCondition());
        entity.setElectronicsCondition(checklist.getElectronicsCondition());
        entity.setSeatDamage(checklist.isSeatDamage());
        entity.setDoorPanelDamage(checklist.isDoorPanelDamage());
        entity.setSteeringWheelWear(checklist.isSteeringWheelWear());
        entity.setCrvlPresent(checklist.isCrvlPresent());
        entity.setManualPresent(checklist.isManualPresent());
        entity.setSpareKeyPresent(checklist.isSpareKeyPresent());
        entity.setMaintenanceRecords(checklist.isMaintenanceRecords());
        entity.setMechanicalNotes(checklist.getMechanicalNotes());
        entity.setAestheticNotes(checklist.getAestheticNotes());
        entity.setDocumentationNotes(checklist.getDocumentationNotes());
        entity.setCriticalIssues(new ArrayList<>(checklist.getCriticalIssues()));
        entity.setConservationScore(checklist.getConservationScore());
        entity.setCreatedAt(checklist.getCreatedAt());
        entity.setUpdatedAt(checklist.getUpdatedAt());
        return entity;
    }

    public static EvaluationChecklist toDomain(EvaluationChecklistJpaEntity entity) {
        if (entity == null) {
            return null;
        }
        List<String> issues = entity.getCriticalIssues() != null
            ? new ArrayList<>(entity.getCriticalIssues())
            : new ArrayList<>();

        return EvaluationChecklist.restore(
            entity.getChecklistId().toString(),
            EvaluationId.from(entity.getEvaluation().getId()),
            entity.getBodyCondition(),
            entity.getPaintCondition(),
            entity.isRustPresence(),
            entity.isLightScratches(),
            entity.isDeepScratches(),
            entity.isSmallDents(),
            entity.isLargeDents(),
            entity.getDoorRepairs(),
            entity.getFenderRepairs(),
            entity.getHoodRepairs(),
            entity.getTrunkRepairs(),
            entity.isHeavyBodywork(),
            entity.getEngineCondition(),
            entity.getTransmissionCondition(),
            entity.getSuspensionCondition(),
            entity.getBrakeCondition(),
            entity.isOilLeaks(),
            entity.isWaterLeaks(),
            entity.isTimingBelt(),
            entity.getBatteryCondition(),
            entity.getTiresCondition(),
            entity.isUnevenWear(),
            entity.isLowTread(),
            entity.getSeatsCondition(),
            entity.getDashboardCondition(),
            entity.getElectronicsCondition(),
            entity.isSeatDamage(),
            entity.isDoorPanelDamage(),
            entity.isSteeringWheelWear(),
            entity.isCrvlPresent(),
            entity.isManualPresent(),
            entity.isSpareKeyPresent(),
            entity.isMaintenanceRecords(),
            entity.getMechanicalNotes(),
            entity.getAestheticNotes(),
            entity.getDocumentationNotes(),
            issues,
            entity.getConservationScore(),
            entity.getCreatedAt(),
            entity.getUpdatedAt()
        );
    }
}
