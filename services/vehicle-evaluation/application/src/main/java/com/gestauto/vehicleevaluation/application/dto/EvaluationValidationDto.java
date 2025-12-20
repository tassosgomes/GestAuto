package com.gestauto.vehicleevaluation.application.dto;

import java.math.BigDecimal;
import java.time.LocalDateTime;

public record EvaluationValidationDto(
    String plate,
    String brand,
    String model,
    Integer year,
    String status,
    BigDecimal approvedValue,
    LocalDateTime validatedAt,
    LocalDateTime validUntil
) {
}
