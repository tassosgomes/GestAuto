package com.gestauto.vehicleevaluation.domain.value;

import com.gestauto.vehicleevaluation.domain.enums.FuelType;
import java.util.Objects;

/**
 * Value Object para informações básicas do veículo.
 *
 * Este Value Object encapsula dados fundamentais do veículo
 * como marca, modelo, ano de fabricação, modelo, cor e tipo de combustível.
 */
public final class VehicleInfo {

    private final String brand;
    private final String model;
    private final String version;
    private final int yearManufacture;
    private final int yearModel;
    private final String color;
    private final FuelType fuelType;

    private VehicleInfo(String brand, String model, String version,
                        int yearManufacture, int yearModel, String color, FuelType fuelType) {
        this.brand = Objects.requireNonNull(brand, "Brand cannot be null");
        this.model = Objects.requireNonNull(model, "Model cannot be null");
        this.version = Objects.requireNonNull(version, "Version cannot be null");
        this.color = Objects.requireNonNull(color, "Color cannot be null");
        this.fuelType = Objects.requireNonNull(fuelType, "Fuel type cannot be null");

        validateYear(yearManufacture, yearModel);

        this.yearManufacture = yearManufacture;
        this.yearModel = yearModel;

        validateFields();
    }

    /**
     * Cria uma nova instância de VehicleInfo.
     *
     * @param brand marca do veículo
     * @param model modelo do veículo
     * @param version versão/variante do modelo
     * @param yearManufacture ano de fabricação
     * @param yearModel ano do modelo
     * @param color cor do veículo
     * @param fuelType tipo de combustível
     * @return nova instância de VehicleInfo
     * @throws IllegalArgumentException se algum dado for inválido
     */
    public static VehicleInfo of(String brand, String model, String version,
                                  int yearManufacture, int yearModel, String color, FuelType fuelType) {
        return new VehicleInfo(brand, model, version, yearManufacture, yearModel, color, fuelType);
    }

    public static VehicleInfo create(String brand, String model, int yearModel, String color, FuelType fuelType, String version) {
        return of(brand, model, version, yearModel, yearModel, color, fuelType);
    }

    /**
     * Valida se os anos são consistentes.
     *
     * @param yearManufacture ano de fabricação
     * @param yearModel ano do modelo
     * @throws IllegalArgumentException se os anos forem inválidos
     */
    private void validateYear(int yearManufacture, int yearModel) {
        int currentYear = java.time.Year.now().getValue();
        int maxYear = currentYear + 2;

        if (yearManufacture < 1900 || yearManufacture > maxYear) {
            throw new IllegalArgumentException(
                String.format("Invalid year of manufacture: %d. Must be between 1900 and %d",
                yearManufacture, maxYear));
        }

        if (yearModel < yearManufacture || yearModel > maxYear) {
            throw new IllegalArgumentException(
                String.format("Invalid year model: %d. Must be between %d and %d",
                yearModel, yearManufacture, maxYear));
        }
    }

    /**
     * Valida os campos obrigatórios.
     */
    private void validateFields() {
        if (brand.trim().isEmpty()) {
            throw new IllegalArgumentException("Brand cannot be empty");
        }

        if (model.trim().isEmpty()) {
            throw new IllegalArgumentException("Model cannot be empty");
        }

        if (version.trim().isEmpty()) {
            throw new IllegalArgumentException("Version cannot be empty");
        }

        if (color.trim().isEmpty()) {
            throw new IllegalArgumentException("Color cannot be empty");
        }
    }

    /**
     * Retorna o nome completo do veículo (marca + modelo).
     *
     * @return nome completo
     */
    public String getFullName() {
        return brand + " " + model;
    }

    /**
     * Retorna a descrição completa incluindo versão.
     *
     * @return descrição completa
     */
    public String getFullDescription() {
        return brand + " " + model + " " + version;
    }

    /**
     * Retorna a idade do veículo em anos.
     *
     * @return idade em anos
     */
    public int getAge() {
        return java.time.Year.now().getValue() - yearManufacture;
    }

    /**
     * Verifica se é um veículo zero km.
     *
     * @return true se for zero km
     */
    public boolean isZeroKm() {
        int currentYear = java.time.Year.now().getValue();
        return yearModel == currentYear && yearManufacture == currentYear;
    }

    /**
     * Verifica se o modelo é mais recente que a fabricação.
     *
     * @return true se o modelo for mais recente
     */
    public boolean isNewerModel() {
        return yearModel > yearManufacture;
    }

    /**
     * Verifica se é um veículo clássico (mais de 30 anos).
     *
     * @return true se for clássico
     */
    public boolean isClassic() {
        return getAge() >= 30;
    }

    // Getters
    public String getBrand() {
        return brand;
    }

    public String getModel() {
        return model;
    }

    public String getVersion() {
        return version;
    }

    public int getYearManufacture() {
        return yearManufacture;
    }

    public int getYearModel() {
        return yearModel;
    }

    public String getColor() {
        return color;
    }

    public FuelType getFuelType() {
        return fuelType;
    }

    @Override
    public boolean equals(Object o) {
        if (this == o) return true;
        if (o == null || getClass() != o.getClass()) return false;
        VehicleInfo that = (VehicleInfo) o;
        return yearManufacture == that.yearManufacture &&
                yearModel == that.yearModel &&
                brand.equals(that.brand) &&
                model.equals(that.model) &&
                version.equals(that.version) &&
                color.equals(that.color) &&
                fuelType == that.fuelType;
    }

    @Override
    public int hashCode() {
        return Objects.hash(brand, model, version, yearManufacture, yearModel, color, fuelType);
    }

    @Override
    public String toString() {
        return "VehicleInfo{" +
                "brand='" + brand + '\'' +
                ", model='" + model + '\'' +
                ", version='" + version + '\'' +
                ", yearManufacture=" + yearManufacture +
                ", yearModel=" + yearModel +
                ", color='" + color + '\'' +
                ", fuelType=" + fuelType +
                '}';
    }
}