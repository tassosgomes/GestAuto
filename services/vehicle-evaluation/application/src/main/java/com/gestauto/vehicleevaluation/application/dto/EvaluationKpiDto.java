package com.gestauto.vehicleevaluation.application.dto;

import java.math.BigDecimal;

public record EvaluationKpiDto(
    BigDecimal evaluationsPerMonth,
    BigDecimal approvalRatePercent,
    BigDecimal averageTicket,
    BigDecimal averageReviewTimeHours
) {
}
