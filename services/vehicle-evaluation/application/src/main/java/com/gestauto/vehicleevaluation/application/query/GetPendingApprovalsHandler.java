package com.gestauto.vehicleevaluation.application.query;

import com.gestauto.vehicleevaluation.application.dto.PagedResult;
import com.gestauto.vehicleevaluation.application.dto.PendingEvaluationSummaryDto;
import com.gestauto.vehicleevaluation.domain.entity.VehicleEvaluation;
import com.gestauto.vehicleevaluation.domain.enums.EvaluationStatus;
import com.gestauto.vehicleevaluation.domain.repository.VehicleEvaluationRepository;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.stereotype.Component;

import java.time.LocalDateTime;
import java.time.temporal.ChronoUnit;
import java.util.Comparator;
import java.util.List;
import java.util.stream.Collectors;

/**
 * Handler responsável por obter avaliações pendentes de aprovação.
 *
 * Este handler implementa a query de pendências seguindo
 * o padrão CQRS e retorna uma lista paginada priorizada.
 */
@Component
@RequiredArgsConstructor
@Slf4j
public class GetPendingApprovalsHandler implements QueryHandler<GetPendingApprovalsQuery, PagedResult<PendingEvaluationSummaryDto>> {

    private final VehicleEvaluationRepository evaluationRepository;

    @Override
    public PagedResult<PendingEvaluationSummaryDto> handle(GetPendingApprovalsQuery query) {
        log.info("Obtendo avaliações pendentes de aprovação: page={}, size={}, sortBy={}, sortDescending={}",
                query.page(), query.size(), query.sortBy(), query.sortDescending());

        try {
            // 1. Buscar avaliações pendentes (busca todas para ordenação em memória)
            List<VehicleEvaluation> allPending = evaluationRepository.findPendingApprovals(
                EvaluationStatus.PENDING_APPROVAL, 0, 1000); // TODO: implementar paginação eficiente

            // 2. Aplicar ordenação
            List<VehicleEvaluation> sortedEvaluations = sortEvaluations(allPending, query);

            // 3. Aplicar paginação manual
            int start = query.page() * query.size();
            int end = Math.min(start + query.size(), sortedEvaluations.size());
            List<VehicleEvaluation> pageContent = sortedEvaluations.subList(start, end);

            // 4. Converter para DTOs
            List<PendingEvaluationSummaryDto> summaries = pageContent.stream()
                .map(this::toSummaryDto)
                .collect(Collectors.toList());

            // 5. Calcular total de páginas
            int totalPages = (int) Math.ceil((double) sortedEvaluations.size() / query.size());
            long totalElements = sortedEvaluations.size();

            // 6. Retornar resultado paginado
            PagedResult<PendingEvaluationSummaryDto> result = new PagedResult<>(
                summaries,
                query.page(),
                query.size(),
                totalElements,
                totalPages,
                query.page() == 0,
                query.page() >= totalPages - 1,
                summaries.size()
            );

            log.info("Pendências obtidas. Retornando {} avaliações de {} total",
                    summaries.size(), totalElements);

            return result;

        } catch (Exception e) {
            log.error("Erro ao obter pendências de aprovação: {}", query, e);
            throw e;
        }
    }

    /**
     * Ordena avaliações conforme query.
     *
     * @param evaluations lista de avaliações
     * @param query query com parâmetros de ordenação
     * @return lista ordenada
     */
    private List<VehicleEvaluation> sortEvaluations(List<VehicleEvaluation> evaluations, GetPendingApprovalsQuery query) {
        Comparator<VehicleEvaluation> comparator;

        switch (query.sortBy()) {
            case "finalValue":
                comparator = Comparator.comparing(
                    (VehicleEvaluation e) -> e.getFinalValue() != null ? e.getFinalValue().getAmount() : java.math.BigDecimal.ZERO,
                    Comparator.reverseOrder()
                );
                break;
            case "createdAt":
                comparator = Comparator.comparing(VehicleEvaluation::getCreatedAt);
                break;
            default:
                comparator = Comparator.comparing(VehicleEvaluation::getCreatedAt);
        }

        if (!query.sortDescending()) {
            comparator = comparator.reversed();
        }

        return evaluations.stream()
            .sorted(comparator)
            .collect(Collectors.toList());
    }

    /**
     * Converte VehicleEvaluation para PendingEvaluationSummaryDto.
     *
     * @param evaluation entidade de domínio
     * @return DTO de resumo para pendências
     */
    private PendingEvaluationSummaryDto toSummaryDto(VehicleEvaluation evaluation) {
        return new PendingEvaluationSummaryDto(
            java.util.UUID.fromString(evaluation.getId().getValueAsString()),
            evaluation.getPlate().getFormatted(),
            formatVehicleInfo(evaluation),
            evaluation.getFinalValue() != null ? evaluation.getFinalValue().getAmount() : null,
            evaluation.getCreatedAt(),
            evaluation.getEvaluatorId(), // TODO: buscar nome do avaliador via serviço
            calculateDaysPending(evaluation.getCreatedAt()),
            evaluation.getPhotos().size(),
            hasCriticalIssues(evaluation)
        );
    }

    /**
     * Formata informações do veículo.
     *
     * @param evaluation avaliação
     * @return string formatada marca/modelo/ano
     */
    private String formatVehicleInfo(VehicleEvaluation evaluation) {
        return String.format("%s %s %d",
            evaluation.getVehicleInfo().getBrand(),
            evaluation.getVehicleInfo().getModel(),
            evaluation.getVehicleInfo().getYearModel());
    }

    /**
     * Calcula dias pendente de aprovação.
     *
     * @param createdAt data de criação
     * @return dias pendentes
     */
    private Integer calculateDaysPending(LocalDateTime createdAt) {
        return (int) ChronoUnit.DAYS.between(createdAt, LocalDateTime.now());
    }

    /**
     * Verifica se avaliação possui problemas críticos.
     *
     * @param evaluation avaliação
     * @return true se possui problemas críticos
     */
    private Boolean hasCriticalIssues(VehicleEvaluation evaluation) {
        // TODO: implementar lógica baseada no checklist
        // Por enquanto, retorna false
        return false;
    }
}