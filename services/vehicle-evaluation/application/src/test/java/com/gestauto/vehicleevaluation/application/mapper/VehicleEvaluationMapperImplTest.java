package com.gestauto.vehicleevaluation.application.mapper;

import com.gestauto.vehicleevaluation.application.dto.VehicleEvaluationDto;
import com.gestauto.vehicleevaluation.application.dto.VehicleEvaluationSummaryDto;
import com.gestauto.vehicleevaluation.domain.entity.DepreciationItem;
import com.gestauto.vehicleevaluation.domain.entity.EvaluationChecklist;
import com.gestauto.vehicleevaluation.domain.entity.EvaluationPhoto;
import com.gestauto.vehicleevaluation.domain.entity.VehicleEvaluation;
import com.gestauto.vehicleevaluation.domain.enums.EvaluationStatus;
import com.gestauto.vehicleevaluation.domain.enums.FuelType;
import com.gestauto.vehicleevaluation.domain.enums.PhotoType;
import com.gestauto.vehicleevaluation.domain.value.EvaluationId;
import com.gestauto.vehicleevaluation.domain.value.Money;
import com.gestauto.vehicleevaluation.domain.value.Plate;
import com.gestauto.vehicleevaluation.domain.value.VehicleInfo;
import org.junit.jupiter.api.Test;

import java.math.BigDecimal;
import java.time.LocalDateTime;
import java.util.List;

import static org.assertj.core.api.Assertions.assertThat;

class VehicleEvaluationMapperImplTest {

    private final VehicleEvaluationMapper mapper = new VehicleEvaluationMapperImpl();

    @Test
    void toDto_whenNull_returnsNull() {
        assertThat(mapper.toDto(null)).isNull();
        assertThat(mapper.toSummaryDto(null)).isNull();
        assertThat(mapper.photosToDtos(null)).isNull();
        assertThat(mapper.depreciationItemsToDtos(null)).isNull();
        assertThat(mapper.toSummaryDtoList(null)).isNull();
    }

    @Test
    void toDto_and_toSummaryDto_mapsNestedValuesAndLists() {
        EvaluationId evaluationId = EvaluationId.generate();
        Plate plate = Plate.of("ABC1234");
        VehicleInfo info = VehicleInfo.of("Toyota", "Corolla", "2.0 XEI", 2023, 2023, "Prata", FuelType.FLEX);

        Money mileage = Money.of(BigDecimal.valueOf(55000));
        Money fipePrice = Money.of(BigDecimal.valueOf(150000));
        Money baseValue = Money.of(BigDecimal.valueOf(140000));
        Money finalValue = Money.of(BigDecimal.valueOf(135000));
        Money approvedValue = Money.of(BigDecimal.valueOf(134000));

        EvaluationPhoto photoWithThumb = EvaluationPhoto.create(
                evaluationId,
                PhotoType.EXTERIOR_FRONT,
                "front.jpg",
                "/path/front.jpg",
                1234,
                "image/jpeg",
                "https://cdn.example/front.jpg",
                "https://cdn.example/front_thumb.jpg"
        );
        EvaluationPhoto photoWithoutThumb = EvaluationPhoto.createWithoutThumbnail(
                evaluationId,
                PhotoType.EXTERIOR_REAR,
                "rear.png",
                "/path/rear.png",
                2222,
                "image/png",
                "https://cdn.example/rear.png"
        );

        DepreciationItem dep = DepreciationItem.create(
                evaluationId,
                "BODY",
                "Minor dent",
                Money.of(BigDecimal.valueOf(1500)),
                "Justification",
                "evaluator"
        );

        EvaluationChecklist checklist = EvaluationChecklist.create(evaluationId);
        LocalDateTime now = LocalDateTime.now();

        VehicleEvaluation evaluation = VehicleEvaluation.restore(
                evaluationId,
                plate,
                "12345678901",
                info,
                mileage,
                EvaluationStatus.APPROVED,
                fipePrice,
                baseValue,
                finalValue,
                approvedValue,
                "obs",
                "just",
                now.minusDays(2),
                now.minusDays(1),
                now.minusDays(1),
                now.minusHours(12),
                "evaluator",
                "approver",
                now.plusDays(10),
                "token",
                List.of(photoWithThumb, photoWithoutThumb),
                List.of(dep),
                checklist
        );

        VehicleEvaluationDto dto = mapper.toDto(evaluation);
        assertThat(dto).isNotNull();
        assertThat(dto.id()).isEqualTo(evaluationId.getValue());
        assertThat(dto.plate()).isEqualTo("ABC-1234");
        assertThat(dto.brand()).isEqualTo("Toyota");
        assertThat(dto.model()).isEqualTo("Corolla");
        assertThat(dto.year()).isEqualTo(2023);
        assertThat(dto.mileage()).isEqualTo(55000);
        assertThat(dto.fipePrice()).isEqualTo(fipePrice.getAmount());
        assertThat(dto.baseValue()).isEqualTo(baseValue.getAmount());
        assertThat(dto.finalValue()).isEqualTo(finalValue.getAmount());
        assertThat(dto.approvedValue()).isEqualTo(approvedValue.getAmount());
        assertThat(dto.status()).isEqualTo(EvaluationStatus.APPROVED.name());
        assertThat(dto.photos()).hasSize(2);
        assertThat(dto.depreciationItems()).hasSize(1);
        assertThat(dto.checklist()).isNotNull();

        VehicleEvaluationSummaryDto summaryDto = mapper.toSummaryDto(evaluation);
        assertThat(summaryDto).isNotNull();
        assertThat(summaryDto.id()).isEqualTo(evaluationId.getValue());
        assertThat(summaryDto.plate()).isEqualTo("ABC-1234");
        assertThat(summaryDto.status()).isEqualTo(EvaluationStatus.APPROVED.name());
        assertThat(summaryDto.finalValue()).isEqualTo(finalValue.getAmount());

        // Exercise default methods on the mapper interface
        assertThat(mapper.statusToString(EvaluationStatus.DRAFT)).isEqualTo("DRAFT");
        assertThat(mapper.bigDecimalToString(BigDecimal.TEN)).isEqualTo("10");
        assertThat(mapper.bigDecimalToString(null)).isNull();
        assertThat(mapper.mapLocalDateTime(now)).isEqualTo(now);
        assertThat(mapper.isExpired(evaluation)).isFalse();
    }
}
