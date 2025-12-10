package com.gestauto.vehicleevaluation.application.service;

import com.gestauto.vehicleevaluation.domain.value.Money;
import com.gestauto.vehicleevaluation.domain.value.VehicleInfo;
import com.gestauto.vehicleevaluation.domain.enums.FuelType;

import java.util.Optional;

/**
 * Serviço para integração com a tabela FIPE (Fundação Instituto de Pesquisas Econômicas).
 *
 * Responsável por obter informações de mercado e precificação de veículos
 * baseados na tabela FIPE, que é a referência nacional para preços de veículos.
 */
public interface FipeService {

    /**
     * Busca informações do veículo na tabela FIPE baseado na placa.
     *
     * @param plate placa do veículo
     * @return Optional com VehicleInfo se encontrado, Optional.empty() caso contrário
     */
    Optional<VehicleInfo> getVehicleInfoByPlate(String plate);

    /**
     * Busca informações do veículo na tabela FIPE baseado em código FIPE.
     *
     * @param fipeCode código FIPE do veículo
     * @return Optional com VehicleInfo se encontrado, Optional.empty() caso contrário
     */
    Optional<VehicleInfo> getVehicleInfoByFipeCode(String fipeCode);

    /**
     * Busca preço de mercado FIPE para o veículo.
     *
     * @param brand marca do veículo
     * @param model modelo do veículo
     * @param year ano do modelo
     * @param fuelType tipo de combustível
     * @return Optional com preço FIPE se encontrado, Optional.empty() caso contrário
     */
    Optional<Money> getFipePrice(String brand, String model, int year, FuelType fuelType);

    /**
     * Valida se o código FIPE informado é válido.
     *
     * @param fipeCode código a ser validado
     * @return true se for um código FIPE válido
     */
    boolean isValidFipeCode(String fipeCode);

    /**
     * Busca informações detalhadas incluindo ano de fabricação e modelo.
     *
     * @param brand marca do veículo
     * @param model modelo do veículo
     * @param yearManufacture ano de fabricação
     * @param yearModel ano do modelo
     * @param fuelType tipo de combustível
     * @return Optional com VehicleInfo completo se encontrado
     */
    Optional<VehicleInfo> getDetailedVehicleInfo(String brand, String model,
                                               int yearManufacture, int yearModel,
                                               FuelType fuelType);

    /**
     * Calcula percentual de liquidez típico para o veículo.
     *
     * @param brand marca do veículo
     * @param model modelo do veículo
     * @param age idade do veículo em anos
     * @return percentual de liquidez (ex: 0.82 para 82%)
     */
    double calculateLiquidityPercentage(String brand, String model, int age);

    /**
     * Verifica se o veículo tem boa aceitação no mercado.
     *
     * @param brand marca do veículo
     * @param model modelo do veículo
     * @param year ano do modelo
     * @return true se for um veículo com boa demanda
     */
    boolean hasGoodMarketAcceptance(String brand, String model, int year);
}