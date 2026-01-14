using GestAuto.Stock.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace GestAuto.Stock.API.Middleware;

public class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlerMiddleware> _logger;

    public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, detail) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Recurso não encontrado", exception.Message),
            UnauthorizedException => (StatusCodes.Status401Unauthorized, "Não autorizado", exception.Message),
            ForbiddenException => (StatusCodes.Status403Forbidden, "Acesso negado", exception.Message),
            ConflictException => (StatusCodes.Status409Conflict, "Conflito", exception.Message),
            DomainException => (StatusCodes.Status422UnprocessableEntity, "Erro de negócio", exception.Message),
            _ => (StatusCodes.Status500InternalServerError, "Erro interno", "Ocorreu um erro inesperado")
        };

        _logger.LogError(exception, "Erro ao processar requisição: {Message}", exception.Message);

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        await context.Response.WriteAsJsonAsync(problemDetails);
    }
}
