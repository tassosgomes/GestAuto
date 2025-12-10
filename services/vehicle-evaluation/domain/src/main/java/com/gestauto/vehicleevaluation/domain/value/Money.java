package com.gestauto.vehicleevaluation.domain.value;

import java.math.BigDecimal;
import java.math.RoundingMode;
import java.util.Objects;

/**
 * Value Object para representar valores monetários.
 *
 * Este Value Object encapsula operações monetárias com precisão
 * de 2 casas decimais, garantindo imutabilidade e validações.
 */
public final class Money {

    private static final BigDecimal HUNDRED = new BigDecimal("100");
    private static final int SCALE = 2;

    // Instâncias imutáveis comuns
    public static final Money ZERO = new Money(BigDecimal.ZERO);
    public static final Money ONE = new Money(BigDecimal.ONE);

    private final BigDecimal amount;

    private Money(BigDecimal amount) {
        this.amount = Objects.requireNonNull(amount, "Amount cannot be null")
                .setScale(SCALE, RoundingMode.HALF_EVEN);

        validateNonNegative();
    }

    /**
     * Cria uma instância de Money a partir de um valor BigDecimal.
     *
     * @param amount valor monetário
     * @return nova instância de Money
     * @throws IllegalArgumentException se o valor for nulo ou negativo
     */
    public static Money of(BigDecimal amount) {
        return new Money(amount);
    }

    /**
     * Cria uma instância de Money a partir de um valor double.
     *
     * @param amount valor monetário
     * @return nova instância de Money
     */
    public static Money of(double amount) {
        return new Money(BigDecimal.valueOf(amount));
    }

    /**
     * Cria uma instância de Money a partir de uma string.
     *
     * @param amount valor monetário como string
     * @return nova instância de Money
     * @throws IllegalArgumentException se a string for inválida
     */
    public static Money of(String amount) {
        try {
            return new Money(new BigDecimal(amount));
        } catch (NumberFormatException e) {
            throw new IllegalArgumentException("Invalid monetary amount: " + amount, e);
        }
    }

    /**
     * Cria uma instância de Money a partir de centavos (long).
     *
     * @param cents valor em centavos
     * @return nova instância de Money
     */
    public static Money fromCents(long cents) {
        return new Money(BigDecimal.valueOf(cents).divide(HUNDRED));
    }

    /**
     * Retorna o valor como BigDecimal.
     *
     * @return valor monetário
     */
    public BigDecimal getAmount() {
        return amount;
    }

    /**
     * Retorna o valor formatado como string monetária brasileira.
     *
     * @return valor formatado (ex: "R$ 1.234,56")
     */
    public String toBrazilianFormat() {
        return String.format("R$ %,.2f", amount);
    }

    /**
     * Retorna o valor em centavos.
     *
     * @return valor em centavos (long)
     */
    public long getCents() {
        return amount.multiply(HUNDRED).longValue();
    }

    /**
     * Verifica se o valor é zero.
     *
     * @return true se for zero
     */
    public boolean isZero() {
        return this.amount.compareTo(BigDecimal.ZERO) == 0;
    }

    /**
     * Verifica se o valor é positivo (maior que zero).
     *
     * @return true se for positivo
     */
    public boolean isPositive() {
        return this.amount.compareTo(BigDecimal.ZERO) > 0;
    }

    /**
     * Verifica se o valor é negativo.
     *
     * @return true se for negativo
     */
    public boolean isNegative() {
        return this.amount.compareTo(BigDecimal.ZERO) < 0;
    }

    /**
     * Soma dois valores monetários.
     *
     * @param other valor a ser somado
     * @return nova instância com o resultado
     */
    public Money add(Money other) {
        Objects.requireNonNull(other, "Other money cannot be null");
        return new Money(this.amount.add(other.amount));
    }

    /**
     * Subtrai dois valores monetários.
     *
     * @param other valor a ser subtraído
     * @return nova instância com o resultado
     * @throws IllegalArgumentException se o resultado for negativo
     */
    public Money subtract(Money other) {
        Objects.requireNonNull(other, "Other money cannot be null");
        BigDecimal result = this.amount.subtract(other.amount);
        return new Money(result);
    }

    /**
     * Multiplica o valor por um fator.
     *
     * @param multiplier fator de multiplicação
     * @return nova instância com o resultado
     */
    public Money multiply(BigDecimal multiplier) {
        Objects.requireNonNull(multiplier, "Multiplier cannot be null");
        return new Money(this.amount.multiply(multiplier));
    }

    /**
     * Multiplica o valor por um fator inteiro.
     *
     * @param multiplier fator de multiplicação
     * @return nova instância com o resultado
     */
    public Money multiply(int multiplier) {
        return multiply(BigDecimal.valueOf(multiplier));
    }

    /**
     * Calcula uma porcentagem do valor.
     *
     * @param percentage porcentagem (ex: 15.0 para 15%)
     * @return nova instância com o resultado
     */
    public Money percentage(double percentage) {
        return multiply(BigDecimal.valueOf(percentage).divide(HUNDRED, SCALE, RoundingMode.HALF_EVEN));
    }

    /**
     * Compara este valor com outro.
     *
     * @param other valor a ser comparado
     * @return -1, 0 ou 1 conforme este valor seja menor, igual ou maior
     */
    public int compareTo(Money other) {
        Objects.requireNonNull(other, "Other money cannot be null");
        return this.amount.compareTo(other.amount);
    }

    /**
     * Verifica se este valor é menor que outro.
     *
     * @param other valor a ser comparado
     * @return true se for menor
     */
    public boolean isLessThan(Money other) {
        return compareTo(other) < 0;
    }

    /**
     * Verifica se este valor é menor ou igual a outro.
     *
     * @param other valor a ser comparado
     * @return true se for menor ou igual
     */
    public boolean isLessThanOrEqualTo(Money other) {
        return compareTo(other) <= 0;
    }

    /**
     * Verifica se este valor é maior que outro.
     *
     * @param other valor a ser comparado
     * @return true se for maior
     */
    public boolean isGreaterThan(Money other) {
        return compareTo(other) > 0;
    }

    /**
     * Verifica se este valor é maior ou igual a outro.
     *
     * @param other valor a ser comparado
     * @return true se for maior ou igual
     */
    public boolean isGreaterThanOrEqualTo(Money other) {
        return compareTo(other) >= 0;
    }

    /**
     * Valida que o valor não seja negativo.
     *
     * @throws IllegalArgumentException se for negativo
     */
    private void validateNonNegative() {
        if (amount.compareTo(BigDecimal.ZERO) < 0) {
            throw new IllegalArgumentException("Money amount cannot be negative: " + amount);
        }
    }

    @Override
    public boolean equals(Object o) {
        if (this == o) return true;
        if (o == null || getClass() != o.getClass()) return false;
        Money money = (Money) o;
        return amount.equals(money.amount);
    }

    @Override
    public int hashCode() {
        return amount.hashCode();
    }

    @Override
    public String toString() {
        return "Money{" +
                "amount=" + amount +
                ", formatted='" + toBrazilianFormat() + '\'' +
                '}';
    }
}