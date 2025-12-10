package com.gestauto.vehicleevaluation.application.command;

import com.gestauto.vehicleevaluation.application.dto.UpdateEvaluationCommand;
import com.gestauto.vehicleevaluation.application.service.DomainEventPublisherService;
import com.gestauto.vehicleevaluation.application.service.FipeService;
import com.gestauto.vehicleevaluation.domain.entity.VehicleEvaluation;
import com.gestauto.vehicleevaluation.domain.enums.EvaluationStatus;
import com.gestauto.vehicleevaluation.domain.enums.FuelType;
import com.gestauto.vehicleevaluation.domain.exception.DomainException;
import com.gestauto.vehicleevaluation.domain.exception.InvalidEvaluationStatusException;
import com.gestauto.vehicleevaluation.domain.repository.VehicleEvaluationRepository;
import com.gestauto.vehicleevaluation.domain.value.EvaluationId;
import com.gestauto.vehicleevaluation.domain.value.Money;
import com.gestauto.vehicleevaluation.domain.value.Plate;
import com.gestauto.vehicleevaluation.domain.value.VehicleInfo;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.stereotype.Component;
import org.springframework.transaction.annotation.Transactional;

import java.util.List;
import java.util.Optional;

/**
 * Handler responsável por atualizar avaliações de veículos existentes.
 *
 * Este handler implementa o comando de atualização de avaliação seguindo
 * o padrão CQRS e permite atualizar dados básicos da avaliação
 * enquanto ela ainda estiver em status DRAFT.
 */
@Component
@RequiredArgsConstructor
@Slf4j
public class UpdateEvaluationHandler implements CommandHandler<UpdateEvaluationCommand, Void> {

    private final VehicleEvaluationRepository vehicleEvaluationRepository;
    private final FipeService fipeService;
    private final DomainEventPublisherService eventPublisher;

    @Override
    @Transactional
    public Void handle(UpdateEvaluationCommand command) throws Exception {
        log.info("Iniciando atualização da avaliação ID: {}", command.id());

        try {
            // 1. Buscar avaliação existente
            EvaluationId evaluationId = EvaluationId.from(command.id().toString());
            VehicleEvaluation evaluation = findEvaluationById(evaluationId);

            // 2. Validar se pode ser editada
            validateEditable(evaluation);

            // 3. Atualizar dados se fornecidos
            updateEvaluationData(evaluation, command);

            // 4. Salvar alterações
            VehicleEvaluation updatedEvaluation = vehicleEvaluationRepository.update(evaluation);

            // 5. Publicar eventos de domínio
            publishDomainEvents(updatedEvaluation);

            log.info("Avaliação atualizada com sucesso. ID: {}", evaluationId.getValueAsString());
            return null;

        } catch (Exception e) {
            log.error("Erro ao atualizar avaliação ID: {}", command.id(), e);
            throw new DomainException("Falha ao atualizar avaliação: " + e.getMessage(), e);
        }
    }

    /**
     * Busca avaliação por ID.
     *
     * @param evaluationId ID da avaliação
     * @return VehicleEvaluation encontrada
     * @throws DomainException se não encontrar
     */
    private VehicleEvaluation findEvaluationById(EvaluationId evaluationId) {
        Optional<VehicleEvaluation> evaluationOpt = vehicleEvaluationRepository.findById(evaluationId);

        if (evaluationOpt.isEmpty()) {
            throw new DomainException("Avaliação não encontrada: " + evaluationId.getValueAsString());
        }

        return evaluationOpt.get();
    }

    /**
     * Valida se a avaliação pode ser editada.
     *
     * @param evaluation avaliação a ser validada
     * @throws InvalidEvaluationStatusException se não puder ser editada
     */
    private void validateEditable(VehicleEvaluation evaluation) {
        if (!evaluation.getStatus().isEditable()) {
            throw new InvalidEvaluationStatusException(
                    evaluation.getStatus(),
                    "atualizar dados básicos"
            );
        }
    }

    /**
     * Atualiza os dados da avaliação com base no comando.
     *
     * @param evaluation avaliação a ser atualizada
     * @param command comando com dados de atualização
     */
    private void updateEvaluationData(VehicleEvaluation evaluation, UpdateEvaluationCommand command) {
        boolean hasChanges = false;

        // Atualiza observações se fornecidas
        if (command.internalNotes() != null) {
            String currentNotes = evaluation.getObservations();
            String newNotes = command.internalNotes().trim();

            if (!newNotes.equals(currentNotes)) {
                evaluation.setObservations(newNotes);
                hasChanges = true;
                log.debug("Observações atualizadas para a avaliação {}", evaluation.getId().getValueAsString());
            }
        }

        // Nota: Outros campos como placa, ano, etc. poderiam ser atualizados aqui
        // se o domínio suportasse essas operações. Por ora, apenas observações
        // são permitidas para simplificar o exemplo.

        if (!hasChanges) {
            log.debug("Nenhuma alteração detectada na avaliação {}", evaluation.getId().getValueAsString());
        }
    }

    /**
     * Publica os eventos de domínio gerados pela avaliação.
     *
     * @param evaluation avaliação com eventos
     */
    private void publishDomainEvents(VehicleEvaluation evaluation) {
        List<com.gestauto.vehicleevaluation.domain.event.DomainEvent> events = evaluation.getDomainEvents();

        if (!events.isEmpty()) {
            eventPublisher.publishBatch(events);
            log.info("Publicados {} eventos de domínio para a avaliação {}", events.size(), evaluation.getId().getValueAsString());
        }
    }
}