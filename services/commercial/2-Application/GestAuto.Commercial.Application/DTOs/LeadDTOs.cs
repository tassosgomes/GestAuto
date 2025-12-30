using GestAuto.Commercial.Domain.ValueObjects;

namespace GestAuto.Commercial.Application.DTOs;

/// <summary>
/// Requisição para criar um novo lead
/// </summary>
public record CreateLeadRequest(
    /// <summary>Nome completo do cliente</summary>
    string Name,
    /// <summary>Email do cliente</summary>
    string Email,
    /// <summary>Telefone do cliente no formato (XX) XXXXX-XXXX</summary>
    string Phone,
    /// <summary>Origem do lead (Instagram, Referral, Google, Store, Phone, Showroom, ClassifiedsPortal, Other)</summary>
    string Source,
    /// <summary>Modelo de veículo interessado (opcional)</summary>
    string? InterestedModel,
    /// <summary>Versão/trim do veículo interessado (opcional)</summary>
    string? InterestedTrim,
    /// <summary>Cor preferida do veículo (opcional)</summary>
    string? InterestedColor
);

/// <summary>
/// Requisição para atualizar informações do lead
/// </summary>
public record UpdateLeadRequest(
    /// <summary>Nome completo do cliente</summary>
    string Name,
    /// <summary>Email do cliente</summary>
    string Email,
    /// <summary>Telefone do cliente</summary>
    string Phone,
    /// <summary>Modelo de veículo interessado</summary>
    string? InterestedModel,
    /// <summary>Versão/trim do veículo interessado</summary>
    string? InterestedTrim,
    /// <summary>Cor preferida do veículo</summary>
    string? InterestedColor
);

/// <summary>
/// Requisição para alterar o status do lead
/// </summary>
public record ChangeLeadStatusRequest(
    /// <summary>Novo status (New, InNegotiation, Qualified, NotQualified, Converted, Lost)</summary>
    string Status
);

/// <summary>
/// Requisição para qualificar um lead
/// </summary>
public record QualifyLeadRequest(
    /// <summary>Indica se o cliente possui veículo para troca</summary>
    bool HasTradeInVehicle,
    /// <summary>Dados do veículo para troca (se existir)</summary>
    Commands.TradeInVehicleDto? TradeInVehicle,
    /// <summary>Forma de pagamento pretendida (Cash, Financing)</summary>
    string PaymentMethod,
    /// <summary>Data esperada para a compra</summary>
    DateTime? ExpectedPurchaseDate,
    /// <summary>Interessado em test-drive</summary>
    bool InterestedInTestDrive
);

/// <summary>
/// Requisição para registrar uma interação com o lead
/// </summary>
public record RegisterInteractionRequest(
    /// <summary>Tipo de interação (Call, Email, Visit, Message, Other)</summary>
    string Type,
    /// <summary>Descrição da interação</summary>
    string Description
);

/// <summary>
/// Resposta com informações completas do lead
/// </summary>
public record LeadResponse(
    /// <summary>Identificador único do lead</summary>
    Guid Id,
    /// <summary>Nome do cliente</summary>
    string Name,
    /// <summary>Email do cliente</summary>
    string Email,
    /// <summary>Telefone do cliente</summary>
    string Phone,
    /// <summary>Origem do lead</summary>
    string Source,
    /// <summary>Status atual do lead (New, InNegotiation, Qualified, NotQualified, Converted, Lost)</summary>
    string Status,
    /// <summary>Pontuação do lead (Bronze, Silver, Gold, Diamond)</summary>
    string Score,
    /// <summary>ID do vendedor responsável</summary>
    Guid SalesPersonId,
    /// <summary>Modelo de veículo interessado</summary>
    string? InterestedModel,
    /// <summary>Versão/trim do veículo interessado</summary>
    string? InterestedTrim,
    /// <summary>Cor preferida do veículo</summary>
    string? InterestedColor,
    /// <summary>Dados de qualificação do lead (se qualificado)</summary>
    QualificationResponse? Qualification,
    /// <summary>Lista de interações com o lead</summary>
    List<InteractionResponse>? Interactions,
    /// <summary>Data de criação do lead</summary>
    DateTime CreatedAt,
    /// <summary>Data da última atualização</summary>
    DateTime UpdatedAt
)
{
    public static LeadResponse FromEntity(Domain.Entities.Lead lead) => new(
        lead.Id,
        lead.Name,
        lead.Email.Value,
        lead.Phone.Formatted,
        lead.Source.ToString(),
        lead.Status.ToString(),
        lead.Score.ToString(),
        lead.SalesPersonId,
        lead.InterestedModel,
        lead.InterestedTrim,
        lead.InterestedColor,
        lead.Qualification != null ? QualificationResponse.FromEntity(lead.Qualification) : null,
        lead.Interactions?.Select(InteractionResponse.FromEntity).ToList(),
        lead.CreatedAt,
        lead.UpdatedAt
    );
}

/// <summary>
/// Informações de qualificação do lead
/// </summary>
public record QualificationResponse(
    /// <summary>Possui veículo para troca</summary>
    bool HasTradeInVehicle,
    /// <summary>Dados do veículo para troca</summary>
    TradeInVehicleResponse? TradeInVehicle,
    /// <summary>Forma de pagamento (Cash, Financing)</summary>
    string PaymentMethod,
    /// <summary>Data esperada para compra</summary>
    DateTime? ExpectedPurchaseDate,
    /// <summary>Interessado em test-drive</summary>
    bool InterestedInTestDrive
)
{
    public static QualificationResponse FromEntity(Domain.ValueObjects.Qualification qualification) => new(
        qualification.HasTradeInVehicle,
        qualification.TradeInVehicle != null 
            ? TradeInVehicleResponse.FromEntity(qualification.TradeInVehicle) 
            : null,
        qualification.PaymentMethod.ToString(),
        qualification.ExpectedPurchaseDate,
        qualification.InterestedInTestDrive
    );
}

/// <summary>
/// Informações do veículo para troca
/// </summary>
public record TradeInVehicleResponse(
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
)
{
    public static TradeInVehicleResponse FromEntity(Domain.ValueObjects.TradeInVehicle vehicle) => new(
        vehicle.Brand,
        vehicle.Model,
        vehicle.Year,
        vehicle.Mileage,
        vehicle.LicensePlate,
        vehicle.Color,
        vehicle.GeneralCondition,
        vehicle.HasDealershipServiceHistory
    );
}

/// <summary>
/// Item de lead em listagem paginada
/// </summary>
public record LeadListItemResponse(
    /// <summary>Identificador único do lead</summary>
    Guid Id,
    /// <summary>Nome do cliente</summary>
    string Name,
    /// <summary>Telefone do cliente</summary>
    string Phone,
    /// <summary>Origem do lead</summary>
    string Source,
    /// <summary>Status do lead</summary>
    string Status,
    /// <summary>Pontuação do lead</summary>
    string Score,
    /// <summary>Modelo de veículo interessado</summary>
    string? InterestedModel,
    /// <summary>Data de criação</summary>
    DateTime CreatedAt,
    /// <summary>ID do vendedor responsável</summary>
    Guid SalesPersonId
)
{
    public static LeadListItemResponse FromEntity(Domain.Entities.Lead lead) => new(
        lead.Id,
        lead.Name,
        lead.Phone.Formatted,
        lead.Source.ToString(),
        lead.Status.ToString(),
        lead.Score.ToString(),
        lead.InterestedModel,
        lead.CreatedAt,
        lead.SalesPersonId
    );
}

/// <summary>
/// Resposta paginada genérica
/// </summary>
/// <typeparam name="T">Tipo do item na página</typeparam>
public record PagedResponse<T>(
    /// <summary>Lista de itens da página atual</summary>
    IReadOnlyList<T> Items,
    /// <summary>Número da página atual (começando em 1)</summary>
    int Page,
    /// <summary>Quantidade de itens por página</summary>
    int PageSize,
    /// <summary>Total de itens no resultado completo</summary>
    int TotalCount
)
{
    /// <summary>Total de páginas disponíveis</summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    /// <summary>Indica se existe próxima página</summary>
    public bool HasNextPage => Page < TotalPages;
    /// <summary>Indica se existe página anterior</summary>
    public bool HasPreviousPage => Page > 1;
}