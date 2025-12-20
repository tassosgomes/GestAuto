package com.gestauto.vehicleevaluation.domain.report;

import java.math.BigDecimal;

public record EvaluationKpi(
    long totalEvaluations,
    BigDecimal approvalRatePercent,
    BigDecimal averageTicket,
    BigDecimal averageReviewTimeHours
) {
}
