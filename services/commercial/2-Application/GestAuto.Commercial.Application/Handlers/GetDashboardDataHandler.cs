using GestAuto.Commercial.Application.DTOs;
using GestAuto.Commercial.Application.Interfaces;
using GestAuto.Commercial.Application.Queries;
using GestAuto.Commercial.Domain.Enums;
using GestAuto.Commercial.Domain.Interfaces;

namespace GestAuto.Commercial.Application.Handlers;

/// <summary>
/// Handler para query do dashboard
/// </summary>
public class GetDashboardDataHandler : IQueryHandler<GetDashboardDataQuery, DashboardResponse>
{
    private readonly ILeadRepository _leadRepository;
    private readonly IProposalRepository _proposalRepository;
    private readonly ITestDriveRepository _testDriveRepository;

    public GetDashboardDataHandler(
        ILeadRepository leadRepository,
        IProposalRepository proposalRepository,
        ITestDriveRepository testDriveRepository)
    {
        _leadRepository = leadRepository;
        _proposalRepository = proposalRepository;
        _testDriveRepository = testDriveRepository;
    }

    public async Task<DashboardResponse> HandleAsync(
        GetDashboardDataQuery query,
        CancellationToken cancellationToken)
    {
        var salesPersonId = query.SalesPersonId;

        // KPIs
        var newLeadsCount = await _leadRepository.CountByStatusAsync(
            LeadStatus.New,
            salesPersonId,
            cancellationToken);

        var openProposalsCount = await _proposalRepository.CountByStatusAsync(
            ProposalStatus.InNegotiation,
            salesPersonId,
            cancellationToken);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var testDrivesTodayCount = await _testDriveRepository.CountByDateAsync(
            today,
            salesPersonId,
            cancellationToken);

        var conversionRate = await CalculateConversionRateAsync(
            salesPersonId,
            cancellationToken);

        // Hot Leads - Top 5 Diamante/Ouro sem interação nas últimas 24h
        var oneDayAgo = DateTime.UtcNow.AddDays(-1);
        var hotLeads = await _leadRepository.GetHotLeadsAsync(
            salesPersonId,
            oneDayAgo,
            limit: 5,
            cancellationToken);

        // Propostas pendentes
        var pendingProposals = await _proposalRepository.GetPendingActionProposalsAsync(
            salesPersonId,
            limit: 5,
            cancellationToken);

        return new DashboardResponse
        {
            Kpis = new DashboardKPIsResponse
            {
                NewLeads = newLeadsCount,
                OpenProposals = openProposalsCount,
                TestDrivesToday = testDrivesTodayCount,
                ConversionRate = conversionRate
            },
            HotLeads = hotLeads.Select(LeadListItemResponse.FromEntity).ToList(),
            PendingActions = pendingProposals.Select(ProposalListItemResponse.FromEntity).ToList()
        };
    }

    private async Task<decimal> CalculateConversionRateAsync(
        string? salesPersonId,
        CancellationToken cancellationToken)
    {
        var nowUtc = DateTime.UtcNow;
        var firstDayOfMonth = new DateTime(nowUtc.Year, nowUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var totalLeads = await _leadRepository.CountCreatedSinceAsync(
            firstDayOfMonth,
            salesPersonId,
            cancellationToken);

        if (totalLeads == 0) return 0;

        var convertedLeads = await _leadRepository.CountByStatusSinceAsync(
            LeadStatus.Converted,
            firstDayOfMonth,
            salesPersonId,
            cancellationToken);

        return Math.Round((decimal)convertedLeads / totalLeads * 100, 1);
    }
}
