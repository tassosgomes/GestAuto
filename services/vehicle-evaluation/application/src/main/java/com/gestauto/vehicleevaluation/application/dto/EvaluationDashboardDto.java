package com.gestauto.vehicleevaluation.application.dto;

import java.time.LocalDateTime;
import java.util.List;

public record EvaluationDashboardDto(
    PeriodDto period,
    EvaluationKpiDto kpis,
    List<MonthlyStatDto> monthlyTrend,
    List<BrandStatDto> brandDistribution,
    LocalDateTime generatedAt
) {
}
