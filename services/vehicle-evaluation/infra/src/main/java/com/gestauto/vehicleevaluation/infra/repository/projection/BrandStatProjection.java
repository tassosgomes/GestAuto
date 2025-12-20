package com.gestauto.vehicleevaluation.infra.repository.projection;

import java.math.BigDecimal;

public interface BrandStatProjection {

    String getBrand();

    Long getEvaluationsCount();

    BigDecimal getAverageTicket();

    BigDecimal getApprovalRatePercent();
}
