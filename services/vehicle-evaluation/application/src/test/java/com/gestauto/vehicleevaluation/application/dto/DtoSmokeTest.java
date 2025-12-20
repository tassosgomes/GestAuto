package com.gestauto.vehicleevaluation.application.dto;

import com.gestauto.vehicleevaluation.domain.enums.EvaluationStatus;
import com.gestauto.vehicleevaluation.domain.value.Money;
import org.junit.jupiter.api.Test;

import java.math.BigDecimal;
import java.time.LocalDate;
import java.time.LocalDateTime;
import java.util.List;
import java.util.UUID;

import static org.assertj.core.api.Assertions.assertThat;

class DtoSmokeTest {

    @Test
    void dtoRecords_canBeConstructedAndAccessed() {
        UUID id = UUID.randomUUID();
        LocalDateTime now = LocalDateTime.now();

        BodyworkDto bodywork = new BodyworkDto("GOOD", "GOOD", false, true, false, false, false, 0, 0, 0, 0, false, "ok");
        MechanicalDto mechanical = new MechanicalDto("GOOD", "GOOD", "GOOD", "GOOD", false, false, true, "GOOD", "ok");
        TiresDto tires = new TiresDto("GOOD", false, false, "ok");
        InteriorDto interior = new InteriorDto("GOOD", "GOOD", "GOOD", false, false, false, "ok");
        DocumentsDto documents = new DocumentsDto(true, true, true, true, "ok");

        UpdateChecklistCommand updateChecklist = new UpdateChecklistCommand(id, bodywork, mechanical, tires, interior, documents);
        assertThat(updateChecklist.evaluationId()).isEqualTo(id);
        assertThat(updateChecklist.documents().crvlPresent()).isTrue();

        UpdateEvaluationCommand updateEvaluation = new UpdateEvaluationCommand(
            id,
            "ABC1234",
            2023,
            45000,
            "Prata",
            "2.0",
            "FLEX",
            "MANUAL",
            List.of("AC"),
            "notes"
        );
        assertThat(updateEvaluation.id()).isEqualTo(id);

        CreateEvaluationCommand create = new CreateEvaluationCommand(
            "ABC1234",
            2023,
            10000,
            "Prata",
            "2.0",
            "FLEX",
            "MANUAL",
            List.of("AC"),
            "notes"
        );
        assertThat(create.plate()).isEqualTo("ABC1234");

        ChecklistItemDto item = new ChecklistItemDto("id", "desc", true, false, "obs", "OK");
        ChecklistSectionDto section = new ChecklistSectionDto("Section", List.of(item), true, 100.0);
        EvaluationChecklistDto checklistDto = new EvaluationChecklistDto(id, List.of(section), true, 1, 1, 100.0, "ok");

        EvaluationPhotoDto photoDto = new EvaluationPhotoDto(id, "FRONT", "url", "file", 123L, now, "upload", now.plusDays(1));
        DepreciationItemDto depDto = new DepreciationItemDto(id, "BODY", "dent", BigDecimal.TEN, BigDecimal.ONE, "obs");

        VehicleEvaluationDto evalDto = new VehicleEvaluationDto(
                id,
                "ABC-1234",
                "123",
                "Toyota",
                "Corolla",
                2023,
                10000,
                "Prata",
                "2.0",
                "Flex",
                "MANUAL",
                List.of("AC"),
                EvaluationStatus.DRAFT.name(),
                BigDecimal.valueOf(150000),
                BigDecimal.valueOf(140000),
                BigDecimal.valueOf(135000),
                null,
                "obs",
                null,
                "evaluator",
                null,
                now,
                now,
                null,
                null,
                null,
                null,
                List.of(photoDto),
                List.of(depDto),
                checklistDto
        );
        assertThat(evalDto.id()).isEqualTo(id);
        assertThat(evalDto.photos()).hasSize(1);

        VehicleEvaluationSummaryDto summaryDto = new VehicleEvaluationSummaryDto(
                id,
                "ABC-1234",
                "Toyota",
                "Corolla",
                2023,
                10000,
                EvaluationStatus.DRAFT.name(),
                BigDecimal.valueOf(135000),
                "evaluator",
                null,
                now,
                now,
                false
        );
        assertThat(summaryDto.brand()).isEqualTo("Toyota");

        // Dashboard-related DTOs
        MonthlyStatDto monthly = new MonthlyStatDto(LocalDate.of(2025, 1, 1), 10, BigDecimal.valueOf(1000));
        PeriodDto period = new PeriodDto(now.minusDays(30), now);
        EvaluationKpiDto kpis = new EvaluationKpiDto(
            BigDecimal.valueOf(10),
            BigDecimal.valueOf(5),
            BigDecimal.valueOf(3),
            BigDecimal.valueOf(2)
        );
        BrandStatDto brandStat = new BrandStatDto("Toyota", 5L, BigDecimal.valueOf(20000), BigDecimal.valueOf(80));
        EvaluationDashboardDto dashboard = new EvaluationDashboardDto(period, kpis, List.of(monthly), List.of(brandStat), now);
        assertThat(dashboard.monthlyTrend()).hasSize(1);

        PendingEvaluationSummaryDto pending = new PendingEvaluationSummaryDto(
            id,
            "ABC-1234",
            "Toyota Corolla 2023",
            BigDecimal.valueOf(135000),
            now,
            "evaluator",
            2,
            10,
            false
        );
        assertThat(pending.id()).isEqualTo(id);

        EvaluationValidationDto validation = new EvaluationValidationDto(
            "ABC-1234",
            "Toyota",
            "Corolla",
            2023,
            EvaluationStatus.APPROVED.name(),
            BigDecimal.valueOf(135000),
            now,
            now.plusDays(10)
        );
        assertThat(validation.plate()).isEqualTo("ABC-1234");

        DepreciationDetailDto depreciationDetail = DepreciationDetailDto.of(
            "dep-1",
            "BODY",
            "dent",
            Money.of(BigDecimal.TEN),
            "ok"
        );
        ValuationResultDto valuation = new ValuationResultDto.Builder()
            .evaluationId(id.toString())
            .fipePrice(Money.of(BigDecimal.valueOf(150000)))
            .totalDepreciation(Money.of(BigDecimal.valueOf(10000)))
            .safetyMargin(Money.of(BigDecimal.valueOf(5000)))
            .profitMargin(Money.of(BigDecimal.valueOf(3000)))
            .suggestedValue(Money.of(BigDecimal.valueOf(135000)))
            .depreciationDetails(List.of(depreciationDetail))
            .liquidityPercentage(0.9)
            .manualAdjustmentPercentage(null)
            .manualAdjustmentAmount(null)
            .finalValue(Money.of(BigDecimal.valueOf(135000)))
            .build();
        assertThat(valuation.getEvaluationId()).isEqualTo(id.toString());

        assertThat(depreciationDetail.getCategory()).isEqualTo("BODY");
        assertThat(brandStat.brand()).isEqualTo("Toyota");
    }
}
