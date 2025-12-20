package com.gestauto.vehicleevaluation.application.dto;

import java.math.BigDecimal;
import java.time.LocalDate;

public record MonthlyStatDto(
    LocalDate month,
    long evaluationsCount,
    BigDecimal totalTicket
) {
}
