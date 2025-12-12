package com.gestauto.vehicleevaluation.api.controller;

import com.gestauto.vehicleevaluation.application.dto.CreateEvaluationCommand;
import com.gestauto.vehicleevaluation.application.command.CreateEvaluationHandler;
import com.gestauto.vehicleevaluation.application.dto.UpdateEvaluationCommand;
import com.gestauto.vehicleevaluation.application.command.UpdateEvaluationHandler;
import com.gestauto.vehicleevaluation.application.dto.UpdateChecklistCommand;
import com.gestauto.vehicleevaluation.application.command.UpdateChecklistHandler;
import com.gestauto.vehicleevaluation.application.dto.PagedResult;
import com.gestauto.vehicleevaluation.application.dto.VehicleEvaluationDto;
import com.gestauto.vehicleevaluation.application.dto.VehicleEvaluationSummaryDto;
import com.gestauto.vehicleevaluation.application.query.GetEvaluationHandler;
import com.gestauto.vehicleevaluation.application.query.GetEvaluationQuery;
import com.gestauto.vehicleevaluation.application.query.ListEvaluationsHandler;
import com.gestauto.vehicleevaluation.application.query.ListEvaluationsQuery;
import io.swagger.v3.oas.annotations.Operation;
import io.swagger.v3.oas.annotations.Parameter;
import io.swagger.v3.oas.annotations.media.Content;
import io.swagger.v3.oas.annotations.media.Schema;
import io.swagger.v3.oas.annotations.responses.ApiResponse;
import io.swagger.v3.oas.annotations.responses.ApiResponses;
import io.swagger.v3.oas.annotations.security.SecurityRequirement;
import io.swagger.v3.oas.annotations.tags.Tag;
import jakarta.validation.Valid;
import lombok.RequiredArgsConstructor;
import lombok.extern.slf4j.Slf4j;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.security.access.prepost.PreAuthorize;
import org.springframework.web.bind.annotation.*;
import org.springframework.web.servlet.support.ServletUriComponentsBuilder;

import java.net.URI;
import java.util.UUID;

/**
 * Controller REST para gestão de avaliações de veículos.
 *
 * Este controller expõe os endpoints da API para criação, atualização,
 * consulta e listagem de avaliações de veículos seminovos.
 */
@RestController
@RequestMapping("/api/v1/evaluations")
@RequiredArgsConstructor
@Slf4j
@Tag(name = "Avaliações de Veículos", description = "Endpoints para gestão de avaliações de veículos seminovos")
@SecurityRequirement(name = "bearerAuth")
public class VehicleEvaluationController {

    private final CreateEvaluationHandler createEvaluationHandler;
    private final UpdateEvaluationHandler updateEvaluationHandler;
    private final UpdateChecklistHandler updateChecklistHandler;
    private final GetEvaluationHandler getEvaluationHandler;
    private final ListEvaluationsHandler listEvaluationsHandler;

    @Operation(
            summary = "Criar nova avaliação",
            description = "Cria uma nova avaliação para o veículo informado. O sistema buscará automaticamente os dados na tabela FIPE."
    )
    @ApiResponses(value = {
            @ApiResponse(
                    responseCode = "201",
                    description = "Avaliação criada com sucesso",
                    content = @Content(schema = @Schema(implementation = UUID.class))
            ),
            @ApiResponse(
                    responseCode = "400",
                    description = "Dados inválidos na requisição"
            ),
            @ApiResponse(
                    responseCode = "409",
                    description = "Já existe avaliação ativa para o veículo"
            ),
            @ApiResponse(
                    responseCode = "403",
                    description = "Usuário não possui permissão"
            )
    })
    @PostMapping
    @PreAuthorize("hasAnyRole('VEHICLE_EVALUATOR', 'EVALUATION_MANAGER', 'MANAGER', 'ADMIN')")
    public ResponseEntity<UUID> createEvaluation(
            @Valid @RequestBody CreateEvaluationCommand command) throws Exception {

        log.info("Recebida requisição para criar avaliação do veículo de placa: {}", command.plate());

        UUID evaluationId = createEvaluationHandler.handle(command);

        URI location = ServletUriComponentsBuilder
                .fromCurrentRequest()
                .path("/{id}")
                .buildAndExpand(evaluationId)
                .toUri();

        log.info("Avaliação criada com sucesso. ID: {}", evaluationId);

        return ResponseEntity.created(location).body(evaluationId);
    }

    @Operation(
            summary = "Atualizar avaliação",
            description = "Atualiza dados de uma avaliação existente. Apenas avaliações em status DRAFT podem ser atualizadas."
    )
    @ApiResponses(value = {
            @ApiResponse(
                    responseCode = "204",
                    description = "Avaliação atualizada com sucesso"
            ),
            @ApiResponse(
                    responseCode = "400",
                    description = "Dados inválidos na requisição"
            ),
            @ApiResponse(
                    responseCode = "404",
                    description = "Avaliação não encontrada"
            ),
            @ApiResponse(
                    responseCode = "409",
                    description = "Avaliação não pode ser editada no status atual"
            ),
            @ApiResponse(
                    responseCode = "403",
                    description = "Usuário não possui permissão"
            )
    })
    @PutMapping("/{id}")
    @PreAuthorize("hasAnyRole('VEHICLE_EVALUATOR', 'EVALUATION_MANAGER', 'MANAGER', 'ADMIN')")
    public ResponseEntity<Void> updateEvaluation(
            @Parameter(description = "ID da avaliação") @PathVariable UUID id,
            @Valid @RequestBody UpdateEvaluationCommand command) throws Exception {

        log.info("Recebida requisição para atualizar avaliação ID: {}", id);

        // Atualiza o ID no comando
        UpdateEvaluationCommand commandWithId = new UpdateEvaluationCommand(
                id,
                command.plate(),
                command.year(),
                command.mileage(),
                command.color(),
                command.version(),
                command.fuelType(),
                command.gearbox(),
                command.accessories(),
                command.internalNotes()
        );

        updateEvaluationHandler.handle(commandWithId);

        log.info("Avaliação atualizada com sucesso. ID: {}", id);

        return ResponseEntity.noContent().build();
    }

    @Operation(
            summary = "Buscar avaliação por ID",
            description = "Retorna os dados completos de uma avaliação específica."
    )
    @ApiResponses(value = {
            @ApiResponse(
                    responseCode = "200",
                    description = "Avaliação encontrada",
                    content = @Content(schema = @Schema(implementation = VehicleEvaluationDto.class))
            ),
            @ApiResponse(
                    responseCode = "404",
                    description = "Avaliação não encontrada"
            ),
            @ApiResponse(
                    responseCode = "403",
                    description = "Usuário não possui permissão"
            )
    })
    @GetMapping("/{id}")
    @PreAuthorize("hasAnyRole('VEHICLE_EVALUATOR', 'EVALUATION_MANAGER', 'MANAGER', 'ADMIN')")
    public ResponseEntity<VehicleEvaluationDto> getEvaluation(
            @Parameter(description = "ID da avaliação") @PathVariable UUID id) throws Exception {

        log.info("Recebida requisição para buscar avaliação ID: {}", id);

        GetEvaluationQuery query = new GetEvaluationQuery(id);
        VehicleEvaluationDto evaluation = getEvaluationHandler.handle(query);

        log.info("Avaliação encontrada. ID: {}, Status: {}", evaluation.id(), evaluation.status());

        return ResponseEntity.ok(evaluation);
    }

    @Operation(
            summary = "Listar avaliações",
            description = "Lista avaliações com suporte a filtros e paginação."
    )
    @ApiResponses(value = {
            @ApiResponse(
                    responseCode = "200",
                    description = "Lista de avaliações retornada com sucesso",
                    content = @Content(schema = @Schema(implementation = PagedResult.class))
            ),
            @ApiResponse(
                    responseCode = "403",
                    description = "Usuário não possui permissão"
            )
    })
    @GetMapping
    @PreAuthorize("hasAnyRole('VEHICLE_EVALUATOR', 'EVALUATION_MANAGER', 'MANAGER', 'ADMIN')")
    public ResponseEntity<PagedResult<VehicleEvaluationSummaryDto>> listEvaluations(
            @Parameter(description = "ID do avaliador") @RequestParam(name = "evaluatorId", required = false) UUID evaluatorId,
            @Parameter(description = "Status da avaliação") @RequestParam(name = "status", required = false) String status,
            @Parameter(description = "Placa do veículo (parcial)") @RequestParam(name = "plate", required = false) String plate,
            @Parameter(description = "Número da página") @RequestParam(name = "page", defaultValue = "0") int page,
            @Parameter(description = "Tamanho da página") @RequestParam(name = "size", defaultValue = "20") int size,
            @Parameter(description = "Campo de ordenação") @RequestParam(name = "sortBy", defaultValue = "createdAt") String sortBy,
            @Parameter(description = "Direção da ordenação") @RequestParam(name = "sortDirection", defaultValue = "DESC") String sortDirection) throws Exception {

        log.info("Recebida requisição para listar avaliações. Filtros: evaluatorId={}, status={}, page={}, size={}",
                evaluatorId, status, page, size);

        // Converte string status para enum se fornecido
        var statusEnum = status != null ? tryParseStatus(status) : null;

        ListEvaluationsQuery query = new ListEvaluationsQuery(
                evaluatorId,
                statusEnum,
                plate,
                page,
                size,
                sortBy,
                sortDirection
        );

        PagedResult<VehicleEvaluationSummaryDto> result = listEvaluationsHandler.handle(query);

        log.info("Listagem concluída. Retornando {} de {} avaliações",
                result.content().size(), result.totalElements());

        return ResponseEntity.ok(result);
    }

    @Operation(
            summary = "Atualizar checklist técnico",
            description = "Atualiza o checklist técnico de uma avaliação com validações por seção e cálculo automático de score."
    )
    @ApiResponses(value = {
            @ApiResponse(
                    responseCode = "204",
                    description = "Checklist atualizado com sucesso"
            ),
            @ApiResponse(
                    responseCode = "400",
                    description = "Dados inválidos na requisição"
            ),
            @ApiResponse(
                    responseCode = "404",
                    description = "Avaliação não encontrada"
            ),
            @ApiResponse(
                    responseCode = "409",
                    description = "Avaliação não pode ser editada no status atual"
            ),
            @ApiResponse(
                    responseCode = "403",
                    description = "Usuário não possui permissão"
            )
    })
    @PutMapping("/{id}/checklist")
    @PreAuthorize("hasAnyRole('VEHICLE_EVALUATOR', 'EVALUATION_MANAGER', 'MANAGER', 'ADMIN')")
    public ResponseEntity<Void> updateChecklist(
            @Parameter(description = "ID da avaliação") @PathVariable UUID id,
            @Valid @RequestBody UpdateChecklistCommand command) throws Exception {

        log.info("Recebida requisição para atualizar checklist da avaliação ID: {}", id);

        // Atualiza o ID no comando
        UpdateChecklistCommand commandWithId = new UpdateChecklistCommand(
                id,
                command.bodywork(),
                command.mechanical(),
                command.tires(),
                command.interior(),
                command.documents()
        );

        updateChecklistHandler.handle(commandWithId);

        log.info("Checklist atualizado com sucesso. ID: {}", id);

        return ResponseEntity.noContent().build();
    }

    /**
      * Tenta converter string para enum EvaluationStatus.
      *
      * @param status string do status
      * @return EvaluationStatus ou null se inválido
      */
    private com.gestauto.vehicleevaluation.domain.enums.EvaluationStatus tryParseStatus(String status) {
        try {
            return com.gestauto.vehicleevaluation.domain.enums.EvaluationStatus.valueOf(status.toUpperCase());
        } catch (IllegalArgumentException e) {
            log.warn("Status inválido fornecido: {}", status);
            return null;
        }
    }
}