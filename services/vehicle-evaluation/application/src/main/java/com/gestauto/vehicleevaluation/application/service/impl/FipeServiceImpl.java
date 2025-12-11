package com.gestauto.vehicleevaluation.application.service.impl;

import com.gestauto.vehicleevaluation.application.service.FipeService;
import com.gestauto.vehicleevaluation.domain.value.Money;
import com.gestauto.vehicleevaluation.domain.value.VehicleInfo;
import com.gestauto.vehicleevaluation.domain.enums.FuelType;
import org.springframework.cache.annotation.Cacheable;
import org.springframework.stereotype.Service;

import java.math.BigDecimal;
import java.time.Year;
import java.util.HashMap;
import java.util.Map;
import java.util.Optional;

/**
 * Implementação mock do serviço FIPE para desenvolvimento e testes.
 *
 * Esta implementação simula o comportamento da API FIPE com dados
 * predefinidos para facilitar o desenvolvimento sem depender da API externa.
 *
 * Utiliza Spring Cache abstraction com Redis backend para cache de 24h
 * das informações e preços de veículos.
 */
@Service
public class FipeServiceImpl implements FipeService {

    // Mock database de veículos comuns no Brasil
    private static final Map<String, VehicleInfo> MOCK_VEHICLES = new HashMap<>();
    private static final Map<String, Money> MOCK_PRICES = new HashMap<>();

    static {
        // Carros populares - Hatches
        MOCK_VEHICLES.put("ABC1234", VehicleInfo.of("Volkswagen", "Gol", "1.0 MSI", 2022, 2023, "Branco", FuelType.FLEX));
        MOCK_VEHICLES.put("DEF5678", VehicleInfo.of("Chevrolet", "Onix", "LT 1.0", 2023, 2023, "Prata", FuelType.FLEX));
        MOCK_VEHICLES.put("GHI9012", VehicleInfo.of("Fiat", "Palio", "1.0 EVO", 2021, 2022, "Vermelho", FuelType.FLEX));
        MOCK_VEHICLES.put("JKL3456", VehicleInfo.of("Ford", "Ka", "1.0 SE", 2022, 2023, "Preto", FuelType.FLEX));

        // Sedans
        MOCK_VEHICLES.put("MNO7890", VehicleInfo.of("Toyota", "Corolla", "2.0 XEI", 2023, 2023, "Prata", FuelType.FLEX));
        MOCK_VEHICLES.put("PQR2345", VehicleInfo.of("Honda", "Civic", "1.5 EXL", 2023, 2024, "Branco", FuelType.FLEX));
        MOCK_VEHICLES.put("STU6789", VehicleInfo.of("Volkswagen", "Voyage", "1.0 MSI", 2022, 2023, "Azul", FuelType.FLEX));

        // SUVs
        MOCK_VEHICLES.put("VWX0123", VehicleInfo.of("Jeep", "Renegade", "1.8 4x2", 2023, 2023, "Preto", FuelType.FLEX));
        MOCK_VEHICLES.put("YZA4567", VehicleInfo.of("Hyundai", "Creta", "1.0 Turbo", 2023, 2024, "Branco", FuelType.FLEX));
        MOCK_VEHICLES.put("BCD8901", VehicleInfo.of("Nissan", "Kicks", "1.6 4x2", 2022, 2023, "Prata", FuelType.FLEX));

        // Preços mock (valores aproximados para 2023)
        MOCK_PRICES.put("VOLKSWAGEN-Gol-2023", Money.of(BigDecimal.valueOf(75000)));
        MOCK_PRICES.put("CHEVROLET-Onix-2023", Money.of(BigDecimal.valueOf(80000)));
        MOCK_PRICES.put("FIAT-Palio-2022", Money.of(BigDecimal.valueOf(65000)));
        MOCK_PRICES.put("FORD-Ka-2023", Money.of(BigDecimal.valueOf(70000)));
        MOCK_PRICES.put("TOYOTA-Corolla-2023", Money.of(BigDecimal.valueOf(150000)));
        MOCK_PRICES.put("HONDA-Civic-2024", Money.of(BigDecimal.valueOf(160000)));
        MOCK_PRICES.put("VOLKSWAGEN-Voyage-2023", Money.of(BigDecimal.valueOf(85000)));
        MOCK_PRICES.put("JEEP-Renegade-2023", Money.of(BigDecimal.valueOf(140000)));
        MOCK_PRICES.put("HYUNDAI-Creta-2024", Money.of(BigDecimal.valueOf(130000)));
        MOCK_PRICES.put("NISSAN-Kicks-2023", Money.of(BigDecimal.valueOf(125000)));
    }

    @Override
    public Optional<VehicleInfo> getVehicleInfoByPlate(String plate) {
        if (plate == null || plate.trim().isEmpty()) {
            return Optional.empty();
        }

        String normalizedPlate = plate.replaceAll("[^A-Z0-9]", "").toUpperCase();
        VehicleInfo vehicleInfo = MOCK_VEHICLES.get(normalizedPlate);

        // Se não encontrar no mock, gera dados genéricos para simulação
        if (vehicleInfo == null) {
            vehicleInfo = generateGenericVehicleInfo(normalizedPlate);
        }

        return Optional.of(vehicleInfo);
    }

    @Override
    public Optional<VehicleInfo> getVehicleInfoByFipeCode(String fipeCode) {
        // Mock implementation - would normally call FIPE API
        if (fipeCode == null || fipeCode.trim().isEmpty()) {
            return Optional.empty();
        }

        // Gera dados genéricos baseados no código
        return Optional.of(generateGenericVehicleInfo(fipeCode));
    }

    @Override
    @Cacheable(value = "fipe-prices", key = "#brand.concat('-').concat(#model).concat('-').concat(#year)", cacheManager = "redisCacheManager")
    public Optional<Money> getFipePrice(String brand, String model, int year, FuelType fuelType) {
        String key = brand.toUpperCase() + "-" + model.toUpperCase() + "-" + year;
        Money price = MOCK_PRICES.get(key);

        if (price == null) {
            // Gera preço baseado em categoria do veículo
            price = generateGenericPrice(brand, model, year);
        }

        return Optional.of(price);
    }

    @Override
    public boolean isValidFipeCode(String fipeCode) {
        return fipeCode != null && fipeCode.matches("\\d{9}");
    }

    @Override
    public Optional<VehicleInfo> getDetailedVehicleInfo(String brand, String model,
                                                        int yearManufacture, int yearModel,
                                                        FuelType fuelType) {
        // Simula busca detalhada
        String version = determineVersion(brand, model);
        String color = "Branco"; // Default color

        VehicleInfo vehicleInfo = VehicleInfo.of(brand, model, version,
                                               yearManufacture, yearModel, color, fuelType);

        return Optional.of(vehicleInfo);
    }

    @Override
    public double calculateLiquidityPercentage(String brand, String model, int age) {
        // Simula cálculo baseado em popularidade e idade
        double baseLiquidity = 0.85; // 85% base

        // Ajuste por marca
        if (brand.equalsIgnoreCase("Toyota") || brand.equalsIgnoreCase("Honda")) {
            baseLiquidity += 0.10; // +10%
        } else if (brand.equalsIgnoreCase("Jeep") || brand.equalsIgnoreCase("Hyundai")) {
            baseLiquidity += 0.05; // +5%
        }

        // Ajuste por idade
        baseLiquidity -= (age * 0.02); // -2% por ano

        // Limites mínimo e máximo
        return Math.max(0.60, Math.min(0.95, baseLiquidity));
    }

    @Override
    public boolean hasGoodMarketAcceptance(String brand, String model, int year) {
        int currentYear = Year.now().getValue();
        int age = currentYear - year;

        // Marcas com boa aceitação
        boolean goodBrand = brand.equalsIgnoreCase("Toyota") ||
                           brand.equalsIgnoreCase("Honda") ||
                           brand.equalsIgnoreCase("Hyundai") ||
                           brand.equalsIgnoreCase("Jeep");

        // Veículos muito velhos têm menos aceitação
        boolean notTooOld = age <= 10;

        return goodBrand && notTooOld;
    }

    private VehicleInfo generateGenericVehicleInfo(String identifier) {
        // Gera dados genéricos para simulação
        String[] brands = {"Volkswagen", "Chevrolet", "Fiat", "Ford", "Toyota", "Honda"};
        String[] models = {"Modelo Base", "Compact", "Sedan", "Hatch", "SUV"};

        String brand = brands[Math.abs(identifier.hashCode()) % brands.length];
        String model = models[Math.abs(identifier.hashCode() / 2) % models.length];
        String version = "1.0";
        int year = 2023;
        FuelType fuelType = FuelType.FLEX;
        String color = "Branco";

        return VehicleInfo.of(brand, model, version, year, year, color, fuelType);
    }

    private Money generateGenericPrice(String brand, String model, int year) {
        int currentYear = Year.now().getValue();
        int age = currentYear - year;

        // Preço base por categoria
        BigDecimal basePrice;
        if (brand.equalsIgnoreCase("Toyota") || brand.equalsIgnoreCase("Honda")) {
            basePrice = BigDecimal.valueOf(120000); // Sedans premium
        } else if (brand.equalsIgnoreCase("Jeep") || brand.equalsIgnoreCase("Hyundai")) {
            basePrice = BigDecimal.valueOf(110000); // SUVs
        } else {
            basePrice = BigDecimal.valueOf(70000); // Carros populares
        }

        // Depreciação por idade
        double depreciation = 1.0 - (age * 0.08);
        depreciation = Math.max(0.30, depreciation); // Mínimo 30% do valor original

        BigDecimal finalPrice = basePrice.multiply(BigDecimal.valueOf(depreciation));
        return Money.of(finalPrice);
    }

    private String determineVersion(String brand, String model) {
        // Determina versão baseada em regras simples
        if (brand.equalsIgnoreCase("Toyota") || brand.equalsIgnoreCase("Honda")) {
            return "2.0 EXL";
        } else if (brand.equalsIgnoreCase("Jeep")) {
            return "1.8 4x2";
        } else {
            return "1.0 MSI";
        }
    }
}