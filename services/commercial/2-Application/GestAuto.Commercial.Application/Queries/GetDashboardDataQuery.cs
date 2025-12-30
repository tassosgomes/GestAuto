using GestAuto.Commercial.Application.DTOs;
using GestAuto.Commercial.Application.Interfaces;

namespace GestAuto.Commercial.Application.Queries;

/// <summary>
/// Query para obter dados do dashboard
/// </summary>
public record GetDashboardDataQuery : IQuery<DashboardResponse>
{
    /// <summary>
    /// ID do vendedor (quando aplicável)
    /// </summary>
    public string? SalesPersonId { get; init; }
}
