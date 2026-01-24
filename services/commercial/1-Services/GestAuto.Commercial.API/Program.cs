using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using GestAuto.Commercial.Infra;
using GestAuto.Commercial.Infra.Messaging;
using GestAuto.Commercial.Infra.HealthChecks;
using GestAuto.Commercial.Application;
using GestAuto.Commercial.API.Middleware;
using GestAuto.Commercial.API.Services;
using System.Reflection;
using Saunter;
using Saunter.AsyncApiSchema.v2;
using GestAuto.Commercial.Domain.Events;
using System.Security.Claims;
using System.Text.Json;
using GestAuto.Commercial.API.Extensions;
using Serilog;

// Type aliases para evitar conflitos entre namespaces
using AsyncApiInfo = Saunter.AsyncApiSchema.v2.Info;
using AsyncApiContact = Saunter.AsyncApiSchema.v2.Contact;
using AsyncApiLicense = Saunter.AsyncApiSchema.v2.License;
using AsyncApiServer = Saunter.AsyncApiSchema.v2.Server;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, loggerConfiguration) =>
{
    var serviceName = context.Configuration["OTEL_SERVICE_NAME"] ?? "commercial";
    var serviceVersion = context.Configuration["OTEL_SERVICE_VERSION"] ?? "1.0.0";

    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.WithProperty("service.name", serviceName)
        .Enrich.WithProperty("service.version", serviceVersion);
});

builder.Logging.AddObservabilityLogging(builder.Configuration);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();
builder.Services.AddDbContext<CommercialDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("CommercialDatabase")));

builder.Services.AddObservability(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod());
});

// Add infrastructure services
builder.Services.AddInfrastructureServices(builder.Configuration);

// Add RabbitMQ services
builder.Services.AddRabbitMq(builder.Configuration);

// Add application services
builder.Services.AddApplicationServices();

// Health checks
builder.Services.AddHealthChecks();
    // .AddCheck<RabbitMqHealthCheck>("rabbitmq"); // Desabilitado por enquanto - RabbitMQ é lazy initialized

// Authentication - Keycloak JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Keycloak:Authority"];
        options.Audience = builder.Configuration["Keycloak:Audience"];
        options.MapInboundClaims = false;
        options.RequireHttpsMetadata = false; // Facilitates local development
        
        // Aceitar certificados autoassinados em desenvolvimento
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
    
    options.AddPolicy("Manager", policy => 
        policy.RequireClaim("roles", "MANAGER", "SALES_MANAGER", "ADMIN"));
});

// Services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ISalesPersonFilterService, SalesPersonFilterService>();

// Background Services
builder.Services.AddHostedService<OutboxProcessorService>();

// Swagger/OpenAPI
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "GestAuto Commercial API",
        Version = "v1",
        Description = @"
API do Módulo Comercial do GestAuto - Sistema de Gestão para Concessionárias de Veículos.

## Funcionalidades Principais

- **Gestão de Leads**: Cadastro, qualificação e acompanhamento de potenciais clientes
- **Propostas Comerciais**: Criação e gestão de propostas de venda
- **Test-Drives**: Agendamento e controle de test-drives
- **Avaliações de Seminovos**: Solicitação e acompanhamento de avaliações

## Autenticação

Esta API utiliza autenticação JWT via Keycloak. Inclua o token no header:

```
Authorization: Bearer <token>
```

## Roles e Permissões

- **Vendedor (SALES_PERSON)**: Acesso aos próprios leads e propostas
- **Gerente (MANAGER/SALES_MANAGER)**: Acesso a todos os registros + aprovação de descontos
- **Admin (ADMIN)**: Acesso administrativo completo

## Códigos de Status

- `200 OK`: Requisição bem-sucedida
- `201 Created`: Recurso criado com sucesso
- `400 Bad Request`: Dados inválidos ou erro de negócio
- `401 Unauthorized`: Token não fornecido ou inválido
- `403 Forbidden`: Sem permissão para o recurso
- `404 Not Found`: Recurso não encontrado
- `500 Internal Server Error`: Erro interno do servidor
",
        Contact = new OpenApiContact
        {
            Name = "GestAuto Team",
            Email = "suporte@gestauto.com.br"
        },
        License = new OpenApiLicense
        {
            Name = "Proprietary"
        }
    });

    // JWT Bearer Authentication
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando Bearer scheme. Exemplo: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
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

    // Include XML comments for method documentation
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    // Group endpoints by controller tags
    options.TagActionsBy(api =>
    {
        if (api.GroupName != null)
            return new[] { api.GroupName };

        if (api.ActionDescriptor is Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor controllerActionDescriptor)
            return new[] { controllerActionDescriptor.ControllerName };

        return new[] { "Other" };
    });

    options.DocInclusionPredicate((name, api) => true);

    // Enable annotations
    options.EnableAnnotations();
});

// AsyncAPI Services with Saunter
builder.Services.AddAsyncApiSchemaGeneration(options =>
{
    options.AsyncApi = new AsyncApiDocument
    {
        Info = new AsyncApiInfo("GestAuto Commercial Events API", "1.0.0")
        {
            Description = @"
API de eventos assíncronos do Módulo Comercial do GestAuto.

Este documento descreve os eventos publicados e consumidos pelo módulo comercial
via RabbitMQ para integração com outros módulos do sistema.

## Transporte
- **Broker**: RabbitMQ
- **Exchange**: gestauto.commercial
- **Tipo de Exchange**: Topic

## Padrão de Mensagens
Todos os eventos seguem o padrão CloudEvents e são serializados em JSON.",
            Contact = new AsyncApiContact
            {
                Name = "GestAuto Team",
                Email = "suporte@gestauto.com.br",
                Url = "https://gestauto.com.br"
            },
            License = new AsyncApiLicense("Proprietary")
            {
                Url = "https://gestauto.com.br/license"
            }
        },
        Servers = new Dictionary<string, AsyncApiServer>
        {
            ["development"] = new AsyncApiServer("localhost:5672", "amqp")
            {
                Description = "RabbitMQ Development Server"
            },
            ["production"] = new AsyncApiServer("rabbitmq.gestauto.com.br:5672", "amqp")
            {
                Description = "RabbitMQ Production Server"
            }
        }
    };

    // Configurar assemblies para scan de mensagens
    options.AssemblyMarkerTypes = new[] { typeof(LeadCreatedEvent) };
});

builder.Services.AddRazorPages();
if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddHttpClient();
}

var app = builder.Build();

// Base path for reverse proxy routing
app.UsePathBase("/commercial/api");

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors("AllowAll");

// Use exception handler middleware
app.UseMiddleware<ExceptionHandlerMiddleware>();

// Authentication and Authorization
app.UseAuthentication();
app.UseAuthorization();

// AsyncAPI Documentation Endpoints with Saunter
app.MapAsyncApiDocuments();
app.MapAsyncApiUi();

// Manter endpoint YAML estático como fallback
app.MapGet("/asyncapi-static.yaml", async (HttpContext context) =>
{
    var content = await System.IO.File.ReadAllTextAsync(
        Path.Combine(AppContext.BaseDirectory, "docs", "asyncapi.yaml"));
    context.Response.ContentType = "application/yaml";
    await context.Response.WriteAsync(content);
}).WithName("GetAsyncApiStaticDocumentation")
.WithOpenApi()
.WithDescription("Retorna a especificação AsyncAPI estática em formato YAML");

app.MapRazorPages();

app.MapHealthChecks("/health");

app.MapControllers();

// Apply migrations automatically on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<CommercialDbContext>();
        var database = context.Database;
        var isPostgres = database.ProviderName?.Contains("Npgsql", StringComparison.OrdinalIgnoreCase) == true;

        if (isPostgres)
        {
            // Prevent concurrent migrations when multiple instances start at once (e.g. integration tests).
            // Advisory locks are scoped to the current connection/session.
            const long migrationLockKey = 739_108_517_203_441_219L;
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
