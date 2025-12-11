package com.gestauto.vehicleevaluation.api;

import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.boot.autoconfigure.domain.EntityScan;
import org.springframework.data.jpa.repository.config.EnableJpaRepositories;
import org.springframework.scheduling.annotation.EnableAsync;

/**
 * Classe principal da aplicação Vehicle Evaluation Service.
 *
 * Esta classe serve como ponto de entrada para o microserviço de avaliação
 * de veículos seminovos do GestAuto.
 *
 * @author GestAuto Team
 * @version 1.0.0
 */
@SpringBootApplication(scanBasePackages = {
    "com.gestauto.vehicleevaluation.api",
    "com.gestauto.vehicleevaluation.application",
    "com.gestauto.vehicleevaluation.infra"
})
@EnableJpaRepositories(basePackages = "com.gestauto.vehicleevaluation.infra")
@EntityScan(basePackages = "com.gestauto.vehicleevaluation.infra")
@EnableAsync
public class VehicleEvaluationApplication {

    public static void main(String[] args) {
        SpringApplication.run(VehicleEvaluationApplication.class, args);
    }
}