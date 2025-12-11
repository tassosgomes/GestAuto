package com.gestauto.vehicleevaluation.domain.entity;

import com.gestauto.vehicleevaluation.domain.value.EvaluationId;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;

import java.util.UUID;

import static org.junit.jupiter.api.Assertions.*;

@DisplayName("EvaluationChecklist Domain Entity Tests")
class EvaluationChecklistTest {

    private EvaluationId evaluationId;
    private EvaluationChecklist checklist;

    @BeforeEach
    void setUp() {
        evaluationId = new EvaluationId(UUID.randomUUID());
        checklist = EvaluationChecklist.create(evaluationId);
    }

    @Test
    @DisplayName("Should create checklist with default values")
    void shouldCreateChecklistWithDefaultValues() {
        assertNotNull(checklist);
        assertEquals("GOOD", checklist.getBodyCondition());
        assertEquals("GOOD", checklist.getEngineCondition());
        assertEquals(0, checklist.getDoorRepairs());
        assertFalse(checklist.isRustPresence());
        assertFalse(checklist.isCrvlPresent());
    }

    @Test
    @DisplayName("Should calculate perfect score for pristine vehicle")
    void shouldCalculatePerfectScoreForPristineVehicle() {
        // Arrange: veículo em condição perfeita
        checklist.setBodyCondition("EXCELLENT");
        checklist.setPaintCondition("EXCELLENT");
        checklist.setEngineCondition("EXCELLENT");
        checklist.setTransmissionCondition("EXCELLENT");
        checklist.setSuspensionCondition("EXCELLENT");
        checklist.setBrakeCondition("EXCELLENT");
        checklist.setTiresCondition("EXCELLENT");
        checklist.setSeatsCondition("EXCELLENT");
        checklist.setDashboardCondition("EXCELLENT");
        checklist.setElectronicsCondition("EXCELLENT");
        checklist.setBatteryCondition("EXCELLENT");
        checklist.setCrvlPresent(true);
        checklist.setManualPresent(true);
        checklist.setSpareKeyPresent(true);
        checklist.setMaintenanceRecords(true);

        // Act
        int score = checklist.calculateScore();

        // Assert
        assertEquals(100, score);
    }

    @Test
    @DisplayName("Should calculate low score for vehicle with multiple issues")
    void shouldCalculateLowScoreForVehicleWithMultipleIssues() {
        // Arrange: veículo com problemas severos
        checklist.setBodyCondition("POOR");
        checklist.setPaintCondition("POOR");
        checklist.setRustPresence(true);
        checklist.setDeepScratches(true);
        checklist.setLargeDents(true);
        checklist.setHeavyBodywork(true);
        checklist.setEngineCondition("POOR");
        checklist.setTransmissionCondition("POOR");
        checklist.setBrakeCondition("POOR");
        checklist.setOilLeaks(true);
        checklist.setWaterLeaks(true);
        checklist.setTiresCondition("POOR");
        checklist.setUnevenWear(true);
        checklist.setLowTread(true);
        checklist.setSeatsCondition("POOR");
        checklist.setDashboardCondition("POOR");
        checklist.setElectronicsCondition("POOR");
        checklist.setSeatDamage(true);
        checklist.setCrvlPresent(false);

        // Act
        int score = checklist.calculateScore();

        // Assert
        assertTrue(score < 50, "Score should be less than 50 for severely damaged vehicle");
        assertTrue(score >= 0, "Score should never be negative");
    }

    @Test
    @DisplayName("Should identify blocking issues when critical problems exist")
    void shouldIdentifyBlockingIssuesWhenCriticalProblemsExist() {
        // Arrange
        checklist.setEngineCondition("POOR");
        checklist.setCrvlPresent(false);
        checklist.setHeavyBodywork(true);

        // Act & Assert
        assertTrue(checklist.hasBlockingIssues());
    }

    @Test
    @DisplayName("Should not have blocking issues for good vehicle")
    void shouldNotHaveBlockingIssuesForGoodVehicle() {
        // Arrange
        checklist.setBodyCondition("GOOD");
        checklist.setEngineCondition("GOOD");
        checklist.setTransmissionCondition("GOOD");
        checklist.setBrakeCondition("GOOD");
        checklist.setCrvlPresent(true);
        checklist.setHeavyBodywork(false);

        // Act & Assert
        assertFalse(checklist.hasBlockingIssues());
    }

    @Test
    @DisplayName("Should validate condition strings")
    void shouldValidateConditionStrings() {
        // Valid conditions should work
        assertDoesNotThrow(() -> checklist.setBodyCondition("EXCELLENT"));
        assertDoesNotThrow(() -> checklist.setBodyCondition("GOOD"));
        assertDoesNotThrow(() -> checklist.setBodyCondition("FAIR"));
        assertDoesNotThrow(() -> checklist.setBodyCondition("POOR"));

        // Invalid condition should throw
        assertThrows(IllegalArgumentException.class, 
                    () -> checklist.setBodyCondition("INVALID"));
    }

    @Test
    @DisplayName("Should validate repair counts")
    void shouldValidateRepairCounts() {
        // Valid counts
        assertDoesNotThrow(() -> checklist.setDoorRepairs(0));
        assertDoesNotThrow(() -> checklist.setDoorRepairs(5));
        assertDoesNotThrow(() -> checklist.setDoorRepairs(10));

        // Invalid counts
        assertThrows(IllegalArgumentException.class, 
                    () -> checklist.setDoorRepairs(-1));
        assertThrows(IllegalArgumentException.class, 
                    () -> checklist.setDoorRepairs(11));
    }

    @Test
    @DisplayName("Should mark as updated when setting values")
    void shouldMarkAsUpdatedWhenSettingValues() {
        // Arrange
        var initialUpdatedAt = checklist.getUpdatedAt();

        // Wait a bit to ensure timestamp difference
        try {
            Thread.sleep(10);
        } catch (InterruptedException e) {
            Thread.currentThread().interrupt();
        }

        // Act
        checklist.setBodyCondition("FAIR");

        // Assert
        assertTrue(checklist.getUpdatedAt().isAfter(initialUpdatedAt));
    }

    @Test
    @DisplayName("Should check if checklist is complete")
    void shouldCheckIfChecklistIsComplete() {
        // Arrange - fill all required fields
        checklist.setBodyCondition("GOOD");
        checklist.setPaintCondition("GOOD");
        checklist.setEngineCondition("GOOD");
        checklist.setTransmissionCondition("GOOD");
        checklist.setSuspensionCondition("GOOD");
        checklist.setBrakeCondition("GOOD");
        checklist.setTiresCondition("GOOD");
        checklist.setBatteryCondition("GOOD");
        checklist.setSeatsCondition("GOOD");
        checklist.setDashboardCondition("GOOD");
        checklist.setElectronicsCondition("GOOD");

        // Act & Assert
        assertTrue(checklist.isComplete());
    }

    @Test
    @DisplayName("Should add and retrieve critical issues")
    void shouldAddAndRetrieveCriticalIssues() {
        // Arrange & Act
        checklist.addCriticalIssue("Engine has severe damage");
        checklist.addCriticalIssue("Structural rust in chassis");

        // Assert
        assertEquals(2, checklist.getCriticalIssues().size());
        assertTrue(checklist.getCriticalIssues().contains("Engine has severe damage"));
    }

    @Test
    @DisplayName("Should clear critical issues")
    void shouldClearCriticalIssues() {
        // Arrange
        checklist.addCriticalIssue("Issue 1");
        checklist.addCriticalIssue("Issue 2");

        // Act
        checklist.clearCriticalIssues();

        // Assert
        assertEquals(0, checklist.getCriticalIssues().size());
    }

    @Test
    @DisplayName("Should calculate penalties by section")
    void shouldCalculatePenaltiesBySection() {
        // Arrange: Add issues that affect different sections
        checklist.setRustPresence(true); // 15 points from bodywork
        checklist.setOilLeaks(true); // 15 points from mechanical
        checklist.setLowTread(true); // 12 points from tires
        checklist.setCrvlPresent(false); // 25 points from documentation

        // Act
        int score = checklist.calculateScore();

        // Assert
        // Base 100 - (15 + 15 + 12 + 25) = 33
        assertEquals(33, score);
    }

    @Test
    @DisplayName("Should generate summary with all sections")
    void shouldGenerateSummaryWithAllSections() {
        // Arrange
        checklist.setBodyCondition("FAIR");
        checklist.setRustPresence(true);
        checklist.setEngineCondition("GOOD");
        checklist.setCrvlPresent(true);
        checklist.setManualPresent(false);

        // Act
        String summary = checklist.generateSummary();

        // Assert
        assertNotNull(summary);
        assertTrue(summary.contains("RESUMO DO CHECKLIST TÉCNICO"));
        assertTrue(summary.contains("Score de Conservação:"));
        assertTrue(summary.contains("LATARIA E PINTURA"));
        assertTrue(summary.contains("MECÂNICA"));
        assertTrue(summary.contains("PNEUS"));
        assertTrue(summary.contains("INTERIOR"));
        assertTrue(summary.contains("DOCUMENTAÇÃO"));
        assertTrue(summary.contains("Ferrugem detectada"));
    }

    @Test
    @DisplayName("Should restore checklist from persisted state")
    void shouldRestoreChecklistFromPersistedState() {
        // Arrange
        String checklistId = UUID.randomUUID().toString();
        
        // Act
        EvaluationChecklist restored = EvaluationChecklist.restore(
                checklistId, evaluationId,
                "FAIR", "GOOD", true,
                false, true, false,
                true, 2, 1, 0,
                1, true, "GOOD",
                "FAIR", "GOOD", "EXCELLENT",
                true, false, true,
                "GOOD", "FAIR", true,
                false, "GOOD", "FAIR",
                "GOOD", true, false,
                false, true, false,
                true, false, "Mechanical notes",
                "Aesthetic notes", "Doc notes", null,
                75, null, null
        );

        // Assert
        assertNotNull(restored);
        assertEquals(checklistId, restored.getChecklistId());
        assertEquals("FAIR", restored.getBodyCondition());
        assertTrue(restored.isRustPresence());
        assertEquals(2, restored.getDoorRepairs());
        assertEquals(75, restored.getConservationScore());
    }
}
