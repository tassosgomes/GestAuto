package com.gestauto.vehicleevaluation.infra.client.fipe.dto;

import com.fasterxml.jackson.annotation.JsonProperty;
import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.io.Serializable;

/**
 * DTO para resposta de preço de veículo da API FIPE
 */
@Data
@NoArgsConstructor
@AllArgsConstructor
@Builder
public class FipeVehicleResponseDto implements Serializable {
    @JsonProperty("valor")
    private String value;

    @JsonProperty("marca")
    private String brand;

    @JsonProperty("modelo")
    private String model;

    @JsonProperty("anoModelo")
    private String modelYear;

    @JsonProperty("combustivel")
    private String fuel;

    @JsonProperty("mesReferencia")
    private String referenceMonth;

    @JsonProperty("tipoVeiculo")
    private Integer vehicleType; // 1 = Carro, 2 = Moto, 3 = Caminhão
}
