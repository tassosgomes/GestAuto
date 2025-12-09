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

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDbContext<CommercialDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("CommercialDatabase")));

// Add infrastructure services
builder.Services.AddInfrastructureServices(builder.Configuration);

// Add RabbitMQ services
builder.Services.AddRabbitMq(builder.Configuration);

// Add application services
builder.Services.AddApplicationServices();

// Health checks
builder.Services.AddHealthChecks()
    .AddCheck<RabbitMqHealthCheck>("rabbitmq");

// Authentication - Logto JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Logto:Authority"];
        options.Audience = builder.Configuration["Logto:Audience"];
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };
    });

// Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SalesPerson", policy => 
        policy.RequireClaim("role", "sales_person", "manager"));
    
    options.AddPolicy("Manager", policy => 
        policy.RequireClaim("role", "manager"));
});

// Services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ISalesPersonFilterService, SalesPersonFilterService>();

// Background Services
builder.Services.AddHostedService<OutboxProcessorService>();

// Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "GestAuto Commercial API",
        Version = "v1",
        Description = "API do Módulo Comercial do GestAuto - Gestão de Leads, Propostas e Vendas"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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

    // Include XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Use exception handler middleware
app.UseMiddleware<ExceptionHandlerMiddleware>();

// Authentication and Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");

app.MapControllers();

app.Run();

// Make Program accessible to integration tests
public partial class Program { }
