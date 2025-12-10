package com.gestauto.vehicleevaluation.domain.value;

import java.util.Objects;
import java.util.UUID;

/**
 * Value Object para identificador único de uma avaliação.
 *
 * Este Value Object encapsula o identificador único de uma avaliação,
 * garantindo imutabilidade e validação.
 */
public final class EvaluationId {

    private final UUID value;

    private EvaluationId(UUID value) {
        this.value = Objects.requireNonNull(value, "EvaluationId cannot be null");
    }

    /**
     * Cria um novo EvaluationId com UUID gerado automaticamente.
     *
     * @return novo EvaluationId
     */
    public static EvaluationId generate() {
        return new EvaluationId(UUID.randomUUID());
    }

    /**
     * Cria um EvaluationId a partir de um UUID existente.
     *
     * @param value UUID existente
     * @return novo EvaluationId
     */
    public static EvaluationId from(UUID value) {
        return new EvaluationId(value);
    }

    /**
     * Cria um EvaluationId a partir de uma string UUID.
     *
     * @param value string no formato UUID
     * @return novo EvaluationId
     * @throws IllegalArgumentException se a string for inválida
     */
    public static EvaluationId from(String value) {
        try {
            return new EvaluationId(UUID.fromString(value));
        } catch (IllegalArgumentException e) {
            throw new IllegalArgumentException("Invalid UUID string: " + value, e);
        }
    }

    /**
     * Retorna o valor UUID.
     *
     * @return valor UUID
     */
    public UUID getValue() {
        return value;
    }

    /**
     * Retorna o valor como string.
     *
     * @return UUID como string
     */
    public String getValueAsString() {
        return value.toString();
    }

    @Override
    public boolean equals(Object o) {
        if (this == o) return true;
        if (o == null || getClass() != o.getClass()) return false;
        EvaluationId that = (EvaluationId) o;
        return value.equals(that.value);
    }

    @Override
    public int hashCode() {
        return value.hashCode();
    }

    @Override
    public String toString() {
        return "EvaluationId{" +
                "value=" + value +
                '}';
    }
}