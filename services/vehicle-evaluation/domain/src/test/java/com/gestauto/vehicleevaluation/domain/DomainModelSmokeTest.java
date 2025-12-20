package com.gestauto.vehicleevaluation.domain;

import com.gestauto.vehicleevaluation.domain.enums.ChecklistSection;
import com.gestauto.vehicleevaluation.domain.enums.Condition;
import com.gestauto.vehicleevaluation.domain.enums.EvaluationStatus;
import com.gestauto.vehicleevaluation.domain.enums.FuelType;
import com.gestauto.vehicleevaluation.domain.enums.PhotoType;
import com.gestauto.vehicleevaluation.domain.event.ChecklistCompletedEvent;
import com.gestauto.vehicleevaluation.domain.event.EvaluationApprovedEvent;
import com.gestauto.vehicleevaluation.domain.event.EvaluationCreatedEvent;
import com.gestauto.vehicleevaluation.domain.event.EvaluationRejectedEvent;
import com.gestauto.vehicleevaluation.domain.event.EvaluationSubmittedEvent;
import com.gestauto.vehicleevaluation.domain.event.ValuationCalculatedEvent;
import com.gestauto.vehicleevaluation.domain.event.VehicleEvaluationCompletedEvent;
import com.gestauto.vehicleevaluation.domain.exception.DomainException;
import com.gestauto.vehicleevaluation.domain.exception.EvaluationNotFoundException;
import com.gestauto.vehicleevaluation.domain.exception.InvalidEvaluationStatusException;
import com.gestauto.vehicleevaluation.domain.report.BrandStat;
import com.gestauto.vehicleevaluation.domain.report.EvaluationKpi;
import com.gestauto.vehicleevaluation.domain.report.MonthlyStat;
import com.gestauto.vehicleevaluation.domain.value.EvaluationId;
import com.gestauto.vehicleevaluation.domain.value.ImageUploadRequest;
import com.gestauto.vehicleevaluation.domain.value.ImageUploadResult;
import com.gestauto.vehicleevaluation.domain.value.Money;
import com.gestauto.vehicleevaluation.domain.value.UploadedPhoto;
import java.io.ByteArrayInputStream;
import java.math.BigDecimal;
import java.time.LocalDate;
import java.time.LocalDateTime;
import java.util.List;
import java.util.Map;
import java.util.UUID;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.*;

@DisplayName("Domain model smoke tests for enums/events/records/exceptions")
class DomainModelSmokeTest {

    @Test
    void shouldExerciseEnums() {
        assertNotNull(ChecklistSection.BODYWORK.getDescription());
        assertEquals(Condition.GOOD, Condition.fromString(null));
        assertEquals(Condition.GOOD, Condition.fromString("  "));
        assertEquals(Condition.EXCELLENT, Condition.fromString("excellent"));
        assertThrows(IllegalArgumentException.class, () -> Condition.fromString("nope"));

        assertTrue(EvaluationStatus.APPROVED.isFinal());
        assertTrue(EvaluationStatus.DRAFT.isEditable());
        assertTrue(EvaluationStatus.DRAFT.canBeSubmitted());
        assertTrue(EvaluationStatus.PENDING_APPROVAL.canBeApproved());
        assertTrue(EvaluationStatus.PENDING_APPROVAL.canBeRejected());
        assertFalse(EvaluationStatus.CANCELLED.isActive());

        assertTrue(PhotoType.EXTERIOR_FRONT.isExternal());
        assertTrue(PhotoType.INTERIOR_FRONT.isInternal());
        assertTrue(PhotoType.DASHBOARD_AC.isPanel());
        assertTrue(PhotoType.ENGINE_BAY.isEngine());
        assertTrue(PhotoType.TRUNK_OPEN.isTrunk());
        assertEquals(15, PhotoType.TOTAL_REQUIRED_PHOTOS);

        assertTrue(FuelType.DIESEL.isFossilFuel());
        assertTrue(FuelType.ETHANOL.isRenewable());
        assertTrue(FuelType.HYBRID.isFlexible());
    }

    @Test
    void shouldCreateAndReadEvents() {
        EvaluationCreatedEvent created = new EvaluationCreatedEvent("e1", "u1", "ABC1234", "VW", "Gol");
        assertEquals("EvaluationCreated", created.getEventType());
        assertNotNull(created.getOccurredAt());
        assertTrue(created.getEventData().contains("\"plate\":\"ABC1234\""));
        assertEquals("u1", created.getEvaluatorId());
        assertEquals("VW", created.getBrand());

        EvaluationSubmittedEvent submitted = new EvaluationSubmittedEvent("e1", "u1", "ABC1234", 1000.0, 2000.0);
        assertEquals("EvaluationSubmitted", submitted.getEventType());
        assertTrue(submitted.getEventData().contains("\"suggestedValue\""));
        assertEquals("ABC1234", submitted.getPlate());

        LocalDateTime validUntil = LocalDateTime.now().plusDays(1);
        EvaluationApprovedEvent approved = new EvaluationApprovedEvent("e1", "m1", "ABC1234", 3000.0, "TOKEN-1", validUntil);
        assertEquals("EvaluationApproved", approved.getEventType());
        assertTrue(approved.getEventData().contains("\"validationToken\":\"TOKEN-1\""));
        assertEquals("m1", approved.getApproverId());
        assertEquals(validUntil, approved.getValidUntil());

        EvaluationRejectedEvent rejected = new EvaluationRejectedEvent("e1", "m1", "ABC1234", null);
        assertEquals("EvaluationRejected", rejected.getEventType());
        assertTrue(rejected.getEventData().contains("rejectionReason"));
        EvaluationRejectedEvent rejectedWithReason = new EvaluationRejectedEvent("e1", "m1", "ABC1234", "x");
        assertTrue(rejectedWithReason.getEventData().contains("\"rejectionReason\":\"x\""));

        ChecklistCompletedEvent checklistCompleted = new ChecklistCompletedEvent(UUID.randomUUID(), 90, false, List.of("x"));
        assertEquals("ChecklistCompleted", checklistCompleted.getEventType());
        assertTrue(checklistCompleted.getEventData().contains("criticalIssuesCount"));
        assertNotNull(checklistCompleted.toString());

        ChecklistCompletedEvent checklistCompletedNulls = new ChecklistCompletedEvent(null, 10, true, null);
        assertNull(checklistCompletedNulls.getEvaluationId());
        assertTrue(checklistCompletedNulls.getEventData().contains("criticalIssuesCount\":0"));

        ValuationCalculatedEvent valuation = new ValuationCalculatedEvent(
            "e1",
            Money.of(new BigDecimal("100.00")),
            Money.of(new BigDecimal("10.00")),
            Money.of(new BigDecimal("90.00")),
            Money.of(new BigDecimal("95.00")),
            10.0,
            true
        );
        assertEquals("ValuationCalculated", valuation.getEventType());
        assertTrue(valuation.getEventData().contains("hasManualAdjustment"));
        assertNotNull(valuation.toString());
        assertTrue(valuation.hasManualAdjustment());

        VehicleEvaluationCompletedEvent completed = new VehicleEvaluationCompletedEvent(
            "e1",
            "ABC1234",
            "VW",
            "Gol",
            2022,
            9500.0,
            validUntil,
            Map.of("k", "v")
        );
        assertEquals("VehicleEvaluationCompleted", completed.getEventType());
        assertTrue(completed.getEventData().contains("\"year\":2022"));
        assertEquals("VW", completed.getBrand());
        assertNotNull(completed.getEvaluationData());
    }

    @Test
    void shouldExerciseRecordsAndExceptions() {
        BrandStat brandStat = new BrandStat("VW", 10, new BigDecimal("100.00"), new BigDecimal("50.00"));
        assertEquals("VW", brandStat.brand());

        EvaluationKpi kpi = new EvaluationKpi(10, new BigDecimal("50.00"), new BigDecimal("100.00"), new BigDecimal("1.50"));
        assertEquals(10, kpi.totalEvaluations());

        MonthlyStat monthlyStat = new MonthlyStat(LocalDate.of(2025, 1, 1), 2, new BigDecimal("200.00"));
        assertEquals(2, monthlyStat.evaluationsCount());

        EvaluationId id = EvaluationId.generate();
        assertEquals(id, EvaluationId.from(id.getValue()));
        assertEquals(id.getValueAsString(), EvaluationId.from(id.getValueAsString()).getValueAsString());
        assertThrows(IllegalArgumentException.class, () -> EvaluationId.from("not-a-uuid"));

        ImageUploadRequest request = new ImageUploadRequest(new ByteArrayInputStream(new byte[] {1, 2}), "a.jpg", "image/jpeg", 2);
        assertEquals("a.jpg", request.fileName());

        ImageUploadResult result = new ImageUploadResult(Map.of("EXTERIOR_FRONT", new UploadedPhoto("u", "t")), Map.of());
        assertTrue(result.errors().isEmpty());
        assertEquals("u", result.uploadedPhotos().get("EXTERIOR_FRONT").originalUrl());

        DomainException de = new DomainException("x", new RuntimeException("y"));
        assertEquals("x", de.getMessage());
        assertNotNull(de.getCause());

        EvaluationNotFoundException enf = new EvaluationNotFoundException("e1");
        assertEquals("e1", enf.getEvaluationId());

        InvalidEvaluationStatusException ies = new InvalidEvaluationStatusException(EvaluationStatus.DRAFT, "approve");
        assertEquals(EvaluationStatus.DRAFT, ies.getCurrentStatus());
        assertEquals("approve", ies.getOperation());

        InvalidEvaluationStatusException custom = new InvalidEvaluationStatusException(EvaluationStatus.DRAFT, "approve", "custom");
        assertEquals("custom", custom.getMessage());
    }
}
