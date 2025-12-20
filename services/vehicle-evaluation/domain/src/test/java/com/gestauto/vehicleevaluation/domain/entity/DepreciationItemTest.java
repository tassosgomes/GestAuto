package com.gestauto.vehicleevaluation.domain.entity;

import com.gestauto.vehicleevaluation.domain.value.EvaluationId;
import com.gestauto.vehicleevaluation.domain.value.Money;
import java.math.BigDecimal;
import java.time.LocalDateTime;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.*;

@DisplayName("DepreciationItem Entity Tests")
class DepreciationItemTest {

    @Test
    void shouldCreateAndClassifyByCategory() {
        EvaluationId evaluationId = EvaluationId.generate();

        DepreciationItem item = DepreciationItem.create(
            evaluationId,
            "BODY",
            "Scratch on door",
            Money.of(new BigDecimal("100.00")),
            "Needs repaint",
            "evaluator-1"
        );

        assertNotNull(item.getDepreciationId());
        assertEquals(evaluationId, item.getEvaluationId());
        assertTrue(item.isBodyDepreciation());
        assertFalse(item.isPaintDepreciation());
        assertTrue(item.getFormattedValue().startsWith("R$ "));
    }

    @Test
    void shouldRestoreUsingGivenCreatedAt() {
        EvaluationId evaluationId = EvaluationId.generate();
        LocalDateTime createdAt = LocalDateTime.now().minusDays(1);

        DepreciationItem item = DepreciationItem.restore(
            "dep-1",
            evaluationId,
            "PAINT",
            "Bumper repaint",
            Money.of(new BigDecimal("200.00")),
            null,
            "evaluator-1",
            createdAt
        );

        assertEquals("dep-1", item.getDepreciationId());
        assertEquals(createdAt, item.getCreatedAt());
        assertTrue(item.isPaintDepreciation());
    }

    @Test
    void shouldValidateInputs() {
        EvaluationId evaluationId = EvaluationId.generate();

        assertThrows(IllegalArgumentException.class, () -> DepreciationItem.create(
            evaluationId,
            "INVALID",
            "desc",
            Money.of(new BigDecimal("1.00")),
            null,
            "evaluator-1"
        ));

        assertThrows(IllegalArgumentException.class, () -> DepreciationItem.create(
            evaluationId,
            "BODY",
            " ",
            Money.of(new BigDecimal("1.00")),
            null,
            "evaluator-1"
        ));

        assertThrows(IllegalArgumentException.class, () -> DepreciationItem.create(
            evaluationId,
            "BODY",
            "desc",
            Money.ZERO,
            null,
            "evaluator-1"
        ));
    }

    @Test
    void shouldIdentifyDepreciationCategories() {
        EvaluationId evaluationId = EvaluationId.generate();

        DepreciationItem mechanical = DepreciationItem.create(
            evaluationId,
            "MECHANICAL",
            "Noise",
            Money.of(new BigDecimal("10.00")),
            null,
            "evaluator-1"
        );
        assertTrue(mechanical.isMechanicalDepreciation());

        DepreciationItem tires = DepreciationItem.create(
            evaluationId,
            "TIRES",
            "Worn tires",
            Money.of(new BigDecimal("10.00")),
            null,
            "evaluator-1"
        );
        assertTrue(tires.isTiresDepreciation());

        DepreciationItem interior = DepreciationItem.create(
            evaluationId,
            "INTERIOR",
            "Seat tear",
            Money.of(new BigDecimal("10.00")),
            null,
            "evaluator-1"
        );
        assertTrue(interior.isInteriorDepreciation());

        DepreciationItem docs = DepreciationItem.create(
            evaluationId,
            "DOCUMENTATION",
            "Missing doc",
            Money.of(new BigDecimal("10.00")),
            null,
            "evaluator-1"
        );
        assertTrue(docs.isDocumentationDepreciation());
    }
}
