package com.gestauto.vehicleevaluation.application.query;

import com.gestauto.vehicleevaluation.application.dto.PagedResult;
import com.gestauto.vehicleevaluation.application.dto.VehicleEvaluationSummaryDto;
import com.gestauto.vehicleevaluation.domain.entity.VehicleEvaluation;
import com.gestauto.vehicleevaluation.domain.enums.EvaluationStatus;
import com.gestauto.vehicleevaluation.domain.enums.FuelType;
import com.gestauto.vehicleevaluation.domain.repository.VehicleEvaluationRepository;
import com.gestauto.vehicleevaluation.domain.value.Money;
import com.gestauto.vehicleevaluation.domain.value.Plate;
import com.gestauto.vehicleevaluation.domain.value.VehicleInfo;
import org.junit.jupiter.api.Test;
import org.mockito.ArgumentMatchers;

import java.math.BigDecimal;
import java.util.List;
import java.util.UUID;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.Mockito.*;

class ListEvaluationsHandlerTest {

    @Test
    void handle_callsFindAll_whenNoFilters() throws Exception {
        VehicleEvaluationRepository repo = mock(VehicleEvaluationRepository.class);
        ListEvaluationsHandler handler = new ListEvaluationsHandler(repo);

        VehicleEvaluation evaluation = VehicleEvaluation.create(
                Plate.of("ABC1234"),
                "12345678901",
                VehicleInfo.of("Toyota", "Corolla", "2.0", 2023, 2023, "Prata", FuelType.FLEX),
                Money.of(BigDecimal.valueOf(10000)),
                "evaluator"
        );

        when(repo.findAll(0, 20)).thenReturn(List.of(evaluation));

        // Use null page/size to exercise ListEvaluationsQuery defaults
        ListEvaluationsQuery query = new ListEvaluationsQuery(null, null, null, null, null, null, null);
        PagedResult<VehicleEvaluationSummaryDto> result = handler.handle(query);

        verify(repo).findAll(0, 20);
        assertThat(result.content()).hasSize(1);
        assertThat(result.page()).isEqualTo(0);
        assertThat(result.size()).isEqualTo(20);
        assertThat(result.totalElements()).isEqualTo(100L);
    }

    @Test
    void handle_callsFindByEvaluator_whenEvaluatorFilter() throws Exception {
        VehicleEvaluationRepository repo = mock(VehicleEvaluationRepository.class);
        ListEvaluationsHandler handler = new ListEvaluationsHandler(repo);

        UUID evaluatorId = UUID.randomUUID();
        when(repo.findByEvaluator(evaluatorId.toString())).thenReturn(List.of());

        ListEvaluationsQuery query = new ListEvaluationsQuery(evaluatorId, null, null, 0, 10, null, null);
        PagedResult<VehicleEvaluationSummaryDto> result = handler.handle(query);

        verify(repo).findByEvaluator(evaluatorId.toString());
        assertThat(result.content()).isEmpty();
        assertThat(result.size()).isEqualTo(10);
    }

    @Test
    void handle_callsFindByStatusAndCounts_whenStatusFilter() throws Exception {
        VehicleEvaluationRepository repo = mock(VehicleEvaluationRepository.class);
        ListEvaluationsHandler handler = new ListEvaluationsHandler(repo);

        when(repo.findByStatus(EvaluationStatus.DRAFT)).thenReturn(List.of());
        when(repo.countByStatus(EvaluationStatus.DRAFT)).thenReturn(7L);

        ListEvaluationsQuery query = new ListEvaluationsQuery(null, EvaluationStatus.DRAFT, null, 0, 5, null, null);
        PagedResult<VehicleEvaluationSummaryDto> result = handler.handle(query);

        verify(repo).findByStatus(EvaluationStatus.DRAFT);
        verify(repo).countByStatus(EvaluationStatus.DRAFT);
        assertThat(result.totalElements()).isEqualTo(7L);
        assertThat(result.totalPages()).isEqualTo(2);
    }

    @Test
    void handle_callsFindByStatusAndEvaluator_whenBothFilters() throws Exception {
        VehicleEvaluationRepository repo = mock(VehicleEvaluationRepository.class);
        ListEvaluationsHandler handler = new ListEvaluationsHandler(repo);

        UUID evaluatorId = UUID.randomUUID();
        when(repo.findByStatusAndEvaluator(EvaluationStatus.DRAFT, evaluatorId.toString())).thenReturn(List.of());

        ListEvaluationsQuery query = new ListEvaluationsQuery(evaluatorId, EvaluationStatus.DRAFT, null, 0, 20, null, null);
        handler.handle(query);

        verify(repo).findByStatusAndEvaluator(EvaluationStatus.DRAFT, evaluatorId.toString());
        verify(repo, never()).findAll(ArgumentMatchers.anyInt(), ArgumentMatchers.anyInt());
    }
}
