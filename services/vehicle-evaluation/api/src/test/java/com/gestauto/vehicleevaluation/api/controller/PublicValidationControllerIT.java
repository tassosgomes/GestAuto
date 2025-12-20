package com.gestauto.vehicleevaluation.api.controller;

import com.gestauto.vehicleevaluation.api.VehicleEvaluationApplication;
import com.gestauto.vehicleevaluation.api.support.IntegrationTestContainers;
import com.gestauto.vehicleevaluation.infra.entity.EvaluationStatusJpa;
import com.gestauto.vehicleevaluation.infra.entity.VehicleEvaluationJpaEntity;
import com.gestauto.vehicleevaluation.infra.entity.VehicleInfoEmbeddable;
import com.gestauto.vehicleevaluation.infra.repository.VehicleEvaluationJpaRepository;
import java.math.BigDecimal;
import java.time.LocalDateTime;
import java.util.UUID;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.autoconfigure.web.servlet.AutoConfigureMockMvc;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.test.web.servlet.MockMvc;

import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.get;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.jsonPath;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.status;

@SpringBootTest(
    classes = VehicleEvaluationApplication.class,
    properties = {
        "spring.autoconfigure.exclude="
            + "org.springframework.boot.autoconfigure.data.redis.RedisAutoConfiguration,"
            + "org.springframework.boot.autoconfigure.data.redis.RedisRepositoriesAutoConfiguration,"
            + "org.springframework.boot.autoconfigure.security.servlet.SecurityAutoConfiguration,"
            + "org.springframework.boot.autoconfigure.security.servlet.SecurityFilterAutoConfiguration,"
            + "org.springframework.boot.autoconfigure.security.oauth2.resource.servlet.OAuth2ResourceServerAutoConfiguration"
    }
)
@AutoConfigureMockMvc(addFilters = false)
class PublicValidationControllerIT extends IntegrationTestContainers {

    @Autowired
    MockMvc mockMvc;

    @Autowired
    VehicleEvaluationJpaRepository jpaRepository;

    @Test
    void validateReturns404WhenTokenNotFound() throws Exception {
        mockMvc.perform(get("/api/v1/evaluations/public/validate/{token}", "missing-token"))
            .andExpect(status().isNotFound());
    }

    @Test
    void validateReturns200WhenTokenIsValidAndNotExpired() throws Exception {
        String token = "token-" + UUID.randomUUID();

        VehicleEvaluationJpaEntity entity = new VehicleEvaluationJpaEntity();
        entity.setId(UUID.randomUUID());
        entity.setPlate("ABC1234");
        entity.setRenavam("12345678901");
        entity.setEvaluatorId("evaluator-1");
        entity.setStatus(EvaluationStatusJpa.APPROVED);
        entity.setVehicleInfo(new VehicleInfoEmbeddable(
            "Volkswagen",
            "Gol",
            2022,
            2022,
            "Preto",
            "FLEX",
            "1.0",
            "Manual",
            "4",
            "1.0",
            "9BWZZZ377VT004251"
        ));
        entity.setMileageAmount(new BigDecimal("50000.00"));
        entity.setMileageCurrency("BRL");
        entity.setApprovedValueAmount(new BigDecimal("45000.00"));
        entity.setApprovedValueCurrency("BRL");
        entity.setValidUntil(LocalDateTime.now().plusHours(24));
        entity.setValidationToken(token);

        jpaRepository.saveAndFlush(entity);

        mockMvc.perform(get("/api/v1/evaluations/public/validate/{token}", token))
            .andExpect(status().isOk())
            .andExpect(jsonPath("$.plate").value("ABC1234"))
            .andExpect(jsonPath("$.brand").value("Volkswagen"))
            .andExpect(jsonPath("$.model").value("Gol"))
            .andExpect(jsonPath("$.year").value(2022))
            .andExpect(jsonPath("$.status").value("APPROVED"))
            .andExpect(jsonPath("$.approvedValue").value(45000.00));
    }
}
