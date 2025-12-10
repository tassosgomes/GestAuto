package com.gestauto.vehicleevaluation.infra.entity;

import jakarta.persistence.*;
import java.time.LocalDateTime;
import java.util.UUID;

/**
 * Entidade JPA para persistência de checklists de avaliação.
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

    // Exterior
    @Column(name = "exterior_paint_score")
    private Integer exteriorPaintScore;

    @Column(name = "exterior_paint_notes", columnDefinition = "TEXT")
    private String exteriorPaintNotes;

    @Column(name = "exterior_body_score")
    private Integer exteriorBodyScore;

    @Column(name = "exterior_body_notes", columnDefinition = "TEXT")
    private String exteriorBodyNotes;

    @Column(name = "exterior_glass_score")
    private Integer exteriorGlassScore;

    @Column(name = "exterior_glass_notes", columnDefinition = "TEXT")
    private String exteriorGlassNotes;

    @Column(name = "exterior_lights_score")
    private Integer exteriorLightsScore;

    @Column(name = "exterior_lights_notes", columnDefinition = "TEXT")
    private String exteriorLightsNotes;

    @Column(name = "exterior_tires_score")
    private Integer exteriorTiresScore;

    @Column(name = "exterior_tires_notes", columnDefinition = "TEXT")
    private String exteriorTiresNotes;

    // Interior
    @Column(name = "interior_seats_score")
    private Integer interiorSeatsScore;

    @Column(name = "interior_seats_notes", columnDefinition = "TEXT")
    private String interiorSeatsNotes;

    @Column(name = "interior_dashboard_score")
    private Integer interiorDashboardScore;

    @Column(name = "interior_dashboard_notes", columnDefinition = "TEXT")
    private String interiorDashboardNotes;

    @Column(name = "interior_carpet_score")
    private Integer interiorCarpetScore;

    @Column(name = "interior_carpet_notes", columnDefinition = "TEXT")
    private String interiorCarpetNotes;

    @Column(name = "interior_controls_score")
    private Integer interiorControlsScore;

    @Column(name = "interior_controls_notes", columnDefinition = "TEXT")
    private String interiorControlsNotes;

    @Column(name = "interior_air_conditioning_score")
    private Integer interiorAirConditioningScore;

    @Column(name = "interior_air_conditioning_notes", columnDefinition = "TEXT")
    private String interiorAirConditioningNotes;

    // Mechanical
    @Column(name = "mechanical_engine_score")
    private Integer mechanicalEngineScore;

    @Column(name = "mechanical_engine_notes", columnDefinition = "TEXT")
    private String mechanicalEngineNotes;

    @Column(name = "mechanical_transmission_score")
    private Integer mechanicalTransmissionScore;

    @Column(name = "mechanical_transmission_notes", columnDefinition = "TEXT")
    private String mechanicalTransmissionNotes;

    @Column(name = "mechanical_suspension_score")
    private Integer mechanicalSuspensionScore;

    @Column(name = "mechanical_suspension_notes", columnDefinition = "TEXT")
    private String mechanicalSuspensionNotes;

    @Column(name = "mechanical_brakes_score")
    private Integer mechanicalBrakesScore;

    @Column(name = "mechanical_brakes_notes", columnDefinition = "TEXT")
    private String mechanicalBrakesNotes;

    @Column(name = "mechanical_exhaust_score")
    private Integer mechanicalExhaustScore;

    @Column(name = "mechanical_exhaust_notes", columnDefinition = "TEXT")
    private String mechanicalExhaustNotes;

    // Safety and Electronics
    @Column(name = "safety_airbags_working")
    private Boolean safetyAirbagsWorking;

    @Column(name = "safety_airbags_notes", columnDefinition = "TEXT")
    private String safetyAirbagsNotes;

    @Column(name = "safety_abs_working")
    private Boolean safetyAbsWorking;

    @Column(name = "safety_abs_notes", columnDefinition = "TEXT")
    private String safetyAbsNotes;

    @Column(name = "safety_seatbelts_working")
    private Boolean safetySeatbeltsWorking;

    @Column(name = "safety_seatbelts_notes", columnDefinition = "TEXT")
    private String safetySeatbeltsNotes;

    @Column(name = "electronics_audio_working")
    private Boolean electronicsAudioWorking;

    @Column(name = "electronics_audio_notes", columnDefinition = "TEXT")
    private String electronicsAudioNotes;

    @Column(name = "electronics_gps_working")
    private Boolean electronicsGpsWorking;

    @Column(name = "electronics_gps_notes", columnDefinition = "TEXT")
    private String electronicsGpsNotes;

    @Column(name = "electronics_sensors_working")
    private Boolean electronicsSensorsWorking;

    @Column(name = "electronics_sensors_notes", columnDefinition = "TEXT")
    private String electronicsSensorsNotes;

    // Test Drive
    @Column(name = "test_drive_performed")
    private Boolean testDrivePerformed;

    @Column(name = "test_drive_score")
    private Integer testDriveScore;

    @Column(name = "test_drive_notes", columnDefinition = "TEXT")
    private String testDriveNotes;

    @Column(name = "overall_score")
    private Double overallScore;

    @Column(name = "evaluator_id", nullable = false, length = 50)
    private String evaluatorId;

    @Column(name = "created_at", nullable = false)
    private LocalDateTime createdAt;

    @Column(name = "completed_at")
    private LocalDateTime completedAt;

    // Construtores
    public EvaluationChecklistJpaEntity() {
        this.createdAt = LocalDateTime.now();
    }

    // Getters e Setters
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

    public Integer getExteriorPaintScore() {
        return exteriorPaintScore;
    }

    public void setExteriorPaintScore(Integer exteriorPaintScore) {
        this.exteriorPaintScore = exteriorPaintScore;
    }

    public String getExteriorPaintNotes() {
        return exteriorPaintNotes;
    }

    public void setExteriorPaintNotes(String exteriorPaintNotes) {
        this.exteriorPaintNotes = exteriorPaintNotes;
    }

    public Integer getExteriorBodyScore() {
        return exteriorBodyScore;
    }

    public void setExteriorBodyScore(Integer exteriorBodyScore) {
        this.exteriorBodyScore = exteriorBodyScore;
    }

    public String getExteriorBodyNotes() {
        return exteriorBodyNotes;
    }

    public void setExteriorBodyNotes(String exteriorBodyNotes) {
        this.exteriorBodyNotes = exteriorBodyNotes;
    }

    public Integer getExteriorGlassScore() {
        return exteriorGlassScore;
    }

    public void setExteriorGlassScore(Integer exteriorGlassScore) {
        this.exteriorGlassScore = exteriorGlassScore;
    }

    public String getExteriorGlassNotes() {
        return exteriorGlassNotes;
    }

    public void setExteriorGlassNotes(String exteriorGlassNotes) {
        this.exteriorGlassNotes = exteriorGlassNotes;
    }

    public Integer getExteriorLightsScore() {
        return exteriorLightsScore;
    }

    public void setExteriorLightsScore(Integer exteriorLightsScore) {
        this.exteriorLightsScore = exteriorLightsScore;
    }

    public String getExteriorLightsNotes() {
        return exteriorLightsNotes;
    }

    public void setExteriorLightsNotes(String exteriorLightsNotes) {
        this.exteriorLightsNotes = exteriorLightsNotes;
    }

    public Integer getExteriorTiresScore() {
        return exteriorTiresScore;
    }

    public void setExteriorTiresScore(Integer exteriorTiresScore) {
        this.exteriorTiresScore = exteriorTiresScore;
    }

    public String getExteriorTiresNotes() {
        return exteriorTiresNotes;
    }

    public void setExteriorTiresNotes(String exteriorTiresNotes) {
        this.exteriorTiresNotes = exteriorTiresNotes;
    }

    public Integer getInteriorSeatsScore() {
        return interiorSeatsScore;
    }

    public void setInteriorSeatsScore(Integer interiorSeatsScore) {
        this.interiorSeatsScore = interiorSeatsScore;
    }

    public String getInteriorSeatsNotes() {
        return interiorSeatsNotes;
    }

    public void setInteriorSeatsNotes(String interiorSeatsNotes) {
        this.interiorSeatsNotes = interiorSeatsNotes;
    }

    public Integer getInteriorDashboardScore() {
        return interiorDashboardScore;
    }

    public void setInteriorDashboardScore(Integer interiorDashboardScore) {
        this.interiorDashboardScore = interiorDashboardScore;
    }

    public String getInteriorDashboardNotes() {
        return interiorDashboardNotes;
    }

    public void setInteriorDashboardNotes(String interiorDashboardNotes) {
        this.interiorDashboardNotes = interiorDashboardNotes;
    }

    public Integer getInteriorCarpetScore() {
        return interiorCarpetScore;
    }

    public void setInteriorCarpetScore(Integer interiorCarpetScore) {
        this.interiorCarpetScore = interiorCarpetScore;
    }

    public String getInteriorCarpetNotes() {
        return interiorCarpetNotes;
    }

    public void setInteriorCarpetNotes(String interiorCarpetNotes) {
        this.interiorCarpetNotes = interiorCarpetNotes;
    }

    public Integer getInteriorControlsScore() {
        return interiorControlsScore;
    }

    public void setInteriorControlsScore(Integer interiorControlsScore) {
        this.interiorControlsScore = interiorControlsScore;
    }

    public String getInteriorControlsNotes() {
        return interiorControlsNotes;
    }

    public void setInteriorControlsNotes(String interiorControlsNotes) {
        this.interiorControlsNotes = interiorControlsNotes;
    }

    public Integer getInteriorAirConditioningScore() {
        return interiorAirConditioningScore;
    }

    public void setInteriorAirConditioningScore(Integer interiorAirConditioningScore) {
        this.interiorAirConditioningScore = interiorAirConditioningScore;
    }

    public String getInteriorAirConditioningNotes() {
        return interiorAirConditioningNotes;
    }

    public void setInteriorAirConditioningNotes(String interiorAirConditioningNotes) {
        this.interiorAirConditioningNotes = interiorAirConditioningNotes;
    }

    public Integer getMechanicalEngineScore() {
        return mechanicalEngineScore;
    }

    public void setMechanicalEngineScore(Integer mechanicalEngineScore) {
        this.mechanicalEngineScore = mechanicalEngineScore;
    }

    public String getMechanicalEngineNotes() {
        return mechanicalEngineNotes;
    }

    public void setMechanicalEngineNotes(String mechanicalEngineNotes) {
        this.mechanicalEngineNotes = mechanicalEngineNotes;
    }

    public Integer getMechanicalTransmissionScore() {
        return mechanicalTransmissionScore;
    }

    public void setMechanicalTransmissionScore(Integer mechanicalTransmissionScore) {
        this.mechanicalTransmissionScore = mechanicalTransmissionScore;
    }

    public String getMechanicalTransmissionNotes() {
        return mechanicalTransmissionNotes;
    }

    public void setMechanicalTransmissionNotes(String mechanicalTransmissionNotes) {
        this.mechanicalTransmissionNotes = mechanicalTransmissionNotes;
    }

    public Integer getMechanicalSuspensionScore() {
        return mechanicalSuspensionScore;
    }

    public void setMechanicalSuspensionScore(Integer mechanicalSuspensionScore) {
        this.mechanicalSuspensionScore = mechanicalSuspensionScore;
    }

    public String getMechanicalSuspensionNotes() {
        return mechanicalSuspensionNotes;
    }

    public void setMechanicalSuspensionNotes(String mechanicalSuspensionNotes) {
        this.mechanicalSuspensionNotes = mechanicalSuspensionNotes;
    }

    public Integer getMechanicalBrakesScore() {
        return mechanicalBrakesScore;
    }

    public void setMechanicalBrakesScore(Integer mechanicalBrakesScore) {
        this.mechanicalBrakesScore = mechanicalBrakesScore;
    }

    public String getMechanicalBrakesNotes() {
        return mechanicalBrakesNotes;
    }

    public void setMechanicalBrakesNotes(String mechanicalBrakesNotes) {
        this.mechanicalBrakesNotes = mechanicalBrakesNotes;
    }

    public Integer getMechanicalExhaustScore() {
        return mechanicalExhaustScore;
    }

    public void setMechanicalExhaustScore(Integer mechanicalExhaustScore) {
        this.mechanicalExhaustScore = mechanicalExhaustScore;
    }

    public String getMechanicalExhaustNotes() {
        return mechanicalExhaustNotes;
    }

    public void setMechanicalExhaustNotes(String mechanicalExhaustNotes) {
        this.mechanicalExhaustNotes = mechanicalExhaustNotes;
    }

    public Boolean getSafetyAirbagsWorking() {
        return safetyAirbagsWorking;
    }

    public void setSafetyAirbagsWorking(Boolean safetyAirbagsWorking) {
        this.safetyAirbagsWorking = safetyAirbagsWorking;
    }

    public String getSafetyAirbagsNotes() {
        return safetyAirbagsNotes;
    }

    public void setSafetyAirbagsNotes(String safetyAirbagsNotes) {
        this.safetyAirbagsNotes = safetyAirbagsNotes;
    }

    public Boolean getSafetyAbsWorking() {
        return safetyAbsWorking;
    }

    public void setSafetyAbsWorking(Boolean safetyAbsWorking) {
        this.safetyAbsWorking = safetyAbsWorking;
    }

    public String getSafetyAbsNotes() {
        return safetyAbsNotes;
    }

    public void setSafetyAbsNotes(String safetyAbsNotes) {
        this.safetyAbsNotes = safetyAbsNotes;
    }

    public Boolean getSafetySeatbeltsWorking() {
        return safetySeatbeltsWorking;
    }

    public void setSafetySeatbeltsWorking(Boolean safetySeatbeltsWorking) {
        this.safetySeatbeltsWorking = safetySeatbeltsWorking;
    }

    public String getSafetySeatbeltsNotes() {
        return safetySeatbeltsNotes;
    }

    public void setSafetySeatbeltsNotes(String safetySeatbeltsNotes) {
        this.safetySeatbeltsNotes = safetySeatbeltsNotes;
    }

    public Boolean getElectronicsAudioWorking() {
        return electronicsAudioWorking;
    }

    public void setElectronicsAudioWorking(Boolean electronicsAudioWorking) {
        this.electronicsAudioWorking = electronicsAudioWorking;
    }

    public String getElectronicsAudioNotes() {
        return electronicsAudioNotes;
    }

    public void setElectronicsAudioNotes(String electronicsAudioNotes) {
        this.electronicsAudioNotes = electronicsAudioNotes;
    }

    public Boolean getElectronicsGpsWorking() {
        return electronicsGpsWorking;
    }

    public void setElectronicsGpsWorking(Boolean electronicsGpsWorking) {
        this.electronicsGpsWorking = electronicsGpsWorking;
    }

    public String getElectronicsGpsNotes() {
        return electronicsGpsNotes;
    }

    public void setElectronicsGpsNotes(String electronicsGpsNotes) {
        this.electronicsGpsNotes = electronicsGpsNotes;
    }

    public Boolean getElectronicsSensorsWorking() {
        return electronicsSensorsWorking;
    }

    public void setElectronicsSensorsWorking(Boolean electronicsSensorsWorking) {
        this.electronicsSensorsWorking = electronicsSensorsWorking;
    }

    public String getElectronicsSensorsNotes() {
        return electronicsSensorsNotes;
    }

    public void setElectronicsSensorsNotes(String electronicsSensorsNotes) {
        this.electronicsSensorsNotes = electronicsSensorsNotes;
    }

    public Boolean getTestDrivePerformed() {
        return testDrivePerformed;
    }

    public void setTestDrivePerformed(Boolean testDrivePerformed) {
        this.testDrivePerformed = testDrivePerformed;
    }

    public Integer getTestDriveScore() {
        return testDriveScore;
    }

    public void setTestDriveScore(Integer testDriveScore) {
        this.testDriveScore = testDriveScore;
    }

    public String getTestDriveNotes() {
        return testDriveNotes;
    }

    public void setTestDriveNotes(String testDriveNotes) {
        this.testDriveNotes = testDriveNotes;
    }

    public Double getOverallScore() {
        return overallScore;
    }

    public void setOverallScore(Double overallScore) {
        this.overallScore = overallScore;
    }

    public String getEvaluatorId() {
        return evaluatorId;
    }

    public void setEvaluatorId(String evaluatorId) {
        this.evaluatorId = evaluatorId;
    }

    public LocalDateTime getCreatedAt() {
        return createdAt;
    }

    public void setCreatedAt(LocalDateTime createdAt) {
        this.createdAt = createdAt;
    }

    public LocalDateTime getCompletedAt() {
        return completedAt;
    }

    public void setCompletedAt(LocalDateTime completedAt) {
        this.completedAt = completedAt;
    }
}