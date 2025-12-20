package com.gestauto.vehicleevaluation.infra.repository.projection;

import java.math.BigDecimal;
import java.time.LocalDate;

public interface MonthlyStatProjection {

    LocalDate getMonth();

    Long getEvaluationsCount();

    BigDecimal getTotalTicket();
}
