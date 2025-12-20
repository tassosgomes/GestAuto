package com.gestauto.vehicleevaluation.infra.repository.projection;

import java.math.BigDecimal;

public interface EvaluationKpiProjection {

    Long getTotalEvaluations();

    BigDecimal getApprovalRatePercent();

    BigDecimal getAverageTicket();

    BigDecimal getAverageReviewTimeHours();
}
