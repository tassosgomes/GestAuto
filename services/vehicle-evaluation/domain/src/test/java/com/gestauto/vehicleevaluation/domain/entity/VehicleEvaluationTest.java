package com.gestauto.vehicleevaluation.domain.entity;

import com.gestauto.vehicleevaluation.domain.enums.EvaluationStatus;
import com.gestauto.vehicleevaluation.domain.enums.FuelType;
import com.gestauto.vehicleevaluation.domain.enums.PhotoType;
import com.gestauto.vehicleevaluation.domain.event.DomainEvent;
import com.gestauto.vehicleevaluation.domain.event.EvaluationApprovedEvent;
import com.gestauto.vehicleevaluation.domain.event.EvaluationCreatedEvent;
import com.gestauto.vehicleevaluation.domain.event.EvaluationSubmittedEvent;
import com.gestauto.vehicleevaluation.domain.event.VehicleEvaluationCompletedEvent;
import com.gestauto.vehicleevaluation.domain.exception.InvalidEvaluationStatusException;
import com.gestauto.vehicleevaluation.domain.value.EvaluationId;
import com.gestauto.vehicleevaluation.domain.value.Money;
import com.gestauto.vehicleevaluation.domain.value.Plate;
import com.gestauto.vehicleevaluation.domain.value.VehicleInfo;
import java.math.BigDecimal;
import java.time.LocalDateTime;
import java.util.List;
import java.util.UUID;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.*;

@DisplayName("VehicleEvaluation Domain Entity Tests")
class VehicleEvaluationTest {

    @Test
    @DisplayName("Should create evaluation with DRAFT status and raise created event")
    void shouldCreateEvaluationAndRaiseCreatedEvent() {
        VehicleEvaluation evaluation = VehicleEvaluation.create(
            Plate.of("ABC1234"),
            "12345678901",
            VehicleInfo.of("Volkswagen", "Gol", "1.0", 2022, 2022, "Preto", FuelType.FLEX),
            Money.of(new BigDecimal("50000.00")),
            "evaluator-1"
        );

        assertNotNull(evaluation.getId());
        assertEquals(EvaluationStatus.DRAFT, evaluation.getStatus());

        List<DomainEvent> events = evaluation.getDomainEvents();
        assertEquals(1, events.size());
        assertTrue(events.get(0) instanceof EvaluationCreatedEvent);

        // getDomainEvents clears the list
        assertTrue(evaluation.getDomainEvents().isEmpty());
    }

    @Test
    @DisplayName("Should not submit evaluation without required data")
    void shouldNotSubmitWithoutRequiredData() {
        VehicleEvaluation evaluation = newDraftEvaluation();

        IllegalStateException ex = assertThrows(IllegalStateException.class, evaluation::submitForApproval);
        assertTrue(ex.getMessage().contains("without photos"));
    }

    @Test
    @DisplayName("Should submit evaluation when checklist, photos and final value are present")
    void shouldSubmitForApprovalWhenRequiredDataPresent() {
        VehicleEvaluation evaluation = newDraftEvaluation();

        evaluation.addPhoto(EvaluationPhoto.create(
            evaluation.getId(),
            PhotoType.EXTERIOR_FRONT,
            "front.jpg",
            "/photos/front.jpg",
            200_000,
            "image/jpeg",
            "http://example.test/front.jpg",
            null
        ));

        EvaluationChecklist checklist = EvaluationChecklist.create(evaluation.getId());
        completeChecklist(checklist);
        evaluation.updateChecklist(checklist);

        evaluation.setFinalValue(Money.of(new BigDecimal("10000.00")));

        evaluation.submitForApproval();

        assertEquals(EvaluationStatus.PENDING_APPROVAL, evaluation.getStatus());
        assertNotNull(evaluation.getSubmittedAt());

        List<DomainEvent> events = evaluation.getDomainEvents();
        assertTrue(events.stream().anyMatch(EvaluationSubmittedEvent.class::isInstance));
    }

    @Test
    @DisplayName("Should approve evaluation, generate token and emit approval/completed events")
    void shouldApproveAndGenerateTokenAndEvents() {
        VehicleEvaluation evaluation = newDraftEvaluation();

        evaluation.addPhoto(EvaluationPhoto.create(
            evaluation.getId(),
            PhotoType.EXTERIOR_FRONT,
            "front.jpg",
            "/photos/front.jpg",
            200_000,
            "image/jpeg",
            "http://example.test/front.jpg",
            null
        ));

        EvaluationChecklist checklist = EvaluationChecklist.create(evaluation.getId());
        completeChecklist(checklist);
        evaluation.updateChecklist(checklist);

        evaluation.setFinalValue(Money.of(new BigDecimal("10000.00")));
        evaluation.submitForApproval();

        evaluation.approve("approver-1", null);

        assertEquals(EvaluationStatus.APPROVED, evaluation.getStatus());
        assertNotNull(evaluation.getApprovedAt());
        assertNotNull(evaluation.getValidUntil());
        assertNotNull(evaluation.getValidationToken());
        assertTrue(evaluation.getValidationToken().startsWith("TOKEN-"));

        List<DomainEvent> events = evaluation.getDomainEvents();
        assertTrue(events.stream().anyMatch(EvaluationApprovedEvent.class::isInstance));
        assertTrue(events.stream().anyMatch(VehicleEvaluationCompletedEvent.class::isInstance));
    }

    @Test
    @DisplayName("Should reject evaluation only with non-empty reason")
    void shouldRejectWithNonEmptyReason() {
        VehicleEvaluation evaluation = newDraftEvaluation();

        evaluation.addPhoto(EvaluationPhoto.create(
            evaluation.getId(),
            PhotoType.EXTERIOR_FRONT,
            "front.jpg",
            "/photos/front.jpg",
            200_000,
            "image/jpeg",
            "http://example.test/front.jpg",
            null
        ));

        EvaluationChecklist checklist = EvaluationChecklist.create(evaluation.getId());
        completeChecklist(checklist);
        evaluation.updateChecklist(checklist);

        evaluation.setFinalValue(Money.of(new BigDecimal("10000.00")));
        evaluation.submitForApproval();

        assertThrows(IllegalArgumentException.class, () -> evaluation.reject("approver-1", "  "));

        evaluation.reject("approver-1", "Documento inconsistente");
        assertEquals(EvaluationStatus.REJECTED, evaluation.getStatus());
    }

    @Test
    @DisplayName("Should detect expired approved evaluation")
    void shouldDetectExpiredEvaluation() {
        EvaluationId id = EvaluationId.from(UUID.randomUUID());
        VehicleEvaluation evaluation = VehicleEvaluation.restore(
            id,
            Plate.of("ABC1234"),
            "12345678901",
            VehicleInfo.of("Volkswagen", "Gol", "1.0", 2022, 2022, "Preto", FuelType.FLEX),
            Money.of(new BigDecimal("50000.00")),
            EvaluationStatus.APPROVED,
            null,
            null,
            Money.of(new BigDecimal("100.00")),
            Money.of(new BigDecimal("100.00")),
            null,
            null,
            LocalDateTime.now().minusDays(1),
            LocalDateTime.now().minusDays(1),
            LocalDateTime.now().minusDays(1),
            LocalDateTime.now().minusDays(1),
            "evaluator-1",
            "approver-1",
            LocalDateTime.now().minusHours(1),
            "TOKEN-EXPIRED",
            null,
            null,
            null
        );

        assertTrue(evaluation.isExpired());
    }

    @Test
    @DisplayName("Should prevent editing when status is not editable")
    void shouldPreventEditsWhenNotEditable() {
        VehicleEvaluation evaluation = newDraftEvaluation();

        evaluation.cancel();
        assertEquals(EvaluationStatus.CANCELLED, evaluation.getStatus());

        assertThrows(InvalidEvaluationStatusException.class,
            () -> evaluation.setFinalValue(Money.of(new BigDecimal("10.00"))));
    }

    @Test
    @DisplayName("Should manage photos, depreciation items, and calculate evaluation values")
    void shouldManagePhotosDepreciationAndCalculateEvaluation() {
        VehicleEvaluation evaluation = newDraftEvaluation();

        EvaluationPhoto photo = EvaluationPhoto.create(
            evaluation.getId(),
            PhotoType.EXTERIOR_FRONT,
            "front.jpg",
            "/photos/front.jpg",
            200_000,
            "image/jpeg",
            "http://example.test/front.jpg",
            null
        );
        evaluation.addPhoto(photo);
        assertTrue(evaluation.hasRequiredPhotos(1));

        assertThrows(IllegalArgumentException.class, () -> evaluation.addPhoto(photo));

        EvaluationPhoto otherEvalPhoto = EvaluationPhoto.create(
            EvaluationId.generate(),
            PhotoType.EXTERIOR_REAR,
            "rear.jpg",
            "/photos/rear.jpg",
            200_000,
            "image/jpeg",
            "http://example.test/rear.jpg",
            null
        );
        assertThrows(IllegalArgumentException.class, () -> evaluation.addPhoto(otherEvalPhoto));

        evaluation.removePhoto(photo.getPhotoId());
        assertFalse(evaluation.hasRequiredPhotos(1));
        assertThrows(IllegalArgumentException.class, () -> evaluation.removePhoto("missing"));

        DepreciationItem depreciation = DepreciationItem.create(
            evaluation.getId(),
            "BODY",
            "Scratch",
            Money.of(new BigDecimal("100.00")),
            null,
            "evaluator-1"
        );
        evaluation.addDepreciationItem(depreciation);
        assertEquals(Money.of(new BigDecimal("100.00")), evaluation.getTotalDepreciation());

        assertThrows(IllegalArgumentException.class,
            () -> evaluation.calculateEvaluation(Money.of(new BigDecimal("1000.00")), 2.0));

        evaluation.calculateEvaluation(Money.of(new BigDecimal("1000.00")), 0.50);
        assertEquals(Money.of(new BigDecimal("500.00")), evaluation.getBaseValue());
        assertEquals(Money.of(new BigDecimal("400.00")), evaluation.getFinalValue());

        evaluation.setFipePrice(Money.of(new BigDecimal("1200.00")));
        evaluation.setBaseValue(Money.of(new BigDecimal("600.00")));
        evaluation.setFinalValue(Money.of(new BigDecimal("450.00")));
        evaluation.setObservations("ok");

        assertEquals("ok", evaluation.getObservations());
    }

    @Test
    @DisplayName("Should validate null args, mismatched IDs, and invalid operations")
    void shouldValidateArgumentsAndInvalidOperations() {
        VehicleEvaluation evaluation = newDraftEvaluation();

        assertThrows(NullPointerException.class, () -> evaluation.calculateEvaluation(null, 0.5));
        assertThrows(IllegalArgumentException.class,
            () -> evaluation.calculateEvaluation(Money.of(new BigDecimal("100.00")), -0.1));

        EvaluationChecklist otherChecklist = EvaluationChecklist.create(EvaluationId.generate());
        assertThrows(IllegalArgumentException.class, () -> evaluation.updateChecklist(otherChecklist));

        DepreciationItem otherDepreciation = DepreciationItem.create(
            EvaluationId.generate(),
            "BODY",
            "Scratch",
            Money.of(new BigDecimal("10.00")),
            null,
            "evaluator-1"
        );
        assertThrows(IllegalArgumentException.class, () -> evaluation.addDepreciationItem(otherDepreciation));

        evaluation.cancel();
        assertThrows(InvalidEvaluationStatusException.class, evaluation::submitForApproval);
        assertThrows(InvalidEvaluationStatusException.class, evaluation::cancel);
    }

    private static VehicleEvaluation newDraftEvaluation() {
        return VehicleEvaluation.create(
            Plate.of("ABC1234"),
            "12345678901",
            VehicleInfo.of("Volkswagen", "Gol", "1.0", 2022, 2022, "Preto", FuelType.FLEX),
            Money.of(new BigDecimal("50000.00")),
            "evaluator-1"
        );
    }

    private static void completeChecklist(EvaluationChecklist checklist) {
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
        checklist.setCrvlPresent(true);
    }
}
