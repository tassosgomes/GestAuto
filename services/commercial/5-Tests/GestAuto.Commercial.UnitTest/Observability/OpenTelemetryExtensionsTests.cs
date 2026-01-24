using FluentAssertions;
using GestAuto.Commercial.API.Extensions;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace GestAuto.Commercial.UnitTest.Observability;

public class OpenTelemetryExtensionsTests
{
    [Theory]
    [InlineData("/health", true)]
    [InlineData("/health/", true)]
    [InlineData("/ready", true)]
    [InlineData("/swagger", true)]
    [InlineData("/swagger/v1/swagger.json", true)]
    [InlineData("/api/leads", false)]
    [InlineData("/api/health-metrics", false)]
    public void ShouldIgnorePath_ShouldMatchExpected(string path, bool expected)
    {
        var result = OpenTelemetryExtensions.ShouldIgnorePath(new PathString(path));

        result.Should().Be(expected);
    }
}
