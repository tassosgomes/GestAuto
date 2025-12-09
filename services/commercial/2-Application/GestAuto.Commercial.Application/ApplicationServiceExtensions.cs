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

        // Validators
        services.AddValidatorsFromAssemblyContaining<Validators.CreateLeadValidator>();

        return services;
    }
}