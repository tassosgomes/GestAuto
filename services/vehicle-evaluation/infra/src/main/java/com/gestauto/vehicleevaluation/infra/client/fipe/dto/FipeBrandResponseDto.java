package com.gestauto.vehicleevaluation.infra.client.fipe.dto;

import com.fasterxml.jackson.annotation.JsonProperty;
import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.io.Serializable;
import java.util.List;

/**
 * DTO para resposta de marcas da API FIPE
 */
@Data
@NoArgsConstructor
@AllArgsConstructor
@Builder
public class FipeBrandResponseDto implements Serializable {
    @JsonProperty("id")
    private String id;

    @JsonProperty("nome")
    private String name;
}
