using System.Security.Claims;
using System.Text.Json;
using GestAuto.Stock.API;
using GestAuto.Stock.API.Extensions;
using GestAuto.Stock.API.Services;
using GestAuto.Stock.API.Middleware;
using GestAuto.Stock.Application;
using GestAuto.Stock.Infra;
using GestAuto.Stock.Infra.Messaging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, loggerConfiguration) =>
{
    var serviceName = context.Configuration["OTEL_SERVICE_NAME"] ?? "stock";
    var serviceVersion = context.Configuration["OTEL_SERVICE_VERSION"] ?? "1.0.0";

    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.WithProperty("service.name", serviceName)
        .Enrich.WithProperty("service.version", serviceVersion);
});

builder.Logging.AddObservabilityLogging(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers(options =>
{
    options.Conventions.Add(new ApiV1RoutePrefixConvention());
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod());
});

// Database (PostgreSQL)
builder.Services.AddDbContext<StockDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("StockDatabase")));

builder.Services.AddObservability(builder.Configuration);

// Layer registrations
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices();

// Time provider (facilitates deterministic tests for background jobs)
builder.Services.AddSingleton(TimeProvider.System);

// Services used by background jobs and callable in tests
builder.Services.AddScoped<ReservationExpirationRunner>();

// RabbitMQ (lazy connection) + outbox processor
// Avoid starting background publisher in automated tests to keep test container lifecycles isolated.
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddRabbitMq(builder.Configuration);
    builder.Services.AddHostedService<OutboxProcessorService>();
    builder.Services.AddHostedService<ReservationExpirationService>();
}

// Health checks (includes DB connectivity)
builder.Services.AddHealthChecks()
    .AddCheck<StockDbHealthCheck>("db");

// Authentication - Keycloak JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Keycloak:Authority"];
        options.Audience = builder.Configuration["Keycloak:Audience"];
        options.MapInboundClaims = false;
        options.RequireHttpsMetadata = false; // Facilitates local development

        if (builder.Environment.IsDevelopment())
        {
            options.BackchannelHttpHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            };
        }

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                if (context.Principal?.Identity is not ClaimsIdentity identity)
                {
                    return Task.CompletedTask;
                }

                // Normalize roles to multiple `roles` claims so policies can match reliably.
                var rolesClaims = identity.FindAll("roles").ToList();

                // If some pipeline mapped roles into ClaimTypes.Role, clone them back into `roles`.
                if (rolesClaims.Count == 0)
                {
                    var mappedRoleClaims = identity.FindAll(ClaimTypes.Role).ToList();
                    if (mappedRoleClaims.Count > 0)
                    {
                        foreach (var c in mappedRoleClaims)
                        {
                            identity.AddClaim(new Claim("roles", c.Value));
                        }
                        rolesClaims = identity.FindAll("roles").ToList();
                    }
                }

                // Keycloak can emit roles as a single JSON array claim.
                if (rolesClaims.Count == 1)
                {
                    var raw = rolesClaims[0].Value?.Trim();
                    if (!string.IsNullOrWhiteSpace(raw) && raw.StartsWith("[") && raw.EndsWith("]"))
                    {
                        try
                        {
                            var roles = JsonSerializer.Deserialize<string[]>(raw);
                            if (roles is { Length: > 0 })
                            {
                                identity.RemoveClaim(rolesClaims[0]);
                                foreach (var role in roles)
                                {
                                    if (!string.IsNullOrWhiteSpace(role))
                                    {
                                        identity.AddClaim(new Claim("roles", role));
                                    }
                                }
                            }
                        }
                        catch
                        {
                            // If parsing fails, keep the original claim.
                        }
                    }
                }

                return Task.CompletedTask;
            },
            OnChallenge = async context =>
            {
                // Ensure 401 replies are ProblemDetails too.
                context.HandleResponse();

                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/problem+json";

                var problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "Não autorizado",
                    Detail = "Token não fornecido ou inválido",
                    Instance = context.Request.Path
                };

                await context.Response.WriteAsJsonAsync(problemDetails);
            },
            OnForbidden = async context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/problem+json";

                var problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status403Forbidden,
                    Title = "Acesso negado",
                    Detail = "Sem permissão para o recurso",
                    Instance = context.Request.Path
                };

                await context.Response.WriteAsJsonAsync(problemDetails);
            }
        };

        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = !builder.Environment.IsDevelopment(),
            ValidateAudience = !builder.Environment.IsDevelopment(),
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            RoleClaimType = "roles" // Claim padronizada conforme ROLES_NAMING_CONVENTION.md
        };
    });

// Authorization Policies (roles em SCREAMING_SNAKE_CASE conforme convenção)
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SalesPerson", policy =>
        policy.RequireClaim("roles", "SALES_PERSON", "SALES_MANAGER", "MANAGER", "ADMIN"));

    options.AddPolicy("SalesManager", policy =>
        policy.RequireClaim("roles", "SALES_MANAGER", "MANAGER", "ADMIN"));

    options.AddPolicy("Manager", policy =>
        policy.RequireClaim("roles", "MANAGER", "ADMIN"));
});

// Swagger/OpenAPI
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "GestAuto Stock API",
        Version = "v1",
        Description = "API do Módulo de Estoque (Stock) do GestAuto"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando Bearer scheme. Exemplo: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Base path for reverse proxy routing
app.UsePathBase("/stock");

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

// Exception -> application/problem+json
app.UseMiddleware<ExceptionHandlerMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

// Convert unmapped routes to ProblemDetails
app.UseStatusCodePages(async statusCodeContext =>
{
    var httpContext = statusCodeContext.HttpContext;
    if (httpContext.Response.HasStarted)
    {
        return;
    }

    if (httpContext.Response.StatusCode is not (StatusCodes.Status404NotFound))
    {
        return;
    }

    httpContext.Response.ContentType = "application/problem+json";
    var problemDetails = new ProblemDetails
    {
        Status = StatusCodes.Status404NotFound,
        Title = "Recurso não encontrado",
        Detail = "A rota solicitada não existe",
        Instance = httpContext.Request.Path
    };

    await httpContext.Response.WriteAsJsonAsync(problemDetails);
});

app.MapHealthChecks("/health");
app.MapControllers();

// Apply migrations automatically on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<StockDbContext>();
        var database = context.Database;
        var isPostgres = database.ProviderName?.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) == true;

        if (isPostgres)
        {
            // Prevent concurrent migrations when multiple instances start at once.
            const long migrationLockKey = 739_108_517_203_441_220L;
            database.OpenConnection();
            try
            {
                database.ExecuteSqlRaw($"SELECT pg_advisory_lock({migrationLockKey});");
                database.Migrate();
            }
            finally
            {
                database.ExecuteSqlRaw($"SELECT pg_advisory_unlock({migrationLockKey});");
                database.CloseConnection();
            }
        }
        else
        {
            database.Migrate();
        }

        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Database migrated successfully.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

app.Run();

// Make Program accessible to integration tests
public partial class Program { }
