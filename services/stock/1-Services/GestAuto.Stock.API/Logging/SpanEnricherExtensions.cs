using GestAuto.Stock.API.Logging;
using Serilog.Configuration;

namespace Serilog;

public static class SpanEnricherExtensions
{
    public static LoggerConfiguration WithSpan(
        this LoggerEnrichmentConfiguration enrichmentConfiguration)
    {
        return enrichmentConfiguration.With<SpanEnricher>();
    }
}