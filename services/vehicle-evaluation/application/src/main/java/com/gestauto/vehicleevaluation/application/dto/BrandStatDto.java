package com.gestauto.vehicleevaluation.application.dto;

import java.math.BigDecimal;

public record BrandStatDto(
    String brand,
    long evaluationsCount,
    BigDecimal averageTicket,
    BigDecimal approvalRatePercent
) {
}
