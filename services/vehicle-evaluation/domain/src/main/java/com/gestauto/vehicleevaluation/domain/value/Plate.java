package com.gestauto.vehicleevaluation.domain.value;

import java.util.Objects;
import java.util.regex.Pattern;

/**
 * Value Object para placa de veículo.
 *
 * Este Value Object encapsula a placa de veículo brasileira,
 * garantindo formatação e validação corretas (padrão Mercosul).
 */
public final class Plate {

    private static final Pattern VALID_PLATE_PATTERN =
        Pattern.compile("^[A-Z]{3}[0-99][A-Z0-9][0-9]$");

    private final String value;

    private Plate(String value) {
        this.value = normalizePlate(Objects.requireNonNull(value, "Plate cannot be null"));
        validate();
    }

    /**
     * Cria uma nova placa após validação.
     *
     * @param value placa do veículo
     * @return nova instância de Plate
     * @throws IllegalArgumentException se a placa for inválida
     */
    public static Plate of(String value) {
        return new Plate(value);
    }

    /**
     * Retorna o valor formatado da placa.
     *
     * @return placa formatada
     */
    public String getValue() {
        return value;
    }

    /**
     * Retorna a placa formatada para exibição (XXX-0000).
     *
     * @return placa formatada
     */
    public String getFormatted() {
        if (value.length() == 7) {
            return value.substring(0, 3) + "-" + value.substring(3);
        }
        return value;
    }

    /**
     * Verifica se a placa segue o padrão Mercosul.
     *
     * @return true se for válida
     */
    public boolean isValidMercosul() {
        return VALID_PLATE_PATTERN.matcher(value).matches();
    }

    /**
     * Verifica se a placa é do padrão antigo (3 letras + 4 números).
     *
     * @return true se for padrão antigo
     */
    public boolean isOldPattern() {
        return value.matches("^[A-Z]{3}[0-9]{4}$");
    }

    /**
     * Normaliza a placa removendo caracteres especiais e convertendo para maiúsculo.
     *
     * @param plate placa original
     * @return placa normalizada
     */
    private String normalizePlate(String plate) {
        return plate.toUpperCase()
                   .replaceAll("[^A-Z0-9]", "")
                   .trim();
    }

    /**
     * Valida o formato da placa.
     *
     * @throws IllegalArgumentException se a placa for inválida
     */
    private void validate() {
        if (value.isEmpty()) {
            throw new IllegalArgumentException("Plate cannot be empty");
        }

        if (value.length() < 7) {
            throw new IllegalArgumentException("Plate must have at least 7 characters");
        }

        if (value.length() > 7) {
            throw new IllegalArgumentException("Plate must have at most 7 characters");
        }

        if (!isValidMercosul() && !isOldPattern()) {
            throw new IllegalArgumentException("Invalid plate format. Expected Mercosul (ABC1D23) or old pattern (ABC1234)");
        }
    }

    @Override
    public boolean equals(Object o) {
        if (this == o) return true;
        if (o == null || getClass() != o.getClass()) return false;
        Plate plate = (Plate) o;
        return value.equals(plate.value);
    }

    @Override
    public int hashCode() {
        return value.hashCode();
    }

    @Override
    public String toString() {
        return "Plate{" +
                "value='" + value + '\'' +
                ", formatted='" + getFormatted() + '\'' +
                '}';
    }
}