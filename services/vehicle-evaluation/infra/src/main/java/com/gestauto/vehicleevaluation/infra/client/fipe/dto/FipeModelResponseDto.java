package com.gestauto.vehicleevaluation.infra.client.fipe.dto;

import com.fasterxml.jackson.annotation.JsonProperty;
import lombok.AllArgsConstructor;
import lombok.Builder;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.io.Serializable;

/**
 * DTO para resposta de modelos da API FIPE
 */
@Data
@NoArgsConstructor
@AllArgsConstructor
@Builder
public class FipeModelResponseDto implements Serializable {
    @JsonProperty("id")
    private String id;

    @JsonProperty("nome")
    private String name;
}
