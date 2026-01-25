using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace GestAuto.Stock.API.Extensions;

public static class OpenTelemetryExtensions
{
    private static readonly string[] IgnoredPaths = ["/health", "/ready", "/swagger"];

    public static IServiceCollection AddObservability(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var serviceName = configuration["OTEL_SERVICE_NAME"] ?? "stock";
        var serviceVersion = configuration["OTEL_SERVICE_VERSION"] ?? "1.0.0";
        var otlpEndpoint = configuration["OTEL_EXPORTER_OTLP_ENDPOINT"] ?? "http://otel-collector:4317";
        var otlpProtocolSetting = configuration["OTEL_EXPORTER_OTLP_PROTOCOL"] ?? "grpc";

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(
                    serviceName: serviceName,
                    serviceVersion: serviceVersion,
                    serviceInstanceId: Environment.MachineName))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation(options =>
                {
                    options.Filter = context => !ShouldIgnorePath(context.Request.Path);
                    options.RecordException = true;
                    options.EnrichWithHttpRequest = (activity, request) =>
                    {
                        activity.SetTag("http.request.path", request.Path);
                    };
                    options.EnrichWithHttpResponse = (activity, response) =>
                    {
                        activity.SetTag("http.response.status_code", response.StatusCode);
                    };
                })
                .AddHttpClientInstrumentation(options =>
                {
                    options.RecordException = true;
                    options.FilterHttpRequestMessage = request =>
                        request.RequestUri == null ||
                        !request.RequestUri.Host.Contains("otel-collector", StringComparison.OrdinalIgnoreCase);
                    options.EnrichWithHttpRequestMessage = (activity, request) =>
                    {
                        activity.SetTag("http.request.uri", SanitizeUrl(request.RequestUri));
                    };
                })
                .AddEntityFrameworkCoreInstrumentation(options =>
                {
                    options.SetDbStatementForText = true;
                    options.SetDbStatementForStoredProcedure = true;
                })
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(otlpEndpoint);
                    options.Protocol = ResolveOtlpProtocol(otlpProtocolSetting);
                    options.ExportProcessorType = ExportProcessorType.Batch;
                    options.BatchExportProcessorOptions = new BatchExportProcessorOptions<Activity>
                    {
                        MaxQueueSize = 2048,
                        ScheduledDelayMilliseconds = 5000,
                        ExporterTimeoutMilliseconds = 5000,
                        MaxExportBatchSize = 512
                    };
                }));

        return services;
    }

    public static ILoggingBuilder AddObservabilityLogging(
        this ILoggingBuilder logging,
        IConfiguration configuration)
    {
        var serviceName = configuration["OTEL_SERVICE_NAME"] ?? "stock";
        var serviceVersion = configuration["OTEL_SERVICE_VERSION"] ?? "1.0.0";
        var otlpEndpoint = configuration["OTEL_EXPORTER_OTLP_ENDPOINT"] ?? "http://otel-collector:4317";
        var otlpProtocolSetting = configuration["OTEL_EXPORTER_OTLP_PROTOCOL"] ?? "grpc";

        logging.AddOpenTelemetry(options =>
        {
            options.SetResourceBuilder(ResourceBuilder.CreateDefault()
                .AddService(
                    serviceName: serviceName,
                    serviceVersion: serviceVersion,
                    serviceInstanceId: Environment.MachineName));

            options.IncludeFormattedMessage = true;
            options.IncludeScopes = true;
            options.ParseStateValues = true;

            options.AddOtlpExporter(otlpOptions =>
            {
                otlpOptions.Endpoint = new Uri(otlpEndpoint);
                otlpOptions.Protocol = ResolveOtlpProtocol(otlpProtocolSetting);
            });
        });

        return logging;
    }
    internal static bool ShouldIgnorePath(PathString path)
    {
        var pathValue = path.Value ?? string.Empty;
        return IgnoredPaths.Any(ignored =>
            pathValue.StartsWith(ignored, StringComparison.OrdinalIgnoreCase));
    }

    private static string? SanitizeUrl(Uri? uri)
    {
        if (uri == null)
        {
            return null;
        }

        return uri.IsAbsoluteUri
            ? $"{uri.Scheme}://{uri.Host}{uri.AbsolutePath}"
            : uri.ToString();
    }

    private static OtlpExportProtocol ResolveOtlpProtocol(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return OtlpExportProtocol.Grpc;
        }

        return value.Equals("http/protobuf", StringComparison.OrdinalIgnoreCase)
            || value.Equals("http", StringComparison.OrdinalIgnoreCase)
            ? OtlpExportProtocol.HttpProtobuf
            : OtlpExportProtocol.Grpc;
    }
}