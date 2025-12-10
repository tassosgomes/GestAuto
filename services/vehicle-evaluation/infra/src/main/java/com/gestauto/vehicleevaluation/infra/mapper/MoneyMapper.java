package com.gestauto.vehicleevaluation.infra.mapper;

import com.gestauto.vehicleevaluation.domain.value.Money;
import java.math.BigDecimal;
import java.util.Objects;

/**
 * Mapper utilitário para conversão de {@link Money} para tipos persistíveis.
 */
public final class MoneyMapper {

    private static final String DEFAULT_CURRENCY = "BRL";

    private MoneyMapper() {
    }

    public static BigDecimal toAmount(Money money) {
        return money != null ? money.getAmount() : null;
    }

    public static String toCurrency(Money money) {
        return money != null ? DEFAULT_CURRENCY : null;
    }

    public static Money toDomain(BigDecimal amount) {
        return amount != null ? Money.of(amount) : null;
    }

    public static String defaultCurrency() {
        return DEFAULT_CURRENCY;
    }

    public static Money fromAmountAndCurrency(BigDecimal amount, String currency) {
        if (amount == null) {
            return null;
        }
        // Currency currently fixed to BRL; validation can evolve if multi-currency is needed.
        String resolvedCurrency = currency != null ? currency : DEFAULT_CURRENCY;
        Objects.requireNonNull(resolvedCurrency, "Currency cannot be null for Money mapping");
        return Money.of(amount);
    }
}
