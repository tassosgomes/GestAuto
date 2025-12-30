namespace GestAuto.Commercial.Application.DTOs;

/// <summary>
/// KPIs do dashboard comercial
/// </summary>
public record DashboardKPIsResponse
{
    /// <summary>
    /// Contagem de leads novos atribuídos ao usuário
    /// </summary>
    public int NewLeads { get; init; }

    /// <summary>
    /// Contagem de propostas em negociação
    /// </summary>
    public int OpenProposals { get; init; }

    /// <summary>
    /// Agendamentos de test-drive para hoje
    /// </summary>
    public int TestDrivesToday { get; init; }

    /// <summary>
    /// Taxa de conversão mensal (%)
    /// </summary>
    public decimal ConversionRate { get; init; }
}

/// <summary>
/// Dados completos do dashboard
/// </summary>
public record DashboardResponse
{
    /// <summary>
    /// KPIs do dashboard
    /// </summary>
    public DashboardKPIsResponse Kpis { get; init; } = new();

    /// <summary>
    /// Top 5 leads quentes (Diamante/Ouro sem interação nas últimas 24h)
    /// </summary>
    public IReadOnlyList<LeadListItemResponse> HotLeads { get; init; } = [];

    /// <summary>
    /// Propostas pendentes de ação
    /// </summary>
    public IReadOnlyList<ProposalListItemResponse> PendingActions { get; init; } = [];
}
