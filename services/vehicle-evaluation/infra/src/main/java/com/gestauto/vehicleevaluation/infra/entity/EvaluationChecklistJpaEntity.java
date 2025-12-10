package com.gestauto.vehicleevaluation.infra.entity;

import jakarta.persistence.CollectionTable;
import jakarta.persistence.Column;
import jakarta.persistence.ElementCollection;
import jakarta.persistence.Entity;
import jakarta.persistence.FetchType;
import jakarta.persistence.Id;
import jakarta.persistence.JoinColumn;
import jakarta.persistence.OneToOne;
import jakarta.persistence.PreUpdate;
import jakarta.persistence.Table;
import java.time.LocalDateTime;
import java.util.ArrayList;
import java.util.List;
import java.util.UUID;

/**
 * Entidade JPA alinhada ao Checklist do domínio.
 */
@Entity
@Table(name = "evaluation_checklists", schema = "vehicle_evaluation")
public class EvaluationChecklistJpaEntity {

    @Id
    @Column(name = "checklist_id", columnDefinition = "UUID")
    private UUID checklistId;

    @OneToOne(fetch = FetchType.LAZY)
    @JoinColumn(name = "evaluation_id", nullable = false)
    private VehicleEvaluationJpaEntity evaluation;

    @Column(name = "body_condition", nullable = false, length = 20)
    private String bodyCondition;

    @Column(name = "paint_condition", nullable = false, length = 20)
    private String paintCondition;

    @Column(name = "rust_presence")
    private boolean rustPresence;

    @Column(name = "light_scratches")
    private boolean lightScratches;

    @Column(name = "deep_scratches")
    private boolean deepScratches;

    @Column(name = "small_dents")
    private boolean smallDents;

    @Column(name = "large_dents")
    private boolean largeDents;

    @Column(name = "door_repairs")
    private int doorRepairs;

    @Column(name = "fender_repairs")
    private int fenderRepairs;

    @Column(name = "hood_repairs")
    private int hoodRepairs;

    @Column(name = "trunk_repairs")
    private int trunkRepairs;

    @Column(name = "heavy_bodywork")
    private boolean heavyBodywork;

    @Column(name = "engine_condition", nullable = false, length = 20)
    private String engineCondition;

    @Column(name = "transmission_condition", nullable = false, length = 20)
    private String transmissionCondition;

    @Column(name = "suspension_condition", nullable = false, length = 20)
    private String suspensionCondition;

    @Column(name = "brake_condition", nullable = false, length = 20)
    private String brakeCondition;

    @Column(name = "oil_leaks")
    private boolean oilLeaks;

    @Column(name = "water_leaks")
    private boolean waterLeaks;

    @Column(name = "timing_belt")
    private boolean timingBelt;

    @Column(name = "battery_condition", nullable = false, length = 20)
    private String batteryCondition;

    @Column(name = "tires_condition", nullable = false, length = 20)
    private String tiresCondition;

    @Column(name = "uneven_wear")
    private boolean unevenWear;

    @Column(name = "low_tread")
    private boolean lowTread;

    @Column(name = "seats_condition", nullable = false, length = 20)
    private String seatsCondition;

    @Column(name = "dashboard_condition", nullable = false, length = 20)
    private String dashboardCondition;

    @Column(name = "electronics_condition", nullable = false, length = 20)
    private String electronicsCondition;

    @Column(name = "seat_damage")
    private boolean seatDamage;

    @Column(name = "door_panel_damage")
    private boolean doorPanelDamage;

    @Column(name = "steering_wheel_wear")
    private boolean steeringWheelWear;

    @Column(name = "crvl_present")
    private boolean crvlPresent;

    @Column(name = "manual_present")
    private boolean manualPresent;

    @Column(name = "spare_key_present")
    private boolean spareKeyPresent;

    @Column(name = "maintenance_records")
    private boolean maintenanceRecords;

    @Column(name = "mechanical_notes", columnDefinition = "TEXT")
    private String mechanicalNotes;

    @Column(name = "aesthetic_notes", columnDefinition = "TEXT")
    private String aestheticNotes;

    @Column(name = "documentation_notes", columnDefinition = "TEXT")
    private String documentationNotes;

    @ElementCollection
    @CollectionTable(name = "checklist_critical_issues", schema = "vehicle_evaluation",
        joinColumns = @JoinColumn(name = "evaluation_id"))
    @Column(name = "issue", columnDefinition = "TEXT")
    private List<String> criticalIssues = new ArrayList<>();

    @Column(name = "conservation_score")
    private Integer conservationScore;

    @Column(name = "created_at", nullable = false)
    private LocalDateTime createdAt;

    @Column(name = "updated_at", nullable = false)
    private LocalDateTime updatedAt;

    public EvaluationChecklistJpaEntity() {
        this.createdAt = LocalDateTime.now();
        this.updatedAt = LocalDateTime.now();
    }

    @PreUpdate
    protected void onUpdate() {
        this.updatedAt = LocalDateTime.now();
    }

    public UUID getChecklistId() {
        return checklistId;
    }

    public void setChecklistId(UUID checklistId) {
        this.checklistId = checklistId;
    }

    public VehicleEvaluationJpaEntity getEvaluation() {
        return evaluation;
    }

    public void setEvaluation(VehicleEvaluationJpaEntity evaluation) {
        this.evaluation = evaluation;
    }

    public String getBodyCondition() {
        return bodyCondition;
    }

    public void setBodyCondition(String bodyCondition) {
        this.bodyCondition = bodyCondition;
    }

    public String getPaintCondition() {
        return paintCondition;
    }

    public void setPaintCondition(String paintCondition) {
        this.paintCondition = paintCondition;
    }

    public boolean isRustPresence() {
        return rustPresence;
    }

    public void setRustPresence(boolean rustPresence) {
        this.rustPresence = rustPresence;
    }

    public boolean isLightScratches() {
        return lightScratches;
    }

    public void setLightScratches(boolean lightScratches) {
        this.lightScratches = lightScratches;
    }

    public boolean isDeepScratches() {
        return deepScratches;
    }

    public void setDeepScratches(boolean deepScratches) {
        this.deepScratches = deepScratches;
    }

    public boolean isSmallDents() {
        return smallDents;
    }

    public void setSmallDents(boolean smallDents) {
        this.smallDents = smallDents;
    }

    public boolean isLargeDents() {
        return largeDents;
    }

    public void setLargeDents(boolean largeDents) {
        this.largeDents = largeDents;
    }

    public int getDoorRepairs() {
        return doorRepairs;
    }

    public void setDoorRepairs(int doorRepairs) {
        this.doorRepairs = doorRepairs;
    }

    public int getFenderRepairs() {
        return fenderRepairs;
    }

    public void setFenderRepairs(int fenderRepairs) {
        this.fenderRepairs = fenderRepairs;
    }

    public int getHoodRepairs() {
        return hoodRepairs;
    }

    public void setHoodRepairs(int hoodRepairs) {
        this.hoodRepairs = hoodRepairs;
    }

    public int getTrunkRepairs() {
        return trunkRepairs;
    }

    public void setTrunkRepairs(int trunkRepairs) {
        this.trunkRepairs = trunkRepairs;
    }

    public boolean isHeavyBodywork() {
        return heavyBodywork;
    }

    public void setHeavyBodywork(boolean heavyBodywork) {
        this.heavyBodywork = heavyBodywork;
    }

    public String getEngineCondition() {
        return engineCondition;
    }

    public void setEngineCondition(String engineCondition) {
        this.engineCondition = engineCondition;
    }

    public String getTransmissionCondition() {
        return transmissionCondition;
    }

    public void setTransmissionCondition(String transmissionCondition) {
        this.transmissionCondition = transmissionCondition;
    }

    public String getSuspensionCondition() {
        return suspensionCondition;
    }

    public void setSuspensionCondition(String suspensionCondition) {
        this.suspensionCondition = suspensionCondition;
    }

    public String getBrakeCondition() {
        return brakeCondition;
    }

    public void setBrakeCondition(String brakeCondition) {
        this.brakeCondition = brakeCondition;
    }

    public boolean isOilLeaks() {
        return oilLeaks;
    }

    public void setOilLeaks(boolean oilLeaks) {
        this.oilLeaks = oilLeaks;
    }

    public boolean isWaterLeaks() {
        return waterLeaks;
    }

    public void setWaterLeaks(boolean waterLeaks) {
        this.waterLeaks = waterLeaks;
    }

    public boolean isTimingBelt() {
        return timingBelt;
    }

    public void setTimingBelt(boolean timingBelt) {
        this.timingBelt = timingBelt;
    }

    public String getBatteryCondition() {
        return batteryCondition;
    }

    public void setBatteryCondition(String batteryCondition) {
        this.batteryCondition = batteryCondition;
    }

    public String getTiresCondition() {
        return tiresCondition;
    }

    public void setTiresCondition(String tiresCondition) {
        this.tiresCondition = tiresCondition;
    }

    public boolean isUnevenWear() {
        return unevenWear;
    }

    public void setUnevenWear(boolean unevenWear) {
        this.unevenWear = unevenWear;
    }

    public boolean isLowTread() {
        return lowTread;
    }

    public void setLowTread(boolean lowTread) {
        this.lowTread = lowTread;
    }

    public String getSeatsCondition() {
        return seatsCondition;
    }

    public void setSeatsCondition(String seatsCondition) {
        this.seatsCondition = seatsCondition;
    }

    public String getDashboardCondition() {
        return dashboardCondition;
    }

    public void setDashboardCondition(String dashboardCondition) {
        this.dashboardCondition = dashboardCondition;
    }

    public String getElectronicsCondition() {
        return electronicsCondition;
    }

    public void setElectronicsCondition(String electronicsCondition) {
        this.electronicsCondition = electronicsCondition;
    }

    public boolean isSeatDamage() {
        return seatDamage;
    }

    public void setSeatDamage(boolean seatDamage) {
        this.seatDamage = seatDamage;
    }

    public boolean isDoorPanelDamage() {
        return doorPanelDamage;
    }

    public void setDoorPanelDamage(boolean doorPanelDamage) {
        this.doorPanelDamage = doorPanelDamage;
    }

    public boolean isSteeringWheelWear() {
        return steeringWheelWear;
    }

    public void setSteeringWheelWear(boolean steeringWheelWear) {
        this.steeringWheelWear = steeringWheelWear;
    }

    public boolean isCrvlPresent() {
        return crvlPresent;
    }

    public void setCrvlPresent(boolean crvlPresent) {
        this.crvlPresent = crvlPresent;
    }

    public boolean isManualPresent() {
        return manualPresent;
    }

    public void setManualPresent(boolean manualPresent) {
        this.manualPresent = manualPresent;
    }

    public boolean isSpareKeyPresent() {
        return spareKeyPresent;
    }

    public void setSpareKeyPresent(boolean spareKeyPresent) {
        this.spareKeyPresent = spareKeyPresent;
    }

    public boolean isMaintenanceRecords() {
        return maintenanceRecords;
    }

    public void setMaintenanceRecords(boolean maintenanceRecords) {
        this.maintenanceRecords = maintenanceRecords;
    }

    public String getMechanicalNotes() {
        return mechanicalNotes;
    }

    public void setMechanicalNotes(String mechanicalNotes) {
        this.mechanicalNotes = mechanicalNotes;
    }

    public String getAestheticNotes() {
        return aestheticNotes;
    }

    public void setAestheticNotes(String aestheticNotes) {
        this.aestheticNotes = aestheticNotes;
    }

    public String getDocumentationNotes() {
        return documentationNotes;
    }

    public void setDocumentationNotes(String documentationNotes) {
        this.documentationNotes = documentationNotes;
    }

    public List<String> getCriticalIssues() {
        return criticalIssues;
    }

    public void setCriticalIssues(List<String> criticalIssues) {
        this.criticalIssues = criticalIssues;
    }

    public Integer getConservationScore() {
        return conservationScore;
    }

    public void setConservationScore(Integer conservationScore) {
        this.conservationScore = conservationScore;
    }

    public LocalDateTime getCreatedAt() {
        return createdAt;
    }

    public void setCreatedAt(LocalDateTime createdAt) {
        this.createdAt = createdAt;
    }

    public LocalDateTime getUpdatedAt() {
        return updatedAt;
    }

    public void setUpdatedAt(LocalDateTime updatedAt) {
        this.updatedAt = updatedAt;
    }
}