package com.gestauto.vehicleevaluation.application.query;

import com.gestauto.vehicleevaluation.application.dto.VehicleEvaluationDto;
import com.gestauto.vehicleevaluation.domain.exception.DomainException;
import com.gestauto.vehicleevaluation.domain.repository.VehicleEvaluationRepository;
import com.gestauto.vehicleevaluation.domain.value.EvaluationId;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.stereotype.Component;

/**
 * Handler responsável por buscar uma avaliação específica por ID.
 *
 * Este handler implementa a query de busca por ID seguindo
 * o padrão CQRS e retorna os dados completos da avaliação.
 */
@Component
@RequiredArgsConstructor
@Slf4j
public class GetEvaluationHandler implements QueryHandler<GetEvaluationQuery, VehicleEvaluationDto> {

    private final VehicleEvaluationRepository vehicleEvaluationRepository;

    @Override
    public VehicleEvaluationDto handle(GetEvaluationQuery query) throws Exception {
        log.info("Buscando avaliação ID: {}", query.evaluationId());

        try {
            // 1. Buscar avaliação por ID
            EvaluationId evaluationId = EvaluationId.from(query.evaluationId().toString());
            var evaluationOpt = vehicleEvaluationRepository.findById(evaluationId);

            if (evaluationOpt.isEmpty()) {
                throw new DomainException("Avaliação não encontrada: " + evaluationId.getValueAsString());
            }

            var evaluation = evaluationOpt.get();

            // 2. Converter para DTO (será implementado no mapper)
            VehicleEvaluationDto dto = convertToDto(evaluation);

            log.info("Avaliação encontrada com sucesso. ID: {}, Status: {}",
                    evaluation.getId().getValueAsString(), evaluation.getStatus());

            return dto;

        } catch (Exception e) {
            log.error("Erro ao buscar avaliação ID: {}", query.evaluationId(), e);
            throw e;
        }
    }

    /**
     * Converte entidade VehicleEvaluation para VehicleEvaluationDto.
     *
     * @param evaluation entidade de domínio
     * @return DTO com dados da avaliação
     */
    private VehicleEvaluationDto convertToDto(com.gestauto.vehicleevaluation.domain.entity.VehicleEvaluation evaluation) {
        // Conversão temporária - será movida para o mapper
        return new VehicleEvaluationDto(
                java.util.UUID.fromString(evaluation.getId().getValueAsString()),
                evaluation.getPlate().getFormatted(),
                evaluation.getRenavam(),
                evaluation.getVehicleInfo().getBrand(),
                evaluation.getVehicleInfo().getModel(),
                evaluation.getVehicleInfo().getYearModel(),
                evaluation.getMileage().getAmount().intValue(),
                evaluation.getVehicleInfo().getColor(),
                evaluation.getVehicleInfo().getVersion(),
                evaluation.getVehicleInfo().getFuelType().getDescription(),
                "MANUAL", // Mock - implementar campo adequado
                java.util.List.of(), // Mock accessories
                evaluation.getStatus().name(),
                evaluation.getFipePrice() != null ? evaluation.getFipePrice().getAmount() : null,
                evaluation.getBaseValue() != null ? evaluation.getBaseValue().getAmount() : null,
                evaluation.getFinalValue() != null ? evaluation.getFinalValue().getAmount() : null,
                evaluation.getApprovedValue() != null ? evaluation.getApprovedValue().getAmount() : null,
                evaluation.getObservations(),
                evaluation.getJustification(),
                evaluation.getEvaluatorId(),
                evaluation.getApproverId(),
                evaluation.getCreatedAt(),
                evaluation.getUpdatedAt(),
                evaluation.getSubmittedAt(),
                evaluation.getApprovedAt(),
                evaluation.getValidUntil(),
                evaluation.getValidationToken(),
                java.util.List.of(), // Mock photos
                java.util.List.of(), // Mock depreciationItems
                null // Mock checklist
        );
    }
}