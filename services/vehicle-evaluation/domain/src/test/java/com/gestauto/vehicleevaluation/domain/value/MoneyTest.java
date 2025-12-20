package com.gestauto.vehicleevaluation.domain.value;

import java.math.BigDecimal;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.*;

@DisplayName("Money Value Object Tests")
class MoneyTest {

    @Test
    void shouldScaleToTwoDecimalsAndFormat() {
        Money money = Money.of(new BigDecimal("10.555"));

        assertEquals(new BigDecimal("10.56"), money.getAmount());
        assertTrue(money.toBrazilianFormat().startsWith("R$ "));
        assertTrue(money.toString().contains("formatted='R$ "));
    }

    @Test
    void shouldCreateFromStringAndRejectInvalidString() {
        assertEquals(Money.of(new BigDecimal("12.34")), Money.of("12.34"));
        assertThrows(IllegalArgumentException.class, () -> Money.of("not-a-number"));
    }

    @Test
    void shouldSupportArithmeticAndComparisons() {
        Money a = Money.of(new BigDecimal("10.00"));
        Money b = Money.of(new BigDecimal("2.50"));

        assertEquals(Money.of(new BigDecimal("12.50")), a.add(b));
        assertEquals(Money.of(new BigDecimal("7.50")), a.subtract(b));
        assertEquals(Money.of(new BigDecimal("20.00")), a.multiply(2));
        assertEquals(Money.of(new BigDecimal("1.50")), Money.of(new BigDecimal("10.00")).percentage(15));

        assertTrue(a.isGreaterThan(b));
        assertTrue(b.isLessThan(a));
        assertTrue(Money.ZERO.isZero());
        assertTrue(Money.ONE.isPositive());
        assertFalse(Money.ONE.isNegative());
    }

    @Test
    void shouldConvertToAndFromCents() {
        Money money = Money.fromCents(1234);
        assertEquals(new BigDecimal("12.34"), money.getAmount());
        assertEquals(1234L, money.getCents());
    }
}
