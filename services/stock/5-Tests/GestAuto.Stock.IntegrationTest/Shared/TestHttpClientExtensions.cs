using System.Net.Http.Headers;

namespace GestAuto.Stock.IntegrationTest.Shared;

internal static class TestHttpClientExtensions
{
    public static HttpClient WithTestAuth(this HttpClient client, Guid userId, string role)
    {
        client.DefaultRequestHeaders.Remove("X-Test-UserId");
        client.DefaultRequestHeaders.Remove("X-Test-Role");

        client.DefaultRequestHeaders.Add("X-Test-UserId", userId.ToString());
        client.DefaultRequestHeaders.Add("X-Test-Role", role);

        // Ensure JSON by default
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        return client;
    }
}
