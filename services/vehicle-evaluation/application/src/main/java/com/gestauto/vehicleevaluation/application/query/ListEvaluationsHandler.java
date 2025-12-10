package com.gestauto.vehicleevaluation.application.query;

import com.gestauto.vehicleevaluation.application.dto.PagedResult;
import com.gestauto.vehicleevaluation.application.dto.VehicleEvaluationSummaryDto;
import com.gestauto.vehicleevaluation.domain.entity.VehicleEvaluation;
import com.gestauto.vehicleevaluation.domain.repository.VehicleEvaluationRepository;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.stereotype.Component;

import java.util.List;
import java.util.UUID;
import java.util.stream.Collectors;

/**
 * Handler responsável por listar avaliações com filtros e paginação.
 *
 * Este handler implementa a query de listagem seguindo
 * o padrão CQRS e retorna uma lista paginada de avaliações.
 */
@Component
@RequiredArgsConstructor
@Slf4j
public class ListEvaluationsHandler implements QueryHandler<ListEvaluationsQuery, PagedResult<VehicleEvaluationSummaryDto>> {

    private final VehicleEvaluationRepository vehicleEvaluationRepository;

    @Override
    public PagedResult<VehicleEvaluationSummaryDto> handle(ListEvaluationsQuery query) throws Exception {
        log.info("Listando avaliações com filtros: evaluatorId={}, status={}, page={}, size={}",
                query.evaluatorId(), query.status(), query.page(), query.size());

        try {
            // 1. Buscar avaliações conforme filtros
            List<VehicleEvaluation> evaluations = findEvaluations(query);

            // 2. Converter para DTOs resumidos
            List<VehicleEvaluationSummaryDto> summaryDtos = evaluations.stream()
                    .map(this::convertToSummaryDto)
                    .collect(Collectors.toList());

            // 3. Calcular metadados de paginação
            long totalElements = countTotalEvaluations(query);
            int totalPages = (int) Math.ceil((double) totalElements / query.size());

            PagedResult<VehicleEvaluationSummaryDto> result = new PagedResult<>(
                    summaryDtos,
                    query.page(),
                    query.size(),
                    totalElements,
                    totalPages,
                    query.page() == 0,
                    query.page() >= totalPages - 1,
                    summaryDtos.size()
            );

            log.info("Listagem concluída. Retornando {} de {} avaliações (página {} de {})",
                    summaryDtos.size(), totalElements, query.page() + 1, totalPages);

            return result;

        } catch (Exception e) {
            log.error("Erro ao listar avaliações com filtros: {}", query, e);
            throw e;
        }
    }

    /**
     * Busca avaliações conforme filtros da query.
     *
     * @param query query com filtros
     * @return lista de avaliações encontradas
     */
    private List<VehicleEvaluation> findEvaluations(ListEvaluationsQuery query) {
        // Implementação básica - em produção usaria specification pattern
        if (query.evaluatorId() != null && query.status() != null) {
            return vehicleEvaluationRepository.findByStatusAndEvaluator(
                    query.status(),
                    query.evaluatorId().toString()
            );
        } else if (query.evaluatorId() != null) {
            return vehicleEvaluationRepository.findByEvaluator(query.evaluatorId().toString());
        } else if (query.status() != null) {
            return vehicleEvaluationRepository.findByStatus(query.status());
        } else {
            return vehicleEvaluationRepository.findAll(query.page(), query.size());
        }
    }

    /**
     * Conta total de avaliações conforme filtros.
     *
     * @param query query com filtros
     * @return número total de avaliações
     */
    private long countTotalEvaluations(ListEvaluationsQuery query) {
        // Implementação básica - em produção usaria count com filtros
        if (query.status() != null) {
            return vehicleEvaluationRepository.countByStatus(query.status());
        } else {
            // Mock - em produção implementaria count total
            return 100L; // Valor mock para exemplo
        }
    }

    /**
     * Converte entidade VehicleEvaluation para VehicleEvaluationSummaryDto.
     *
     * @param evaluation entidade de domínio
     * @return DTO resumido da avaliação
     */
    private VehicleEvaluationSummaryDto convertToSummaryDto(VehicleEvaluation evaluation) {
        return new VehicleEvaluationSummaryDto(
                UUID.fromString(evaluation.getId().getValueAsString()),
                evaluation.getPlate().getFormatted(),
                evaluation.getVehicleInfo().getBrand(),
                evaluation.getVehicleInfo().getModel(),
                evaluation.getVehicleInfo().getYearModel(),
                evaluation.getMileage().getAmount().intValue(),
                evaluation.getStatus().name(),
                evaluation.getFinalValue() != null ? evaluation.getFinalValue().getAmount() : null,
                evaluation.getEvaluatorId(),
                evaluation.getApproverId(),
                evaluation.getCreatedAt(),
                evaluation.getUpdatedAt(),
                evaluation.isExpired()
        );
    }
}