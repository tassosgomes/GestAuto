using GestAuto.Commercial.Domain.Entities;

namespace GestAuto.Commercial.Application.DTOs;

/// <summary>
/// Requisição para solicitar avaliação de seminovo
/// </summary>
public record RequestEvaluationRequest(
    /// <summary>ID da proposta associada à avaliação</summary>
    Guid ProposalId,
    /// <summary>Marca do veículo</summary>
    string Brand,
    /// <summary>Modelo do veículo</summary>
    string Model,
    /// <summary>Ano de fabricação</summary>
    int Year,
    /// <summary>Quilometragem atual</summary>
    int Mileage,
    /// <summary>Placa do veículo</summary>
    string LicensePlate,
    /// <summary>Cor do veículo</summary>
    string Color,
    /// <summary>Descrição do estado geral do veículo</summary>
    string GeneralCondition,
    /// <summary>Possui histórico de manutenção na concessionária</summary>
    bool HasDealershipServiceHistory
);

/// <summary>
/// Requisição para resposta do cliente sobre avaliação
/// </summary>
public record CustomerResponseRequest(
    /// <summary>Cliente aceitou a avaliação e o valor oferecido</summary>
    bool Accepted,
    /// <summary>Motivo da recusa (se rejeitado)</summary>
    string? RejectionReason
);

/// <summary>
/// Resposta com informações completas da avaliação de seminovo
/// </summary>
public record EvaluationResponse(
    /// <summary>ID único da avaliação</summary>
    Guid Id,
    /// <summary>ID da proposta associada</summary>
    Guid ProposalId,
    /// <summary>Status da avaliação (Requested, Received, Responded, CustomerAccepted, CustomerRejected)</summary>
    string Status,
    /// <summary>Dados do veículo avaliado</summary>
    UsedVehicleResponse Vehicle,
    /// <summary>Valor avaliado do veículo</summary>
    decimal? EvaluatedValue,
    /// <summary>Observações sobre a avaliação</summary>
    string? EvaluationNotes,
    /// <summary>Data quando foi solicitada a avaliação</summary>
    DateTime RequestedAt,
    /// <summary>Data quando o módulo de seminovos respondeu</summary>
    DateTime? RespondedAt,
    /// <summary>Cliente aceitou a avaliação</summary>
    bool? CustomerAccepted,
    /// <summary>Motivo da recusa (se cliente rejeitou)</summary>
    string? CustomerRejectionReason
)
{
    public static EvaluationResponse FromEntity(UsedVehicleEvaluation evaluation) => new(
        evaluation.Id,
        evaluation.ProposalId,
        evaluation.Status.ToString(),
        UsedVehicleResponse.FromEntity(evaluation.Vehicle),
        evaluation.EvaluatedValue?.Amount,
        evaluation.EvaluationNotes,
        evaluation.RequestedAt,
        evaluation.RespondedAt,
        evaluation.CustomerAccepted,
        evaluation.CustomerRejectionReason
    );
}

/// <summary>
/// Item de avaliação em listagem paginada
/// </summary>
public record EvaluationListItemResponse(
    /// <summary>ID único da avaliação</summary>
    Guid Id,
    /// <summary>ID da proposta associada</summary>
    Guid ProposalId,
    /// <summary>Status da avaliação</summary>
    string Status,
    /// <summary>Descrição do veículo (marca, modelo, ano)</summary>
    string VehicleInfo,
    /// <summary>Valor avaliado</summary>
    decimal? EvaluatedValue,
    /// <summary>Data da solicitação</summary>
    DateTime RequestedAt
)
{
    public static EvaluationListItemResponse FromEntity(UsedVehicleEvaluation evaluation) => new(
        evaluation.Id,
        evaluation.ProposalId,
        evaluation.Status.ToString(),
        $"{evaluation.Vehicle.Brand} {evaluation.Vehicle.Model} {evaluation.Vehicle.Year}",
        evaluation.EvaluatedValue?.Amount,
        evaluation.RequestedAt
    );
}

/// <summary>
/// Informações do veículo seminovo sendo avaliado
/// </summary>
public record UsedVehicleResponse(
    /// <summary>Marca do veículo</summary>
    string Brand,
    /// <summary>Modelo do veículo</summary>
    string Model,
    /// <summary>Ano de fabricação</summary>
    int Year,
    /// <summary>Quilometragem</summary>
    int Mileage,
    /// <summary>Placa do veículo</summary>
    string LicensePlate,
    /// <summary>Cor do veículo</summary>
    string Color,
    /// <summary>Estado geral do veículo</summary>
    string GeneralCondition,
    /// <summary>Possui histórico de manutenção na concessionária</summary>
    bool HasDealershipServiceHistory
)
{
    public static UsedVehicleResponse FromEntity(UsedVehicle vehicle) => new(
        vehicle.Brand,
        vehicle.Model,
        vehicle.Year,
        vehicle.Mileage,
        vehicle.LicensePlate.Formatted,
        vehicle.Color,
        vehicle.GeneralCondition,
        vehicle.HasDealershipServiceHistory
    );
}