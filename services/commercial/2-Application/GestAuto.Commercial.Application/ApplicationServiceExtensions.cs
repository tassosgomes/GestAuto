using Microsoft.Extensions.DependencyInjection;
using FluentValidation;

namespace GestAuto.Commercial.Application;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Domain Services
        services.AddScoped<Domain.Services.LeadScoringService>();

        // Handlers - Leads
        services.AddScoped<Interfaces.ICommandHandler<Commands.CreateLeadCommand, DTOs.LeadResponse>, Handlers.CreateLeadHandler>();
        services.AddScoped<Interfaces.ICommandHandler<Commands.QualifyLeadCommand, DTOs.LeadResponse>, Handlers.QualifyLeadHandler>();
        services.AddScoped<Interfaces.ICommandHandler<Commands.ChangeLeadStatusCommand, DTOs.LeadResponse>, Handlers.ChangeLeadStatusHandler>();
        services.AddScoped<Interfaces.ICommandHandler<Commands.RegisterInteractionCommand, DTOs.InteractionResponse>, Handlers.RegisterInteractionHandler>();
        services.AddScoped<Interfaces.ICommandHandler<Commands.UpdateLeadCommand, DTOs.LeadResponse>, Handlers.UpdateLeadHandler>();
        
        services.AddScoped<Interfaces.IQueryHandler<Queries.GetLeadQuery, DTOs.LeadResponse>, Handlers.GetLeadHandler>();
        services.AddScoped<Interfaces.IQueryHandler<Queries.ListLeadsQuery, DTOs.PagedResponse<DTOs.LeadListItemResponse>>, Handlers.ListLeadsHandler>();
        services.AddScoped<Interfaces.IQueryHandler<Queries.ListInteractionsQuery, IReadOnlyList<DTOs.InteractionResponse>>, Handlers.ListInteractionsHandler>();

        // Handlers - Dashboard
        services.AddScoped<Interfaces.IQueryHandler<Queries.GetDashboardDataQuery, DTOs.DashboardResponse>, Handlers.GetDashboardDataHandler>();

        // Handlers - Proposals
        services.AddScoped<Interfaces.ICommandHandler<Commands.CreateProposalCommand, DTOs.ProposalResponse>, Handlers.CreateProposalHandler>();
        services.AddScoped<Interfaces.ICommandHandler<Commands.UpdateProposalCommand, DTOs.ProposalResponse>, Handlers.UpdateProposalHandler>();
        services.AddScoped<Interfaces.ICommandHandler<Commands.AddProposalItemCommand, DTOs.ProposalResponse>, Handlers.AddProposalItemHandler>();
        services.AddScoped<Interfaces.ICommandHandler<Commands.RemoveProposalItemCommand, DTOs.ProposalResponse>, Handlers.RemoveProposalItemHandler>();
        services.AddScoped<Interfaces.ICommandHandler<Commands.ApplyDiscountCommand, DTOs.ProposalResponse>, Handlers.ApplyDiscountHandler>();
        services.AddScoped<Interfaces.ICommandHandler<Commands.ApproveDiscountCommand, DTOs.ProposalResponse>, Handlers.ApproveDiscountHandler>();
        services.AddScoped<Interfaces.ICommandHandler<Commands.CloseProposalCommand, DTOs.ProposalResponse>, Handlers.CloseProposalHandler>();
        
        services.AddScoped<Interfaces.IQueryHandler<Queries.GetProposalQuery, DTOs.ProposalResponse>, Handlers.GetProposalHandler>();
        services.AddScoped<Interfaces.IQueryHandler<Queries.ListProposalsQuery, DTOs.PagedResponse<DTOs.ProposalListItemResponse>>, Handlers.ListProposalsHandler>();

        // Handlers - Test Drives
        services.AddScoped<Interfaces.ICommandHandler<Commands.ScheduleTestDriveCommand, DTOs.TestDriveResponse>, Handlers.ScheduleTestDriveHandler>();
        services.AddScoped<Interfaces.ICommandHandler<Commands.CompleteTestDriveCommand, DTOs.TestDriveResponse>, Handlers.CompleteTestDriveHandler>();
        services.AddScoped<Interfaces.ICommandHandler<Commands.CancelTestDriveCommand, DTOs.TestDriveResponse>, Handlers.CancelTestDriveHandler>();
        
        services.AddScoped<Interfaces.IQueryHandler<Queries.GetTestDriveQuery, DTOs.TestDriveResponse>, Handlers.GetTestDriveHandler>();
        services.AddScoped<Interfaces.IQueryHandler<Queries.ListTestDrivesQuery, DTOs.PagedResponse<DTOs.TestDriveListItemResponse>>, Handlers.ListTestDrivesHandler>();

        // Handlers - Evaluations
        services.AddScoped<Interfaces.ICommandHandler<Commands.RequestEvaluationCommand, DTOs.EvaluationResponse>, Handlers.RequestEvaluationHandler>();
        services.AddScoped<Interfaces.ICommandHandler<Commands.RegisterCustomerResponseCommand, DTOs.EvaluationResponse>, Handlers.RegisterCustomerResponseHandler>();
        
        services.AddScoped<Interfaces.IQueryHandler<Queries.GetEvaluationQuery, DTOs.EvaluationResponse>, Handlers.GetEvaluationHandler>();
        services.AddScoped<Interfaces.IQueryHandler<Queries.ListEvaluationsQuery, DTOs.PagedResponse<DTOs.EvaluationListItemResponse>>, Handlers.ListEvaluationsHandler>();

        // Handlers - Orders
        services.AddScoped<Interfaces.ICommandHandler<Commands.AddOrderNotesCommand, DTOs.OrderResponse>, Handlers.AddOrderNotesHandler>();
        
        services.AddScoped<Interfaces.IQueryHandler<Queries.GetOrderQuery, DTOs.OrderResponse>, Handlers.GetOrderHandler>();
        services.AddScoped<Interfaces.IQueryHandler<Queries.ListOrdersQuery, DTOs.PagedResponse<DTOs.OrderListItemResponse>>, Handlers.ListOrdersHandler>();

        // Validators
        services.AddValidatorsFromAssemblyContaining<Validators.CreateLeadValidator>();

        return services;
    }
}