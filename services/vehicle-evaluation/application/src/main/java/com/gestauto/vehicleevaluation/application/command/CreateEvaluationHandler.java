package com.gestauto.vehicleevaluation.application.command;

import com.gestauto.vehicleevaluation.application.dto.CreateEvaluationCommand;
import com.gestauto.vehicleevaluation.application.service.DomainEventPublisherService;
import com.gestauto.vehicleevaluation.application.service.FipeService;
import com.gestauto.vehicleevaluation.domain.entity.VehicleEvaluation;
import com.gestauto.vehicleevaluation.domain.enums.EvaluationStatus;
import com.gestauto.vehicleevaluation.domain.enums.FuelType;
import com.gestauto.vehicleevaluation.domain.exception.DomainException;
import com.gestauto.vehicleevaluation.domain.repository.VehicleEvaluationRepository;
import com.gestauto.vehicleevaluation.domain.value.Money;
import com.gestauto.vehicleevaluation.domain.value.Plate;
import com.gestauto.vehicleevaluation.domain.value.VehicleInfo;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.stereotype.Component;
import org.springframework.transaction.annotation.Transactional;

import java.util.List;
import java.util.Optional;
import java.util.UUID;

/**
 * Handler responsável por criar novas avaliações de veículos.
 *
 * Este handler implementa o comando de criação de avaliação seguindo
 * o padrão CQRS e encapsulando toda a lógica de negócio necessária
 * para a criação de uma avaliação de veículo.
 */
@Component
@RequiredArgsConstructor
@Slf4j
public class CreateEvaluationHandler implements CommandHandler<CreateEvaluationCommand, UUID> {

    private final VehicleEvaluationRepository vehicleEvaluationRepository;
    private final FipeService fipeService;
    private final DomainEventPublisherService eventPublisher;

    @Override
    @Transactional
    public UUID handle(CreateEvaluationCommand command) throws Exception {
        log.info("Iniciando criação da avaliação para o veículo de placa: {}", command.plate());

        try {
            // 1. Validar placa única
            Plate plate = Plate.of(command.plate());
            validateUniquePlate(plate);

            // 2. Obter informações do veículo via FIPE
            VehicleInfo vehicleInfo = getVehicleInfo(command);

            // 3. Criar entidade VehicleEvaluation
            VehicleEvaluation evaluation = createEvaluation(command, plate, vehicleInfo);

            // 4. Salvar avaliação
            VehicleEvaluation savedEvaluation = vehicleEvaluationRepository.save(evaluation);

            // 5. Publicar eventos de domínio
            publishDomainEvents(savedEvaluation);

            log.info("Avaliação criada com sucesso. ID: {}", savedEvaluation.getId().getValueAsString());

            return UUID.fromString(savedEvaluation.getId().getValueAsString());

        } catch (Exception e) {
            log.error("Erro ao criar avaliação para o veículo de placa: {}", command.plate(), e);
            throw new DomainException("Falha ao criar avaliação: " + e.getMessage(), e);
        }
    }

    /**
     * Valida que não existe avaliação ativa para a placa informada.
     *
     * @param plate placa a ser validada
     * @throws DomainException se já existir avaliação ativa
     */
    private void validateUniquePlate(Plate plate) {
        boolean hasActiveEvaluation = vehicleEvaluationRepository.existsByPlateAndStatus(
                plate, EvaluationStatus.DRAFT) ||
                vehicleEvaluationRepository.existsByPlateAndStatus(plate, EvaluationStatus.PENDING_APPROVAL) ||
                vehicleEvaluationRepository.existsByPlateAndStatus(plate, EvaluationStatus.APPROVED);

        if (hasActiveEvaluation) {
            throw new DomainException("Já existe uma avaliação ativa para o veículo de placa: " + plate.getFormatted());
        }
    }

    /**
     * Obtém informações do veículo através do serviço FIPE.
     *
     * @param command comando de criação com dados do veículo
     * @return VehicleInfo com dados completos do veículo
     * @throws DomainException se não for possível obter informações
     */
    private VehicleInfo getVehicleInfo(CreateEvaluationCommand command) {
        // Tenta buscar informações via placa primeiro
        Optional<VehicleInfo> vehicleInfoOpt = fipeService.getVehicleInfoByPlate(command.plate());

        if (vehicleInfoOpt.isPresent()) {
            return vehicleInfoOpt.get();
        }

        // Se não encontrar via placa, cria VehicleInfo com dados do comando
        FuelType fuelType = parseFuelType(command.fuelType());
        String version = command.version() != null && !command.version().trim().isEmpty()
                         ? command.version()
                         : "Standard";
        String color = command.color() != null && !command.color().trim().isEmpty()
                       ? command.color()
                       : "Não informada";

        return VehicleInfo.of(
                "Não informada", // brand (será preenchida depois)
                "Não informado", // model (será preenchido depois)
                version,
                command.year(),
                command.year(),
                color,
                fuelType
        );
    }

    /**
     * Converte string do tipo de combustível para enum FuelType.
     *
     * @param fuelTypeString string do tipo de combustível
     * @return FuelType correspondente
     */
    private FuelType parseFuelType(String fuelTypeString) {
        if (fuelTypeString == null || fuelTypeString.trim().isEmpty()) {
            return FuelType.FLEX; // Default
        }

        try {
            return FuelType.valueOf(fuelTypeString.toUpperCase());
        } catch (IllegalArgumentException e) {
            // Tenta mapeamento por descrição ou usa default
            String normalized = fuelTypeString.toLowerCase();
            if (normalized.contains("flex")) return FuelType.FLEX;
            if (normalized.contains("gasolina")) return FuelType.GASOLINE;
            if (normalized.contains("etanol")) return FuelType.ETHANOL;
            if (normalized.contains("diesel")) return FuelType.DIESEL;
            if (normalized.contains("elétrico")) return FuelType.ELECTRIC;
            if (normalized.contains("híbrido")) return FuelType.HYBRID;
            if (normalized.contains("gnv") || normalized.contains("gás")) return FuelType.NATURAL_GAS;

            return FuelType.FLEX; // Default
        }
    }

    /**
     * Cria a entidade VehicleEvaluation com dados do comando.
     *
     * @param command comando de criação
     * @param plate objeto Plate validado
     * @param vehicleInfo informações do veículo
     * @return nova instância de VehicleEvaluation
     */
    private VehicleEvaluation createEvaluation(CreateEvaluationCommand command,
                                             Plate plate,
                                             VehicleInfo vehicleInfo) {
        // Converte quilometragem para Money
        Money mileage = Money.of(command.mileage());

        // Mock do ID do avaliador - em produção viria do contexto de segurança
        String evaluatorId = "evaluator-" + UUID.randomUUID().toString().substring(0, 8);

        // Mock do RENAVAM - em produção seria validado
        String renavam = generateMockRenavam();

        // Cria avaliação
        VehicleEvaluation evaluation = VehicleEvaluation.create(
                plate,
                renavam,
                vehicleInfo,
                mileage,
                evaluatorId
        );

        // Adiciona observações internas se fornecidas
        if (command.internalNotes() != null && !command.internalNotes().trim().isEmpty()) {
            evaluation.setObservations(command.internalNotes());
        }

        return evaluation;
    }

    /**
     * Gera um RENAVAM mock para testes.
     *
     * @return RENAVAM formatado
     */
    private String generateMockRenavam() {
        // Gera RENAVAM aleatório de 11 dígitos
        return String.format("%011d", (long) (Math.random() * 10000000000L));
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