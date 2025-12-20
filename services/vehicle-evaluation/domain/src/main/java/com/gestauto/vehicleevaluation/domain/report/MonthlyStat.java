package com.gestauto.vehicleevaluation.domain.report;

import java.math.BigDecimal;
import java.time.LocalDate;

public record MonthlyStat(
    LocalDate month,
    long evaluationsCount,
    BigDecimal totalTicket
) {
}
