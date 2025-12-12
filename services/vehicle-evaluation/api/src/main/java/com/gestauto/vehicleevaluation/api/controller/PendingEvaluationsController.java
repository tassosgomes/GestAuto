package com.gestauto.vehicleevaluation.api.controller;

import com.gestauto.vehicleevaluation.application.command.ApproveEvaluationCommand;
import com.gestauto.vehicleevaluation.application.command.ApproveEvaluationHandler;
import com.gestauto.vehicleevaluation.application.command.RejectEvaluationCommand;
import com.gestauto.vehicleevaluation.application.command.RejectEvaluationHandler;
import com.gestauto.vehicleevaluation.application.dto.PagedResult;
import com.gestauto.vehicleevaluation.application.dto.PendingEvaluationSummaryDto;
import com.gestauto.vehicleevaluation.application.query.GetPendingApprovalsHandler;
import com.gestauto.vehicleevaluation.application.query.GetPendingApprovalsQuery;
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
import org.springframework.http.ResponseEntity;
import org.springframework.security.access.prepost.PreAuthorize;
import org.springframework.web.bind.annotation.*;

import java.util.UUID;

/**
 * Controller REST para gestão de avaliações pendentes de aprovação.
 *
 * Este controller expõe os endpoints para dashboard de pendências,
 * aprovação e rejeição de avaliações por gestores.
 */
@RestController
@RequestMapping("/api/v1/evaluations/pending")
@RequiredArgsConstructor
@Slf4j
@Tag(name = "Avaliações Pendentes", description = "Endpoints para gestão de aprovações de avaliações")
@SecurityRequirement(name = "bearerAuth")
@PreAuthorize("hasAnyRole('MANAGER', 'ADMIN')")
public class PendingEvaluationsController {

    private final GetPendingApprovalsHandler getPendingApprovalsHandler;
    private final ApproveEvaluationHandler approveEvaluationHandler;
    private final RejectEvaluationHandler rejectEvaluationHandler;

    @GetMapping
    @Operation(summary = "Listar avaliações pendentes de aprovação",
               description = "Retorna lista paginada de avaliações aguardando aprovação gerencial")
    @ApiResponses(value = {
        @ApiResponse(responseCode = "200", description = "Lista obtida com sucesso",
                    content = @Content(schema = @Schema(implementation = PagedResult.class))),
        @ApiResponse(responseCode = "403", description = "Acesso negado - usuário sem permissão de gestor")
    })
    public ResponseEntity<PagedResult<PendingEvaluationSummaryDto>> getPendingApprovals(
            @Parameter(description = "Parâmetros de paginação e ordenação")
            @Valid @ModelAttribute GetPendingApprovalsQuery query) {

        log.info("Recebida requisição para listar pendências: {}", query);
        PagedResult<PendingEvaluationSummaryDto> result = getPendingApprovalsHandler.handle(query);
        return ResponseEntity.ok(result);
    }

    @PostMapping("/{id}/approve")
    @Operation(summary = "Aprovar avaliação",
               description = "Aprova uma avaliação pendente, opcionalmente ajustando o valor")
    @ApiResponses(value = {
        @ApiResponse(responseCode = "200", description = "Avaliação aprovada com sucesso"),
        @ApiResponse(responseCode = "400", description = "Dados inválidos ou avaliação não pode ser aprovada"),
        @ApiResponse(responseCode = "403", description = "Acesso negado"),
        @ApiResponse(responseCode = "404", description = "Avaliação não encontrada")
    })
    public ResponseEntity<Void> approveEvaluation(
            @Parameter(description = "ID da avaliação")
            @PathVariable UUID id,
            @Parameter(description = "Dados da aprovação")
            @Valid @RequestBody ApproveEvaluationRequest request) {

        log.info("Recebida requisição para aprovar avaliação: {}", id);
        ApproveEvaluationCommand command = new ApproveEvaluationCommand(id, request.adjustedValue());
        approveEvaluationHandler.handle(command);
        return ResponseEntity.ok().build();
    }

    @PostMapping("/{id}/reject")
    @Operation(summary = "Rejeitar avaliação",
               description = "Rejeita uma avaliação pendente com justificativa obrigatória")
    @ApiResponses(value = {
        @ApiResponse(responseCode = "200", description = "Avaliação rejeitada com sucesso"),
        @ApiResponse(responseCode = "400", description = "Justificativa obrigatória ou dados inválidos"),
        @ApiResponse(responseCode = "403", description = "Acesso negado"),
        @ApiResponse(responseCode = "404", description = "Avaliação não encontrada")
    })
    public ResponseEntity<Void> rejectEvaluation(
            @Parameter(description = "ID da avaliação")
            @PathVariable UUID id,
            @Parameter(description = "Dados da rejeição")
            @Valid @RequestBody RejectEvaluationRequest request) {

        log.info("Recebida requisição para rejeitar avaliação: {}", id);
        RejectEvaluationCommand command = new RejectEvaluationCommand(id, request.reason());
        rejectEvaluationHandler.handle(command);
        return ResponseEntity.ok().build();
    }

    /**
     * Request DTO para aprovação.
     */
    public record ApproveEvaluationRequest(
        @Schema(description = "Valor ajustado (opcional)")
        java.math.BigDecimal adjustedValue
    ) {}

    /**
     * Request DTO para rejeição.
     */
    public record RejectEvaluationRequest(
        @Schema(description = "Motivo da rejeição", required = true)
        String reason
    ) {}
}