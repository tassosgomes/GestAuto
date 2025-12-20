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

    // Penalty Constants for Score Calculation
    private static final int RUST_PENALTY = 15;
    private static final int DEEP_SCRATCHES_PENALTY = 10;
    private static final int LARGE_DENTS_PENALTY = 20;
    private static final int HEAVY_BODYWORK_PENALTY = 25;
    private static final int DOOR_REPAIR_PENALTY = 5;
    private static final int FENDER_REPAIR_PENALTY = 4;
    private static final int HOOD_REPAIR_PENALTY = 8;
    private static final int TRUNK_REPAIR_PENALTY = 6;
    private static final int OIL_LEAKS_PENALTY = 15;
    private static final int WATER_LEAKS_PENALTY = 10;
    private static final int TIMING_BELT_MISSING_PENALTY = 8;
    private static final int UNEVEN_WEAR_PENALTY = 10;
    private static final int LOW_TREAD_PENALTY = 12;
    private static final int SEAT_DAMAGE_PENALTY = 10;
    private static final int DOOR_PANEL_DAMAGE_PENALTY = 5;
    private static final int STEERING_WHEEL_WEAR_PENALTY = 3;
    private static final int MISSING_CRVL_PENALTY = 25;
    private static final int MISSING_MANUAL_PENALTY = 5;
    private static final int MISSING_SPARE_KEY_PENALTY = 5;
    private static final int MISSING_MAINTENANCE_RECORDS_PENALTY = 3;

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
    private LocalDateTime createdAt;
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
        this.timingBelt = true;
        this.unevenWear = false;
        this.lowTread = false;
        this.seatDamage = false;
        this.doorPanelDamage = false;
        this.steeringWheelWear = false;
        this.crvlPresent = false;
        this.manualPresent = true;
        this.spareKeyPresent = true;
        this.maintenanceRecords = true;
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
     * Reidrata checklist a partir do estado persistido.
     */
    public static EvaluationChecklist restore(String checklistId, EvaluationId evaluationId,
                                              String bodyCondition, String paintCondition, boolean rustPresence,
                                              boolean lightScratches, boolean deepScratches, boolean smallDents,
                                              boolean largeDents, int doorRepairs, int fenderRepairs, int hoodRepairs,
                                              int trunkRepairs, boolean heavyBodywork, String engineCondition,
                                              String transmissionCondition, String suspensionCondition, String brakeCondition,
                                              boolean oilLeaks, boolean waterLeaks, boolean timingBelt,
                                              String batteryCondition, String tiresCondition, boolean unevenWear,
                                              boolean lowTread, String seatsCondition, String dashboardCondition,
                                              String electronicsCondition, boolean seatDamage, boolean doorPanelDamage,
                                              boolean steeringWheelWear, boolean crvlPresent, boolean manualPresent,
                                              boolean spareKeyPresent, boolean maintenanceRecords, String mechanicalNotes,
                                              String aestheticNotes, String documentationNotes, List<String> criticalIssues,
                                              Integer conservationScore, LocalDateTime createdAt, LocalDateTime updatedAt) {
        EvaluationChecklist checklist = new EvaluationChecklist(checklistId, evaluationId);
        checklist.bodyCondition = bodyCondition;
        checklist.paintCondition = paintCondition;
        checklist.rustPresence = rustPresence;
        checklist.lightScratches = lightScratches;
        checklist.deepScratches = deepScratches;
        checklist.smallDents = smallDents;
        checklist.largeDents = largeDents;
        checklist.doorRepairs = doorRepairs;
        checklist.fenderRepairs = fenderRepairs;
        checklist.hoodRepairs = hoodRepairs;
        checklist.trunkRepairs = trunkRepairs;
        checklist.heavyBodywork = heavyBodywork;
        checklist.engineCondition = engineCondition;
        checklist.transmissionCondition = transmissionCondition;
        checklist.suspensionCondition = suspensionCondition;
        checklist.brakeCondition = brakeCondition;
        checklist.oilLeaks = oilLeaks;
        checklist.waterLeaks = waterLeaks;
        checklist.timingBelt = timingBelt;
        checklist.batteryCondition = batteryCondition;
        checklist.tiresCondition = tiresCondition;
        checklist.unevenWear = unevenWear;
        checklist.lowTread = lowTread;
        checklist.seatsCondition = seatsCondition;
        checklist.dashboardCondition = dashboardCondition;
        checklist.electronicsCondition = electronicsCondition;
        checklist.seatDamage = seatDamage;
        checklist.doorPanelDamage = doorPanelDamage;
        checklist.steeringWheelWear = steeringWheelWear;
        checklist.crvlPresent = crvlPresent;
        checklist.manualPresent = manualPresent;
        checklist.spareKeyPresent = spareKeyPresent;
        checklist.maintenanceRecords = maintenanceRecords;
        checklist.mechanicalNotes = mechanicalNotes;
        checklist.aestheticNotes = aestheticNotes;
        checklist.documentationNotes = documentationNotes;
        checklist.criticalIssues.clear();
        if (criticalIssues != null) {
            checklist.criticalIssues.addAll(criticalIssues);
        }
        checklist.conservationScore = conservationScore;
        checklist.updatedAt = updatedAt != null ? updatedAt : checklist.updatedAt;
        checklist.createdAt = createdAt != null ? createdAt : checklist.createdAt;
        return checklist;
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

        if (rustPresence) penalty += RUST_PENALTY;
        if (deepScratches) penalty += DEEP_SCRATCHES_PENALTY;
        if (largeDents) penalty += LARGE_DENTS_PENALTY;
        if (heavyBodywork) penalty += HEAVY_BODYWORK_PENALTY;

        penalty += doorRepairs * DOOR_REPAIR_PENALTY;
        penalty += fenderRepairs * FENDER_REPAIR_PENALTY;
        penalty += hoodRepairs * HOOD_REPAIR_PENALTY;
        penalty += trunkRepairs * TRUNK_REPAIR_PENALTY;

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

        if (oilLeaks) penalty += OIL_LEAKS_PENALTY;
        if (waterLeaks) penalty += WATER_LEAKS_PENALTY;
        if (!timingBelt) penalty += TIMING_BELT_MISSING_PENALTY;
        if (batteryCondition.equals("POOR")) penalty += 10;

        return penalty;
    }

    private int getTiresPenalty() {
        int penalty = 0;

        if (tiresCondition.equals("POOR")) penalty += 15;
        else if (tiresCondition.equals("FAIR")) penalty += 8;

        if (unevenWear) penalty += UNEVEN_WEAR_PENALTY;
        if (lowTread) penalty += LOW_TREAD_PENALTY;

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

        if (seatDamage) penalty += SEAT_DAMAGE_PENALTY;
        if (doorPanelDamage) penalty += DOOR_PANEL_DAMAGE_PENALTY;
        if (steeringWheelWear) penalty += STEERING_WHEEL_WEAR_PENALTY;

        return penalty;
    }

    private int getDocumentationPenalty() {
        int penalty = 0;

        if (!crvlPresent) penalty += MISSING_CRVL_PENALTY;
        if (!manualPresent) penalty += MISSING_MANUAL_PENALTY;
        if (!spareKeyPresent) penalty += MISSING_SPARE_KEY_PENALTY;
        if (!maintenanceRecords) penalty += MISSING_MAINTENANCE_RECORDS_PENALTY;

        return penalty;
    }

    /**
     * Gera um resumo automático dos problemas identificados no checklist.
     *
     * @return String formatada com o resumo completo
     */
    public String generateSummary() {
        StringBuilder summary = new StringBuilder();
        
        summary.append("=== RESUMO DO CHECKLIST TÉCNICO ===\n\n");

        // Score de conservação
        int score = calculateScore();
        summary.append(String.format("Score de Conservação: %d/100 ", score));
        if (score >= 80) {
            summary.append("(EXCELENTE)\n");
        } else if (score >= 60) {
            summary.append("(BOM)\n");
        } else if (score >= 40) {
            summary.append("(REGULAR)\n");
        } else {
            summary.append("(RUIM)\n");
        }
        summary.append("\n");

        // Problemas críticos
        if (hasBlockingIssues()) {
            summary.append("⚠️ PROBLEMAS CRÍTICOS BLOQUEANTES:\n");
            criticalIssues.forEach(issue -> summary.append("  - ").append(issue).append("\n"));
            summary.append("\n");
        }

        // Resumo por seção
        summary.append("--- LATARIA E PINTURA ---\n");
        summary.append(String.format("Condição da Lataria: %s\n", bodyCondition));
        summary.append(String.format("Condição da Pintura: %s\n", paintCondition));
        if (rustPresence) summary.append("⚠️ Ferrugem detectada\n");
        if (deepScratches) summary.append("⚠️ Arranhões profundos\n");
        if (largeDents) summary.append("⚠️ Amassados grandes\n");
        if (heavyBodywork) summary.append("⚠️ Funilaria pesada realizada\n");
        if (doorRepairs > 0) summary.append(String.format("  %d reparo(s) em portas\n", doorRepairs));
        summary.append("\n");

        summary.append("--- MECÂNICA ---\n");
        summary.append(String.format("Motor: %s\n", engineCondition));
        summary.append(String.format("Transmissão: %s\n", transmissionCondition));
        summary.append(String.format("Suspensão: %s\n", suspensionCondition));
        summary.append(String.format("Freios: %s\n", brakeCondition));
        if (oilLeaks) summary.append("⚠️ Vazamento de óleo\n");
        if (waterLeaks) summary.append("⚠️ Vazamento de água\n");
        if (!timingBelt) summary.append("⚠️ Correia dentada necessita atenção\n");
        summary.append("\n");

        summary.append("--- PNEUS ---\n");
        summary.append(String.format("Condição: %s\n", tiresCondition));
        if (unevenWear) summary.append("⚠️ Desgaste irregular\n");
        if (lowTread) summary.append("⚠️ Banda de rodagem baixa\n");
        summary.append("\n");

        summary.append("--- INTERIOR ---\n");
        summary.append(String.format("Bancos: %s\n", seatsCondition));
        summary.append(String.format("Painel: %s\n", dashboardCondition));
        summary.append(String.format("Eletrônica: %s\n", electronicsCondition));
        if (seatDamage) summary.append("⚠️ Danos nos bancos\n");
        if (doorPanelDamage) summary.append("⚠️ Danos nos forros das portas\n");
        summary.append("\n");

        summary.append("--- DOCUMENTAÇÃO ---\n");
        summary.append(String.format("CRLV: %s\n", crvlPresent ? "✓ Presente" : "✗ Ausente"));
        summary.append(String.format("Manual: %s\n", manualPresent ? "✓ Presente" : "✗ Ausente"));
        summary.append(String.format("Chave Reserva: %s\n", spareKeyPresent ? "✓ Presente" : "✗ Ausente"));
        summary.append(String.format("Histórico Manutenção: %s\n", maintenanceRecords ? "✓ Presente" : "✗ Ausente"));

        return summary.toString();
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