using System.Reflection;
using System.Text;

namespace GestAuto.Commercial.API.AsyncApi;

/// <summary>
/// Provedor de documentação AsyncAPI para o módulo Commercial
/// Serve a documentação AsyncAPI diretamente do arquivo YAML embarcado
/// </summary>
public static class CommercialAsyncApiDocumentProvider
{
    /// <summary>
    /// Obtém a documentação AsyncAPI em formato YAML
    /// Carrega do arquivo embarcado "asyncapi.yaml"
    /// </summary>
    public static async Task<string> GetAsyncApiDocumentationAsync()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = "GestAuto.Commercial.API.docs.asyncapi.yaml";

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            throw new InvalidOperationException(
                $"Arquivo de documentação AsyncAPI não encontrado: {resourceName}. " +
                "Certifique-se que asyncapi.yaml está no projeto como 'Embedded Resource'.");
        }

        using var reader = new StreamReader(stream, Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }
}
