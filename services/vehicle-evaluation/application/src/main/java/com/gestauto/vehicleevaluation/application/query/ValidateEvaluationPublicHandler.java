package com.gestauto.vehicleevaluation.application.query;

import com.gestauto.vehicleevaluation.application.dto.EvaluationValidationDto;
import com.gestauto.vehicleevaluation.domain.entity.VehicleEvaluation;
import com.gestauto.vehicleevaluation.domain.repository.VehicleEvaluationRepository;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.stereotype.Component;

import java.time.LocalDateTime;

@Component
@RequiredArgsConstructor
@Slf4j
public class ValidateEvaluationPublicHandler implements QueryHandler<ValidateEvaluationPublicQuery, EvaluationValidationDto> {

    private final VehicleEvaluationRepository evaluationRepository;

    @Override
    public EvaluationValidationDto handle(ValidateEvaluationPublicQuery query) {
        log.info("Validating evaluation report publicly via token");

        VehicleEvaluation evaluation = evaluationRepository.findByValidationToken(query.token())
            .orElse(null);

        if (evaluation == null) {
            return null;
        }

        if (evaluation.getValidUntil() == null || evaluation.getValidUntil().isBefore(LocalDateTime.now())) {
            return null;
        }

        return new EvaluationValidationDto(
            evaluation.getPlate().getFormatted(),
            evaluation.getVehicleInfo().getBrand(),
            evaluation.getVehicleInfo().getModel(),
            evaluation.getVehicleInfo().getYearModel(),
            evaluation.getStatus().name(),
            evaluation.getApprovedValue() != null ? evaluation.getApprovedValue().getAmount() : null,
            LocalDateTime.now(),
            evaluation.getValidUntil()
        );
    }
}
