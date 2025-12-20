package com.gestauto.vehicleevaluation.application.query;

import com.gestauto.vehicleevaluation.application.dto.VehicleEvaluationDto;
import com.gestauto.vehicleevaluation.domain.entity.VehicleEvaluation;
import com.gestauto.vehicleevaluation.domain.enums.FuelType;
import com.gestauto.vehicleevaluation.domain.exception.DomainException;
import com.gestauto.vehicleevaluation.domain.repository.VehicleEvaluationRepository;
import com.gestauto.vehicleevaluation.domain.value.EvaluationId;
import com.gestauto.vehicleevaluation.domain.value.Money;
import com.gestauto.vehicleevaluation.domain.value.Plate;
import com.gestauto.vehicleevaluation.domain.value.VehicleInfo;
import org.junit.jupiter.api.Test;

import java.math.BigDecimal;
import java.util.Optional;
import java.util.UUID;

import static org.assertj.core.api.Assertions.assertThat;
import static org.assertj.core.api.Assertions.assertThatThrownBy;
import static org.mockito.Mockito.*;

class GetEvaluationHandlerTest {

    @Test
    void handle_whenNotFound_throwsDomainException() {
        VehicleEvaluationRepository repo = mock(VehicleEvaluationRepository.class);
        GetEvaluationHandler handler = new GetEvaluationHandler(repo);

        UUID id = UUID.randomUUID();
        when(repo.findById(EvaluationId.from(id))).thenReturn(Optional.empty());

        assertThatThrownBy(() -> handler.handle(new GetEvaluationQuery(id)))
                .isInstanceOf(DomainException.class);
    }

    @Test
    void handle_whenFound_returnsDtoWithExpectedFields() throws Exception {
        VehicleEvaluationRepository repo = mock(VehicleEvaluationRepository.class);
        GetEvaluationHandler handler = new GetEvaluationHandler(repo);

        VehicleEvaluation evaluation = VehicleEvaluation.create(
                Plate.of("ABC1234"),
                "12345678901",
                VehicleInfo.of("Toyota", "Corolla", "2.0", 2023, 2023, "Prata", FuelType.FLEX),
                Money.of(BigDecimal.valueOf(10000)),
                "evaluator"
        );

        UUID id = evaluation.getId().getValue();
        when(repo.findById(EvaluationId.from(id))).thenReturn(Optional.of(evaluation));

        VehicleEvaluationDto dto = handler.handle(new GetEvaluationQuery(id));

        assertThat(dto.id()).isEqualTo(id);
        assertThat(dto.plate()).isEqualTo("ABC-1234");
        assertThat(dto.brand()).isEqualTo("Toyota");
        assertThat(dto.model()).isEqualTo("Corolla");
        assertThat(dto.gearbox()).isEqualTo("MANUAL");
        assertThat(dto.photos()).isEmpty();
        assertThat(dto.depreciationItems()).isEmpty();
        assertThat(dto.checklist()).isNull();
    }
}
