package com.gestauto.vehicleevaluation.application.dto;

import java.time.LocalDateTime;

public record PeriodDto(
    LocalDateTime startDate,
    LocalDateTime endDate
) {
}
