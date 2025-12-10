package com.gestauto.vehicleevaluation.domain.entity;

import com.gestauto.vehicleevaluation.domain.value.EvaluationId;
import java.time.LocalDateTime;
import java.util.ArrayList;
import java.util.Collections;
import java.util.List;
import java.util.Objects;
import java.util.UUID;

/**
 * Entidade de domínio representando o checklist técnico de uma avaliação.
 *
 * Esta entidade encapsula todas as verificações técnicas realizadas
 * no veículo, divididas por categorias específicas.
 */
public final class EvaluationChecklist {

    private final String checklistId;
    private final EvaluationId evaluationId;

    // Avaliação da Lataria e Pintura
    private String bodyCondition; // EXCELLENT, GOOD, FAIR, POOR
    private String paintCondition; // EXCELLENT, GOOD, FAIR, POOR
    private boolean rustPresence;
    private boolean lightScratches;
    private boolean deepScratches;
    private boolean smallDents;
    private boolean largeDents;
    private int doorRepairs;
    private int fenderRepairs;
    private int hoodRepairs;
    private int trunkRepairs;
    private boolean heavyBodywork;

    // Avaliação Mecânica
    private String engineCondition; // EXCELLENT, GOOD, FAIR, POOR
    private String transmissionCondition; // EXCELLENT, GOOD, FAIR, POOR
    private String suspensionCondition; // EXCELLENT, GOOD, FAIR, POOR
    private String brakeCondition; // EXCELLENT, GOOD, FAIR, POOR
    private boolean oilLeaks;
    private boolean waterLeaks;
    private boolean timingBelt;
    private String batteryCondition; // EXCELLENT, GOOD, FAIR, POOR

    // Avaliação de Pneus
    private String tiresCondition; // EXCELLENT, GOOD, FAIR, POOR
    private boolean unevenWear;
    private boolean lowTread;

    // Avaliação do Interior
    private String seatsCondition; // EXCELLENT, GOOD, FAIR, POOR
    private String dashboardCondition; // EXCELLENT, GOOD, FAIR, POOR
    private String electronicsCondition; // EXCELLENT, GOOD, FAIR, POOR
    private boolean seatDamage;
    private boolean doorPanelDamage;
    private boolean steeringWheelWear;

    // Avaliação de Documentação
    private boolean crvlPresent;
    private boolean manualPresent;
    private boolean spareKeyPresent;
    private boolean maintenanceRecords;

    // Observações e Resultados
    private String mechanicalNotes;
    private String aestheticNotes;
    private String documentationNotes;
    private final List<String> criticalIssues;
    private Integer conservationScore;
    private final LocalDateTime createdAt;
    private LocalDateTime updatedAt;

    private EvaluationChecklist(String checklistId, EvaluationId evaluationId) {
        this.checklistId = Objects.requireNonNull(checklistId, "ChecklistId cannot be null");
        this.evaluationId = Objects.requireNonNull(evaluationId, "EvaluationId cannot be null");
        this.createdAt = LocalDateTime.now();
        this.updatedAt = LocalDateTime.now();
        this.criticalIssues = new ArrayList<>();

        // Inicializa campos opcionais
        this.bodyCondition = "GOOD";
        this.paintCondition = "GOOD";
        this.engineCondition = "GOOD";
        this.transmissionCondition = "GOOD";
        this.suspensionCondition = "GOOD";
        this.brakeCondition = "GOOD";
        this.batteryCondition = "GOOD";
        this.tiresCondition = "GOOD";
        this.seatsCondition = "GOOD";
        this.dashboardCondition = "GOOD";
        this.electronicsCondition = "GOOD";
        this.doorRepairs = 0;
        this.fenderRepairs = 0;
        this.hoodRepairs = 0;
        this.trunkRepairs = 0;

        // Inicializa flags boolean como false
        this.rustPresence = false;
        this.lightScratches = false;
        this.deepScratches = false;
        this.smallDents = false;
        this.largeDents = false;
        this.heavyBodywork = false;
        this.oilLeaks = false;
        this.waterLeaks = false;
        this.timingBelt = false;
        this.unevenWear = false;
        this.lowTread = false;
        this.seatDamage = false;
        this.doorPanelDamage = false;
        this.steeringWheelWear = false;
        this.crvlPresent = false;
        this.manualPresent = false;
        this.spareKeyPresent = false;
        this.maintenanceRecords = false;
    }

    /**
     * Cria um novo checklist para uma avaliação.
     *
     * @param evaluationId ID da avaliação
     * @return nova instância de EvaluationChecklist
     */
    public static EvaluationChecklist create(EvaluationId evaluationId) {
        String checklistId = UUID.randomUUID().toString();
        return new EvaluationChecklist(checklistId, evaluationId);
    }

    /**
     * Verifica se o checklist está completo.
     *
     * @return true se estiver completo
     */
    public boolean isComplete() {
        return bodyCondition != null && !bodyCondition.trim().isEmpty() &&
               paintCondition != null && !paintCondition.trim().isEmpty() &&
               engineCondition != null && !engineCondition.trim().isEmpty() &&
               transmissionCondition != null && !transmissionCondition.trim().isEmpty() &&
               suspensionCondition != null && !suspensionCondition.trim().isEmpty() &&
               brakeCondition != null && !brakeCondition.trim().isEmpty() &&
               tiresCondition != null && !tiresCondition.trim().isEmpty() &&
               batteryCondition != null && !batteryCondition.trim().isEmpty() &&
               seatsCondition != null && !seatsCondition.trim().isEmpty() &&
               dashboardCondition != null && !dashboardCondition.trim().isEmpty() &&
               electronicsCondition != null && !electronicsCondition.trim().isEmpty();
    }

    /**
     * Verifica se há questões críticas que impedem aprovação.
     *
     * @return true se houver questões críticas
     */
    public boolean hasBlockingIssues() {
        return !criticalIssues.isEmpty() ||
               engineCondition.equals("POOR") ||
               brakeCondition.equals("POOR") ||
               transmissionCondition.equals("POOR") ||
               !crvlPresent ||
               heavyBodywork;
    }

    /**
     * Calcula o score de conservação (0-100).
     *
     * @return score de conservação
     */
    public int calculateScore() {
        int score = 100;

        // Penalidades da lataria e pintura
        score -= getBodyworkPenalty();
        score -= getPaintPenalty();

        // Penalidades mecânicas
        score -= getMechanicalPenalty();

        // Penalidades de pneus
        score -= getTiresPenalty();

        // Penalidades do interior
        score -= getInteriorPenalty();

        // Penalidades de documentação
        score -= getDocumentationPenalty();

        return Math.max(0, Math.min(100, score));
    }

    /**
     * Adiciona uma questão crítica.
     *
     * @param issue descrição da questão crítica
     */
    public void addCriticalIssue(String issue) {
        Objects.requireNonNull(issue, "Critical issue cannot be null");
        if (!issue.trim().isEmpty()) {
            criticalIssues.add(issue);
        }
    }

    /**
     * Limpa a lista de questões críticas.
     */
    public void clearCriticalIssues() {
        criticalIssues.clear();
    }

    // Métodos de penalidade para cálculo de score
    private int getBodyworkPenalty() {
        int penalty = 0;

        if (rustPresence) penalty += 15;
        if (deepScratches) penalty += 10;
        if (largeDents) penalty += 20;
        if (heavyBodywork) penalty += 25;

        penalty += doorRepairs * 5;
        penalty += fenderRepairs * 4;
        penalty += hoodRepairs * 8;
        penalty += trunkRepairs * 6;

        if (bodyCondition.equals("POOR")) penalty += 20;
        else if (bodyCondition.equals("FAIR")) penalty += 10;

        return penalty;
    }

    private int getPaintPenalty() {
        int penalty = 0;

        if (paintCondition.equals("POOR")) penalty += 15;
        else if (paintCondition.equals("FAIR")) penalty += 8;

        return penalty;
    }

    private int getMechanicalPenalty() {
        int penalty = 0;

        if (engineCondition.equals("POOR")) penalty += 25;
        else if (engineCondition.equals("FAIR")) penalty += 10;

        if (transmissionCondition.equals("POOR")) penalty += 20;
        else if (transmissionCondition.equals("FAIR")) penalty += 8;

        if (suspensionCondition.equals("POOR")) penalty += 15;
        else if (suspensionCondition.equals("FAIR")) penalty += 5;

        if (brakeCondition.equals("POOR")) penalty += 30;
        else if (brakeCondition.equals("FAIR")) penalty += 10;

        if (oilLeaks) penalty += 15;
        if (waterLeaks) penalty += 10;
        if (!timingBelt) penalty += 8;
        if (batteryCondition.equals("POOR")) penalty += 10;

        return penalty;
    }

    private int getTiresCondition() {
        int penalty = 0;

        if (tiresCondition.equals("POOR")) penalty += 15;
        else if (tiresCondition.equals("FAIR")) penalty += 8;
        else if (tiresCondition.equals("GOOD")) penalty += 3;

        if (unevenWear) penalty += 10;
        if (lowTread) penalty += 12;

        return penalty;
    }

    private int getInteriorPenalty() {
        int penalty = 0;

        if (seatsCondition.equals("POOR")) penalty += 15;
        else if (seatsCondition.equals("FAIR")) penalty += 8;

        if (dashboardCondition.equals("POOR")) penalty += 12;
        else if (dashboardCondition.equals("FAIR")) penalty += 5;

        if (electronicsCondition.equals("POOR")) penalty += 20;
        else if (electronicsCondition.equals("FAIR")) penalty += 8;

        if (seatDamage) penalty += 10;
        if (doorPanelDamage) penalty += 5;
        if (steeringWheelWear) penalty += 3;

        return penalty;
    }

    private int getDocumentationPenalty() {
        int penalty = 0;

        if (!crvlPresent) penalty += 25;
        if (!manualPresent) penalty += 5;
        if (!spareKeyPresent) penalty += 5;
        if (!maintenanceRecords) penalty += 3;

        return penalty;
    }

    /**
     * Marca o checklist como atualizado.
     */
    public void markAsUpdated() {
        this.updatedAt = LocalDateTime.now();
    }

    // Getters
    public String getChecklistId() {
        return checklistId;
    }

    public EvaluationId getEvaluationId() {
        return evaluationId;
    }

    public String getBodyCondition() {
        return bodyCondition;
    }

    public String getPaintCondition() {
        return paintCondition;
    }

    public boolean isRustPresence() {
        return rustPresence;
    }

    public boolean isLightScratches() {
        return lightScratches;
    }

    public boolean isDeepScratches() {
        return deepScratches;
    }

    public boolean isSmallDents() {
        return smallDents;
    }

    public boolean isLargeDents() {
        return largeDents;
    }

    public int getDoorRepairs() {
        return doorRepairs;
    }

    public int getFenderRepairs() {
        return fenderRepairs;
    }

    public int getHoodRepairs() {
        return hoodRepairs;
    }

    public int getTrunkRepairs() {
        return trunkRepairs;
    }

    public boolean isHeavyBodywork() {
        return heavyBodywork;
    }

    public String getEngineCondition() {
        return engineCondition;
    }

    public String getTransmissionCondition() {
        return transmissionCondition;
    }

    public String getSuspensionCondition() {
        return suspensionCondition;
    }

    public String getBrakeCondition() {
        return brakeCondition;
    }

    public boolean isOilLeaks() {
        return oilLeaks;
    }

    public boolean isWaterLeaks() {
        return waterLeaks;
    }

    public boolean isTimingBelt() {
        return timingBelt;
    }

    public String getBatteryCondition() {
        return batteryCondition;
    }

    public String getTiresCondition() {
        return tiresCondition;
    }

    public boolean isUnevenWear() {
        return unevenWear;
    }

    public boolean isLowTread() {
        return lowTread;
    }

    public String getSeatsCondition() {
        return seatsCondition;
    }

    public String getDashboardCondition() {
        return dashboardCondition;
    }

    public String getElectronicsCondition() {
        return electronicsCondition;
    }

    public boolean isSeatDamage() {
        return seatDamage;
    }

    public boolean isDoorPanelDamage() {
        return doorPanelDamage;
    }

    public boolean isSteeringWheelWear() {
        return steeringWheelWear;
    }

    public boolean isCrvlPresent() {
        return crvlPresent;
    }

    public boolean isManualPresent() {
        return manualPresent;
    }

    public boolean isSpareKeyPresent() {
        return spareKeyPresent;
    }

    public boolean isMaintenanceRecords() {
        return maintenanceRecords;
    }

    public String getMechanicalNotes() {
        return mechanicalNotes;
    }

    public String getAestheticNotes() {
        return aestheticNotes;
    }

    public String getDocumentationNotes() {
        return documentationNotes;
    }

    public List<String> getCriticalIssues() {
        return Collections.unmodifiableList(criticalIssues);
    }

    public Integer getConservationScore() {
        return conservationScore;
    }

    public LocalDateTime getCreatedAt() {
        return createdAt;
    }

    public LocalDateTime getUpdatedAt() {
        return updatedAt;
    }

    // Setters com validação
    public void setBodyCondition(String bodyCondition) {
        validateCondition(bodyCondition, "bodyCondition");
        this.bodyCondition = bodyCondition;
        markAsUpdated();
    }

    public void setPaintCondition(String paintCondition) {
        validateCondition(paintCondition, "paintCondition");
        this.paintCondition = paintCondition;
        markAsUpdated();
    }

    public void setRustPresence(boolean rustPresence) {
        this.rustPresence = rustPresence;
        markAsUpdated();
    }

    public void setLightScratches(boolean lightScratches) {
        this.lightScratches = lightScratches;
        markAsUpdated();
    }

    public void setDeepScratches(boolean deepScratches) {
        this.deepScratches = deepScratches;
        markAsUpdated();
    }

    public void setSmallDents(boolean smallDents) {
        this.smallDents = smallDents;
        markAsUpdated();
    }

    public void setLargeDents(boolean largeDents) {
        this.largeDents = largeDents;
        markAsUpdated();
    }

    public void setDoorRepairs(int doorRepairs) {
        validateRepairCount(doorRepairs, "doorRepairs");
        this.doorRepairs = doorRepairs;
        markAsUpdated();
    }

    public void setFenderRepairs(int fenderRepairs) {
        validateRepairCount(fenderRepairs, "fenderRepairs");
        this.fenderRepairs = fenderRepairs;
        markAsUpdated();
    }

    public void setHoodRepairs(int hoodRepairs) {
        validateRepairCount(hoodRepairs, "hoodRepairs");
        this.hoodRepairs = hoodRepairs;
        markAsUpdated();
    }

    public void setTrunkRepairs(int trunkRepairs) {
        validateRepairCount(trunkRepairs, "trunkRepairs");
        this.trunkRepairs = trunkRepairs;
        markAsUpdated();
    }

    public void setHeavyBodywork(boolean heavyBodywork) {
        this.heavyBodywork = heavyBodywork;
        markAsUpdated();
    }

    public void setEngineCondition(String engineCondition) {
        validateCondition(engineCondition, "engineCondition");
        this.engineCondition = engineCondition;
        markAsUpdated();
    }

    public void setTransmissionCondition(String transmissionCondition) {
        validateCondition(transmissionCondition, "transmissionCondition");
        this.transmissionCondition = transmissionCondition;
        markAsUpdated();
    }

    public void setSuspensionCondition(String suspensionCondition) {
        validateCondition(suspensionCondition, "suspensionCondition");
        this.suspensionCondition = suspensionCondition;
        markAsUpdated();
    }

    public void setBrakeCondition(String brakeCondition) {
        validateCondition(brakeCondition, "brakeCondition");
        this.brakeCondition = brakeCondition;
        markAsUpdated();
    }

    public void setOilLeaks(boolean oilLeaks) {
        this.oilLeaks = oilLeaks;
        markAsUpdated();
    }

    public void setWaterLeaks(boolean waterLeaks) {
        this.waterLeaks = waterLeaks;
        markAsUpdated();
    }

    public void setTimingBelt(boolean timingBelt) {
        this.timingBelt = timingBelt;
        markAsUpdated();
    }

    public void setBatteryCondition(String batteryCondition) {
        validateCondition(batteryCondition, "batteryCondition");
        this.batteryCondition = batteryCondition;
        markAsUpdated();
    }

    public void setTiresCondition(String tiresCondition) {
        validateCondition(tiresCondition, "tiresCondition");
        this.tiresCondition = tiresCondition;
        markAsUpdated();
    }

    public void setUnevenWear(boolean unevenWear) {
        this.unevenWear = unevenWear;
        markAsUpdated();
    }

    public void setLowTread(boolean lowTread) {
        this.lowTread = lowTread;
        markAsUpdated();
    }

    public void setSeatsCondition(String seatsCondition) {
        validateCondition(seatsCondition, "seatsCondition");
        this.seatsCondition = seatsCondition;
        markAsUpdated();
    }

    public void setDashboardCondition(String dashboardCondition) {
        validateCondition(dashboardCondition, "dashboardCondition");
        this.dashboardCondition = dashboardCondition;
        markAsUpdated();
    }

    public void setElectronicsCondition(String electronicsCondition) {
        validateCondition(electronicsCondition, "electronicsCondition");
        this.electronicsCondition = electronicsCondition;
        markAsUpdated();
    }

    public void setSeatDamage(boolean seatDamage) {
        this.seatDamage = seatDamage;
        markAsUpdated();
    }

    public void setDoorPanelDamage(boolean doorPanelDamage) {
        this.doorPanelDamage = doorPanelDamage;
        markAsUpdated();
    }

    public void setSteeringWheelWear(boolean steeringWheelWear) {
        this.steeringWheelWear = steeringWheelWear;
        markAsUpdated();
    }

    public void setCrvlPresent(boolean crvlPresent) {
        this.crvlPresent = crvlPresent;
        markAsUpdated();
    }

    public void setManualPresent(boolean manualPresent) {
        this.manualPresent = manualPresent;
        markAsUpdated();
    }

    public void setSpareKeyPresent(boolean spareKeyPresent) {
        this.spareKeyPresent = spareKeyPresent;
        markAsUpdated();
    }

    public void setMaintenanceRecords(boolean maintenanceRecords) {
        this.maintenanceRecords = maintenanceRecords;
        markAsUpdated();
    }

    public void setMechanicalNotes(String mechanicalNotes) {
        this.mechanicalNotes = mechanicalNotes;
        markAsUpdated();
    }

    public void setAestheticNotes(String aestheticNotes) {
        this.aestheticNotes = aestheticNotes;
        markAsUpdated();
    }

    public void setDocumentationNotes(String documentationNotes) {
        this.documentationNotes = documentationNotes;
        markAsUpdated();
    }

    public void setConservationScore(Integer conservationScore) {
        this.conservationScore = conservationScore;
        markAsUpdated();
    }

    /**
     * Valida se uma condição é válida.
     *
     * @param condition condição a validar
     * @param fieldName nome do campo para erro
     * @throws IllegalArgumentException se a condição for inválida
     */
    private void validateCondition(String condition, String fieldName) {
        Objects.requireNonNull(condition, fieldName + " cannot be null");
        if (!condition.trim().isEmpty() &&
            !condition.equals("EXCELLENT") &&
            !condition.equals("GOOD") &&
            !condition.equals("FAIR") &&
            !condition.equals("POOR")) {
            throw new IllegalArgumentException(
                "Invalid " + fieldName + ": " + condition + ". Must be EXCELLENT, GOOD, FAIR, or POOR"
            );
        }
    }

    /**
     * Valida se a quantidade de reparos é válida.
     *
     * @param count quantidade de reparos
     * @param fieldName nome do campo para erro
     * @throws IllegalArgumentException se a quantidade for inválida
     */
    private void validateRepairCount(int count, String fieldName) {
        if (count < 0 || count > 10) {
            throw new IllegalArgumentException(fieldName + " must be between 0 and 10");
        }
    }

    @Override
    public boolean equals(Object o) {
        if (this == o) return true;
        if (o == null || getClass() != o.getClass()) return false;
        EvaluationChecklist that = (EvaluationChecklist) o;
        return checklistId.equals(that.checklistId);
    }

    @Override
    public int hashCode() {
        return checklistId.hashCode();
    }

    @Override
    public String toString() {
        return "EvaluationChecklist{" +
                "checklistId='" + checklistId + '\'' +
                ", evaluationId=" + evaluationId +
                ", conservationScore=" + conservationScore +
                ", blockingIssues=" + criticalIssues.size() +
                '}';
    }
}