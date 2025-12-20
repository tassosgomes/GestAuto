package com.gestauto.vehicleevaluation.application.query;

import com.gestauto.vehicleevaluation.domain.report.BrandStat;
import com.gestauto.vehicleevaluation.domain.report.EvaluationKpi;
import com.gestauto.vehicleevaluation.domain.report.MonthlyStat;
import com.gestauto.vehicleevaluation.domain.repository.EvaluationDashboardRepository;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.mockito.Mock;
import org.mockito.junit.jupiter.MockitoExtension;

import java.math.BigDecimal;
import java.time.LocalDate;
import java.time.LocalDateTime;
import java.util.List;

import static org.junit.jupiter.api.Assertions.*;
import static org.mockito.Mockito.when;

@ExtendWith(MockitoExtension.class)
@DisplayName("GetEvaluationDashboardHandler Tests")
class GetEvaluationDashboardHandlerTest {

    @Mock
    private EvaluationDashboardRepository dashboardRepository;

    private GetEvaluationDashboardHandler handler;

    @BeforeEach
    void setUp() {
        handler = new GetEvaluationDashboardHandler(dashboardRepository);
    }

    @Test
    @DisplayName("should build dashboard with computed evaluationsPerMonth")
    void shouldBuildDashboardWithEvaluationsPerMonth() {
        LocalDateTime startDate = LocalDateTime.of(2025, 1, 1, 0, 0);
        LocalDateTime endDate = LocalDateTime.of(2025, 12, 31, 23, 59);

        EvaluationKpi kpi = new EvaluationKpi(
            120,
            BigDecimal.valueOf(80.0),
            BigDecimal.valueOf(50000.00),
            BigDecimal.valueOf(12.5)
        );

        when(dashboardRepository.getKpis(startDate, endDate, null, null)).thenReturn(kpi);
        when(dashboardRepository.getMonthlyStats(startDate, endDate, null, null)).thenReturn(List.of(
            new MonthlyStat(LocalDate.of(2025, 1, 1), 10, BigDecimal.valueOf(500000)),
            new MonthlyStat(LocalDate.of(2025, 2, 1), 10, BigDecimal.valueOf(510000))
        ));
        when(dashboardRepository.getBrandStats(startDate, endDate, null, null)).thenReturn(List.of(
            new BrandStat("Toyota", 30, BigDecimal.valueOf(52000), BigDecimal.valueOf(85.0))
        ));

        var result = handler.handle(new GetEvaluationDashboardQuery(startDate, endDate, null, null));

        assertNotNull(result);
        assertNotNull(result.kpis());
        assertEquals(new BigDecimal("10.00"), result.kpis().evaluationsPerMonth());
        assertEquals(BigDecimal.valueOf(80.0), result.kpis().approvalRatePercent());
        assertEquals(2, result.monthlyTrend().size());
        assertEquals(1, result.brandDistribution().size());
    }
}
