package com.gestauto.vehicleevaluation.application.query;

import com.gestauto.vehicleevaluation.application.dto.PagedResult;
import com.gestauto.vehicleevaluation.application.dto.PendingEvaluationSummaryDto;
import com.gestauto.vehicleevaluation.domain.entity.VehicleEvaluation;
import com.gestauto.vehicleevaluation.domain.enums.EvaluationStatus;
import com.gestauto.vehicleevaluation.domain.enums.FuelType;
import com.gestauto.vehicleevaluation.domain.repository.VehicleEvaluationRepository;
import com.gestauto.vehicleevaluation.domain.value.EvaluationId;
import com.gestauto.vehicleevaluation.domain.value.Money;
import com.gestauto.vehicleevaluation.domain.value.Plate;
import com.gestauto.vehicleevaluation.domain.value.VehicleInfo;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.mockito.junit.jupiter.MockitoExtension;
import org.mockito.junit.jupiter.MockitoSettings;
import org.mockito.quality.Strictness;

import java.math.BigDecimal;
import java.time.LocalDateTime;
import java.util.ArrayList;
import java.util.List;
import java.util.UUID;

import static org.assertj.core.api.Assertions.*;
import static org.mockito.ArgumentMatchers.any;
import static org.mockito.ArgumentMatchers.anyInt;
import static org.mockito.Mockito.*;

/**
 * Testes unitários para GetPendingApprovalsHandler.
 */
@ExtendWith(MockitoExtension.class)
@MockitoSettings(strictness = Strictness.LENIENT)
class GetPendingApprovalsHandlerTest {

    @Mock
    private VehicleEvaluationRepository evaluationRepository;

    @InjectMocks
    private GetPendingApprovalsHandler handler;

    private List<VehicleEvaluation> mockEvaluations;

    @BeforeEach
    void setUp() {
        mockEvaluations = new ArrayList<>();
        
        // Criar 5 avaliações mock com valores diferentes
        for (int i = 0; i < 5; i++) {
            VehicleEvaluation eval = mock(VehicleEvaluation.class);
            EvaluationId evalId = EvaluationId.from(UUID.randomUUID().toString());
            
            when(eval.getId()).thenReturn(evalId);
            when(eval.getPlate()).thenReturn(Plate.of("ABC" + i + "D34"));
            when(eval.getVehicleInfo()).thenReturn(VehicleInfo.create("Brand" + i, "Model" + i, 2020 + i, "Prata", FuelType.FLEX, "Version"));
            when(eval.getFinalValue()).thenReturn(Money.of(new BigDecimal(50000 + (i * 10000))));
            when(eval.getCreatedAt()).thenReturn(LocalDateTime.now().minusDays(i));
            when(eval.getEvaluatorId()).thenReturn("evaluator-" + i);
            when(eval.getPhotos()).thenReturn(List.of());
            
            mockEvaluations.add(eval);
        }
    }

    @Test
    void handle_WithDefaultQuery_ShouldReturnPagedResults() {
        // Arrange
        GetPendingApprovalsQuery query = new GetPendingApprovalsQuery(0, 20, "finalValue", true);
        when(evaluationRepository.findPendingApprovals(any(), anyInt(), anyInt()))
            .thenReturn(mockEvaluations);

        // Act
        PagedResult<PendingEvaluationSummaryDto> result = handler.handle(query);

        // Assert
        assertThat(result).isNotNull();
        assertThat(result.content()).hasSize(5);
        assertThat(result.page()).isEqualTo(0);
        assertThat(result.size()).isEqualTo(20);
        assertThat(result.totalElements()).isEqualTo(5);
    }

    @Test
    void handle_WithSortByValue_ShouldOrderByValueDescending() {
        // Arrange
        GetPendingApprovalsQuery query = new GetPendingApprovalsQuery(0, 20, "finalValue", true);
        when(evaluationRepository.findPendingApprovals(any(), anyInt(), anyInt()))
            .thenReturn(mockEvaluations);

        // Act
        PagedResult<PendingEvaluationSummaryDto> result = handler.handle(query);

        // Assert
        List<PendingEvaluationSummaryDto> content = result.content();
        assertThat(content).isNotEmpty();
        
        // Verificar que está ordenado do maior para o menor valor
        for (int i = 0; i < content.size() - 1; i++) {
            BigDecimal currentValue = content.get(i).suggestedValue();
            BigDecimal nextValue = content.get(i + 1).suggestedValue();
            assertThat(currentValue).isGreaterThanOrEqualTo(nextValue);
        }
    }

    @Test
    void handle_WithSortByDateAscending_ShouldOrderByDateAscending() {
        // Arrange
        GetPendingApprovalsQuery query = new GetPendingApprovalsQuery(0, 20, "createdAt", false);
        when(evaluationRepository.findPendingApprovals(any(), anyInt(), anyInt()))
            .thenReturn(mockEvaluations);

        // Act
        PagedResult<PendingEvaluationSummaryDto> result = handler.handle(query);

        // Assert
        List<PendingEvaluationSummaryDto> content = result.content();
        assertThat(content).isNotEmpty();
        
        // Verificar que está ordenado da data mais antiga para a mais recente
        for (int i = 0; i < content.size() - 1; i++) {
            LocalDateTime currentDate = content.get(i).createdAt();
            LocalDateTime nextDate = content.get(i + 1).createdAt();
            assertThat(currentDate).isBeforeOrEqualTo(nextDate);
        }
    }

    @Test
    void handle_WithPagination_ShouldReturnCorrectPage() {
        // Arrange
        GetPendingApprovalsQuery query = new GetPendingApprovalsQuery(1, 2, "finalValue", true);
        when(evaluationRepository.findPendingApprovals(any(), anyInt(), anyInt()))
            .thenReturn(mockEvaluations);

        // Act
        PagedResult<PendingEvaluationSummaryDto> result = handler.handle(query);

        // Assert
        assertThat(result.content()).hasSize(2);
        assertThat(result.page()).isEqualTo(1);
        assertThat(result.size()).isEqualTo(2);
        assertThat(result.totalPages()).isEqualTo(3);
    }

    @Test
    void handle_ShouldCalculateDaysPending() {
        // Arrange
        GetPendingApprovalsQuery query = new GetPendingApprovalsQuery(0, 20, "createdAt", false);
        when(evaluationRepository.findPendingApprovals(any(), anyInt(), anyInt()))
            .thenReturn(mockEvaluations);

        // Act
        PagedResult<PendingEvaluationSummaryDto> result = handler.handle(query);

        // Assert
        List<PendingEvaluationSummaryDto> content = result.content();
        assertThat(content).allMatch(dto -> dto.daysPending() >= 0);

        // Ordenado por createdAt asc: mais antiga primeiro => mais dias pendente
        assertThat(content.get(0).daysPending())
            .isGreaterThanOrEqualTo(content.get(content.size() - 1).daysPending());
    }

    @Test
    void handle_ShouldFormatVehicleInfo() {
        // Arrange
        GetPendingApprovalsQuery query = new GetPendingApprovalsQuery(0, 20, "finalValue", true);
        when(evaluationRepository.findPendingApprovals(any(), anyInt(), anyInt()))
            .thenReturn(mockEvaluations);

        // Act
        PagedResult<PendingEvaluationSummaryDto> result = handler.handle(query);

        // Assert
        List<PendingEvaluationSummaryDto> content = result.content();
        assertThat(content).allMatch(dto -> 
            dto.vehicleInfo() != null && 
            dto.vehicleInfo().contains("Brand") &&
            dto.vehicleInfo().contains("Model")
        );
    }

    @Test
    void handle_WithEmptyResults_ShouldReturnEmptyPage() {
        // Arrange
        GetPendingApprovalsQuery query = new GetPendingApprovalsQuery(0, 20, "finalValue", true);
        when(evaluationRepository.findPendingApprovals(any(), anyInt(), anyInt()))
            .thenReturn(List.of());

        // Act
        PagedResult<PendingEvaluationSummaryDto> result = handler.handle(query);

        // Assert
        assertThat(result.content()).isEmpty();
        assertThat(result.totalElements()).isEqualTo(0);
        assertThat(result.totalPages()).isEqualTo(0);
    }

    @Test
    void query_WithNullValues_ShouldUseDefaults() {
        // Act
        GetPendingApprovalsQuery query = new GetPendingApprovalsQuery(null, null, null, null);

        // Assert
        assertThat(query.page()).isEqualTo(0);
        assertThat(query.size()).isEqualTo(20);
        assertThat(query.sortBy()).isEqualTo("finalValue");
        assertThat(query.sortDescending()).isTrue();
    }

    @Test
    void query_WithNegativePage_ShouldUseZero() {
        // Act
        GetPendingApprovalsQuery query = new GetPendingApprovalsQuery(-5, 20, "finalValue", true);

        // Assert
        assertThat(query.page()).isEqualTo(0);
    }

    @Test
    void query_WithSizeOver100_ShouldCap() {
        // Act
        GetPendingApprovalsQuery query = new GetPendingApprovalsQuery(0, 200, "finalValue", true);

        // Assert
        assertThat(query.size()).isEqualTo(100);
    }

    @Test
    void handle_ShouldSetPaginationFlags() {
        // Arrange
        GetPendingApprovalsQuery query = new GetPendingApprovalsQuery(0, 2, "finalValue", true);
        when(evaluationRepository.findPendingApprovals(any(), anyInt(), anyInt()))
            .thenReturn(mockEvaluations);

        // Act
        PagedResult<PendingEvaluationSummaryDto> result = handler.handle(query);

        // Assert
        assertThat(result.first()).isTrue();
        assertThat(result.last()).isFalse();
    }

    @Test
    void handle_OnLastPage_ShouldSetLastFlag() {
        // Arrange
        GetPendingApprovalsQuery query = new GetPendingApprovalsQuery(2, 2, "finalValue", true);
        when(evaluationRepository.findPendingApprovals(any(), anyInt(), anyInt()))
            .thenReturn(mockEvaluations);

        // Act
        PagedResult<PendingEvaluationSummaryDto> result = handler.handle(query);

        // Assert
        assertThat(result.last()).isTrue();
    }
}
