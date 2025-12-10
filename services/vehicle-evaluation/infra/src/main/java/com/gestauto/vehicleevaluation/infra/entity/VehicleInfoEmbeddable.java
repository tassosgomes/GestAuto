package com.gestauto.vehicleevaluation.infra.entity;

import jakarta.persistence.Column;
import jakarta.persistence.Embeddable;
import java.time.Year;

/**
 * Embeddable JPA para informações do veículo.
 *
 * Representa o Value Object VehicleInfo do domínio
 * como um componente persistente do JPA.
 */
@Embeddable
public class VehicleInfoEmbeddable {

    @Column(name = "brand", nullable = false, length = 50)
    private String brand;

    @Column(name = "model", nullable = false, length = 100)
    private String model;

    @Column(name = "year_manufacture", nullable = false)
    private Integer yearManufacture;

    @Column(name = "year_model", nullable = false)
    private Integer yearModel;

    @Column(name = "color", nullable = false, length = 30)
    private String color;

    @Column(name = "fuel_type", nullable = false, length = 20)
    private String fuelType;

    @Column(name = "engine", length = 50)
    private String engine;

    @Column(name = "transmission", length = 50)
    private String transmission;

    @Column(name = "doors", length = 20)
    private String doors;

    @Column(name = "version", length = 100)
    private String version;

    @Column(name = "chassis", length = 17)
    private String chassis;

    // Construtores
    public VehicleInfoEmbeddable() {
    }

    public VehicleInfoEmbeddable(String brand, String model, Integer yearManufacture,
                                Integer yearModel, String color, String fuelType,
                                String engine, String transmission, String doors,
                                String version, String chassis) {
        this.brand = brand;
        this.model = model;
        this.yearManufacture = yearManufacture;
        this.yearModel = yearModel;
        this.color = color;
        this.fuelType = fuelType;
        this.engine = engine;
        this.transmission = transmission;
        this.doors = doors;
        this.version = version;
        this.chassis = chassis;
    }

    // Getters e Setters
    public String getBrand() {
        return brand;
    }

    public void setBrand(String brand) {
        this.brand = brand;
    }

    public String getModel() {
        return model;
    }

    public void setModel(String model) {
        this.model = model;
    }

    public Integer getYearManufacture() {
        return yearManufacture;
    }

    public void setYearManufacture(Integer yearManufacture) {
        this.yearManufacture = yearManufacture;
    }

    public Integer getYearModel() {
        return yearModel;
    }

    public void setYearModel(Integer yearModel) {
        this.yearModel = yearModel;
    }

    public String getColor() {
        return color;
    }

    public void setColor(String color) {
        this.color = color;
    }

    public String getFuelType() {
        return fuelType;
    }

    public void setFuelType(String fuelType) {
        this.fuelType = fuelType;
    }

    public String getEngine() {
        return engine;
    }

    public void setEngine(String engine) {
        this.engine = engine;
    }

    public String getTransmission() {
        return transmission;
    }

    public void setTransmission(String transmission) {
        this.transmission = transmission;
    }

    public String getDoors() {
        return doors;
    }

    public void setDoors(String doors) {
        this.doors = doors;
    }

    public String getVersion() {
        return version;
    }

    public void setVersion(String version) {
        this.version = version;
    }

    public String getChassis() {
        return chassis;
    }

    public void setChassis(String chassis) {
        this.chassis = chassis;
    }

    @Override
    public boolean equals(Object o) {
        if (this == o) return true;
        if (o == null || getClass() != o.getClass()) return false;

        VehicleInfoEmbeddable that = (VehicleInfoEmbeddable) o;

        if (!brand.equals(that.brand)) return false;
        if (!model.equals(that.model)) return false;
        if (!yearManufacture.equals(that.yearManufacture)) return false;
        if (!yearModel.equals(that.yearModel)) return false;
        if (!color.equals(that.color)) return false;
        return fuelType.equals(that.fuelType);
    }

    @Override
    public int hashCode() {
        int result = brand.hashCode();
        result = 31 * result + model.hashCode();
        result = 31 * result + yearManufacture.hashCode();
        result = 31 * result + yearModel.hashCode();
        result = 31 * result + color.hashCode();
        result = 31 * result + fuelType.hashCode();
        return result;
    }
}