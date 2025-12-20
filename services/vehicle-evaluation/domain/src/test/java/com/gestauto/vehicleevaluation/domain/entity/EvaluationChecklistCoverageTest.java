package com.gestauto.vehicleevaluation.domain.entity;

import com.gestauto.vehicleevaluation.domain.value.EvaluationId;
import java.time.LocalDateTime;
import java.util.List;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.*;

@DisplayName("EvaluationChecklist Coverage Tests")
class EvaluationChecklistCoverageTest {

    @Test
    void shouldGenerateSummaryForHighScoreWithoutBlockingIssues() {
        EvaluationChecklist checklist = EvaluationChecklist.create(EvaluationId.generate());

        checklist.setCrvlPresent(true);
        checklist.setHeavyBodywork(false);
        checklist.setEngineCondition("GOOD");
        checklist.setBrakeCondition("GOOD");
        checklist.setTransmissionCondition("GOOD");

        int score = checklist.calculateScore();
        assertTrue(score >= 80);
        assertFalse(checklist.hasBlockingIssues());

        String summary = checklist.generateSummary();
        assertTrue(summary.contains("Score de Conservação"));
        assertTrue(summary.contains("(EXCELENTE)"));
        assertNotNull(checklist.getChecklistId());
        assertNotNull(checklist.getCreatedAt());
        assertNotNull(checklist.getUpdatedAt());
    }

    @Test
    void shouldGenerateSummaryForMediumScoresAndExercisePenalties() {
        EvaluationChecklist checklist = EvaluationChecklist.create(EvaluationId.generate());

        checklist.setCrvlPresent(true);

        checklist.setRustPresence(true);
        checklist.setLowTread(true);

        int score = checklist.calculateScore();
        assertTrue(score < 80 && score >= 60);

        String summary = checklist.generateSummary();
        assertTrue(summary.contains("(BOM)"));

        checklist.setEngineCondition("FAIR");
        checklist.setBrakeCondition("FAIR");

        int lowerScore = checklist.calculateScore();
        assertTrue(lowerScore < 60 && lowerScore >= 40);

        String summary2 = checklist.generateSummary();
        assertTrue(summary2.contains("(REGULAR)"));
    }

    @Test
    void shouldHandleBlockingIssuesCriticalIssuesAndRestore() {
        EvaluationId evaluationId = EvaluationId.generate();
        EvaluationChecklist checklist = EvaluationChecklist.create(evaluationId);

        checklist.setBodyCondition("POOR");
        checklist.setPaintCondition("POOR");
        checklist.setRustPresence(true);
        checklist.setDeepScratches(true);
        checklist.setLargeDents(true);
        checklist.setDoorRepairs(2);
        checklist.setFenderRepairs(1);
        checklist.setHoodRepairs(1);
        checklist.setTrunkRepairs(1);
        checklist.setHeavyBodywork(true);

        checklist.setEngineCondition("POOR");
        checklist.setTransmissionCondition("POOR");
        checklist.setSuspensionCondition("POOR");
        checklist.setBrakeCondition("POOR");
        checklist.setOilLeaks(true);
        checklist.setWaterLeaks(true);
        checklist.setTimingBelt(false);
        checklist.setBatteryCondition("POOR");

        checklist.setTiresCondition("POOR");
        checklist.setUnevenWear(true);
        checklist.setLowTread(true);

        checklist.setSeatsCondition("POOR");
        checklist.setDashboardCondition("POOR");
        checklist.setElectronicsCondition("POOR");
        checklist.setSeatDamage(true);
        checklist.setDoorPanelDamage(true);
        checklist.setSteeringWheelWear(true);

        checklist.setCrvlPresent(false);
        checklist.setManualPresent(false);
        checklist.setSpareKeyPresent(false);
        checklist.setMaintenanceRecords(false);

        checklist.setMechanicalNotes("oil leak");
        checklist.setAestheticNotes("bad paint");
        checklist.setDocumentationNotes("missing docs");

        checklist.addCriticalIssue("Severe engine issue");
        checklist.addCriticalIssue(" ");

        assertTrue(checklist.hasBlockingIssues());
        assertFalse(checklist.getCriticalIssues().isEmpty());

        int score = checklist.calculateScore();
        assertTrue(score < 40);

        String summary = checklist.generateSummary();
        assertTrue(summary.contains("PROBLEMAS CRÍTICOS"));
        assertTrue(summary.contains("(RUIM)"));

        checklist.clearCriticalIssues();
        assertTrue(checklist.getCriticalIssues().isEmpty());

        // Exercise restore path
        EvaluationChecklist restored = EvaluationChecklist.restore(
            "check-1",
            evaluationId,
            checklist.getBodyCondition(),
            checklist.getPaintCondition(),
            checklist.isRustPresence(),
            checklist.isLightScratches(),
            checklist.isDeepScratches(),
            checklist.isSmallDents(),
            checklist.isLargeDents(),
            checklist.getDoorRepairs(),
            checklist.getFenderRepairs(),
            checklist.getHoodRepairs(),
            checklist.getTrunkRepairs(),
            checklist.isHeavyBodywork(),
            checklist.getEngineCondition(),
            checklist.getTransmissionCondition(),
            checklist.getSuspensionCondition(),
            checklist.getBrakeCondition(),
            checklist.isOilLeaks(),
            checklist.isWaterLeaks(),
            checklist.isTimingBelt(),
            checklist.getBatteryCondition(),
            checklist.getTiresCondition(),
            checklist.isUnevenWear(),
            checklist.isLowTread(),
            checklist.getSeatsCondition(),
            checklist.getDashboardCondition(),
            checklist.getElectronicsCondition(),
            checklist.isSeatDamage(),
            checklist.isDoorPanelDamage(),
            checklist.isSteeringWheelWear(),
            checklist.isCrvlPresent(),
            checklist.isManualPresent(),
            checklist.isSpareKeyPresent(),
            checklist.isMaintenanceRecords(),
            checklist.getMechanicalNotes(),
            checklist.getAestheticNotes(),
            checklist.getDocumentationNotes(),
            List.of("a"),
            10,
            LocalDateTime.now().minusDays(1),
            LocalDateTime.now()
        );

        assertEquals("check-1", restored.getChecklistId());
        assertEquals(evaluationId, restored.getEvaluationId());
        assertEquals(1, restored.getCriticalIssues().size());
        restored.markAsUpdated();
        assertNotNull(restored.getUpdatedAt());
    }
}
