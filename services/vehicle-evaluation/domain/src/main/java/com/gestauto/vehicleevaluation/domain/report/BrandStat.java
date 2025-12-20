package com.gestauto.vehicleevaluation.domain.report;

import java.math.BigDecimal;

public record BrandStat(
    String brand,
    long evaluationsCount,
    BigDecimal averageTicket,
    BigDecimal approvalRatePercent
) {
}
