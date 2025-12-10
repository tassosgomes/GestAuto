using GestAuto.Commercial.Domain.ValueObjects;

namespace GestAuto.Commercial.Application.DTOs;

/// <summary>
/// Requisição para criar uma nova proposta comercial
/// </summary>
public record CreateProposalRequest(
    /// <summary>ID do lead para o qual criar a proposta</summary>
    Guid LeadId,
    /// <summary>Modelo do veículo</summary>
    string VehicleModel,
    /// <summary>Versão/trim do veículo</summary>
    string VehicleTrim,
    /// <summary>Cor do veículo</summary>
    string VehicleColor,
    /// <summary>Ano do veículo</summary>
    int VehicleYear,
    /// <summary>Indica se o veículo está pronto para entrega</summary>
    bool IsReadyDelivery,
    /// <summary>Preço base do veículo</summary>
    decimal VehiclePrice,
    /// <summary>Forma de pagamento (Cash, Financing)</summary>
    string PaymentMethod,
    /// <summary>Valor de entrada (se parcelado)</summary>
    decimal? DownPayment,
    /// <summary>Número de parcelas (se parcelado)</summary>
    int? Installments
);

/// <summary>
/// Requisição para atualizar uma proposta existente
/// </summary>
public record UpdateProposalRequest(
    /// <summary>Modelo do veículo</summary>
    string? VehicleModel,
    /// <summary>Versão/trim do veículo</summary>
    string? VehicleTrim,
    /// <summary>Cor do veículo</summary>
    string? VehicleColor,
    /// <summary>Ano do veículo</summary>
    int? VehicleYear,
    /// <summary>Pronto para entrega</summary>
    bool? IsReadyDelivery,
    /// <summary>Preço do veículo</summary>
    decimal? VehiclePrice,
    /// <summary>Forma de pagamento</summary>
    string? PaymentMethod,
    /// <summary>Valor de entrada</summary>
    decimal? DownPayment,
    /// <summary>Número de parcelas</summary>
    int? Installments
);

/// <summary>
/// Requisição para aplicar desconto em uma proposta
/// </summary>
public record ApplyDiscountRequest(
    /// <summary>Valor do desconto</summary>
    decimal Amount,
    /// <summary>Motivo do desconto</summary>
    string Reason
);

/// <summary>
/// Requisição para aprovar desconto em proposta
/// </summary>
public record ApproveDiscountRequest(
    /// <summary>ID do gerente aprovador</summary>
    Guid ManagerId
);

/// <summary>
/// Requisição para fechar uma proposta com venda
/// </summary>
public record CloseProposalRequest(
    /// <summary>ID do vendedor que fez a venda</summary>
    Guid SalesPersonId
);

/// <summary>
/// Requisição para adicionar item adicional à proposta
/// </summary>
public record AddProposalItemRequest(
    /// <summary>Descrição do item (acessórios, proteção, etc)</summary>
    string Description,
    /// <summary>Valor do item</summary>
    decimal Value
);

/// <summary>
/// Resposta com informações de um item da proposta
/// </summary>
public record ProposalItemResponse(
    /// <summary>ID único do item</summary>
    Guid Id,
    /// <summary>Descrição do item</summary>
    string Description,
    /// <summary>Valor do item</summary>
    decimal Value
)
{
    public static ProposalItemResponse FromEntity(Domain.Entities.ProposalItem item) => new(
        item.Id,
        item.Description,
        item.Price.Amount
    );
}

/// <summary>
/// Resposta com informações completas da proposta
/// </summary>
public record ProposalResponse(
    /// <summary>ID único da proposta</summary>
    Guid Id,
    /// <summary>ID do lead associado</summary>
    Guid LeadId,
    /// <summary>Status da proposta (Draft, Sent, Accepted, Rejected, Closed)</summary>
    string Status,
    /// <summary>Modelo do veículo</summary>
    string VehicleModel,
    /// <summary>Versão/trim do veículo</summary>
    string VehicleTrim,
    /// <summary>Cor do veículo</summary>
    string VehicleColor,
    /// <summary>Ano do veículo</summary>
    int VehicleYear,
    /// <summary>Pronto para entrega imediata</summary>
    bool IsReadyDelivery,
    /// <summary>Preço base do veículo</summary>
    decimal VehiclePrice,
    /// <summary>Valor do desconto aplicado</summary>
    decimal DiscountAmount,
    /// <summary>Motivo do desconto</summary>
    string? DiscountReason,
    /// <summary>Desconto foi aprovado pelo gerente</summary>
    bool DiscountApproved,
    /// <summary>ID do gerente que aprovou o desconto</summary>
    Guid? DiscountApproverId,
    /// <summary>Valor avaliado do veículo para troca</summary>
    decimal TradeInValue,
    /// <summary>Forma de pagamento (Cash, Financing)</summary>
    string PaymentMethod,
    /// <summary>Valor de entrada</summary>
    decimal? DownPayment,
    /// <summary>Número de parcelas</summary>
    int? Installments,
    /// <summary>Itens adicionais da proposta</summary>
    List<ProposalItemResponse> Items,
    /// <summary>ID da avaliação de seminovo (se houver troca)</summary>
    Guid? UsedVehicleEvaluationId,
    /// <summary>Valor total da proposta (veículo + itens - desconto - troca)</summary>
    decimal TotalValue,
    /// <summary>Data de criação da proposta</summary>
    DateTime CreatedAt,
    /// <summary>Data da última atualização</summary>
    DateTime UpdatedAt
)
{
    public static ProposalResponse FromEntity(Domain.Entities.Proposal proposal) => new(
        proposal.Id,
        proposal.LeadId,
        proposal.Status.ToString(),
        proposal.VehicleModel,
        proposal.VehicleTrim,
        proposal.VehicleColor,
        proposal.VehicleYear,
        proposal.IsReadyDelivery,
        proposal.VehiclePrice.Amount,
        proposal.DiscountAmount.Amount,
        proposal.DiscountReason,
        proposal.DiscountApproverId.HasValue,
        proposal.DiscountApproverId,
        proposal.TradeInValue.Amount,
        proposal.PaymentMethod.ToString(),
        proposal.DownPayment?.Amount,
        proposal.Installments,
        proposal.Items.Select(ProposalItemResponse.FromEntity).ToList(),
        proposal.UsedVehicleEvaluationId,
        proposal.TotalValue.Amount,
        proposal.CreatedAt,
        proposal.UpdatedAt
    );
}

/// <summary>
/// Item de proposta em listagem paginada
/// </summary>
public record ProposalListItemResponse(
    /// <summary>ID único da proposta</summary>
    Guid Id,
    /// <summary>ID do lead associado</summary>
    Guid LeadId,
    /// <summary>Status da proposta</summary>
    string Status,
    /// <summary>Modelo do veículo</summary>
    string VehicleModel,
    /// <summary>Valor total da proposta</summary>
    decimal TotalValue,
    /// <summary>Data de criação</summary>
    DateTime CreatedAt
)
{
    public static ProposalListItemResponse FromEntity(Domain.Entities.Proposal proposal) => new(
        proposal.Id,
        proposal.LeadId,
        proposal.Status.ToString(),
        proposal.VehicleModel,
        proposal.TotalValue.Amount,
        proposal.CreatedAt
    );
}
