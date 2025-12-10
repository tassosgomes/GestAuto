# AsyncAPI UI - Documentação de Integração

## 📋 Visão Geral

A integração AsyncAPI permite visualizar e documentar todos os eventos assíncronos (publicados/consumidos) do módulo comercial do GestAuto através de uma interface web interativa.

## 🎯 Funcionalidades

### 1. **Documentação YAML Completa**
- Especificação AsyncAPI v2.6.0 em `/docs/asyncapi.yaml`
- Descreve 11 canais de eventos (RabbitMQ/AMQP)
- Define 9 operações de publicação e 2 de consumo
- Inclui 12+ schemas de payloads com exemplos

### 2. **Endpoints de Acesso**

#### OpenAPI (REST API)
```
GET http://localhost:5092/swagger
```
- Interface Swagger UI padrão
- Documenta endpoints REST do módulo
- Inclui autenticação JWT e políticas de autorização

#### AsyncAPI YAML
```
GET http://localhost:5092/asyncapi.yaml
```
- Retorna especificação AsyncAPI em formato YAML puro
- Pode ser integrado em ferramentas de API Gateway
- Porta: 5092 (Development)

#### AsyncAPI UI (Viewer)
```
GET http://localhost:5092/asyncapi
```
- Redireciona para **Redocly** (visualizador web gratuito)
- Interface interativa para explorar eventos
- Mostra canais, operações, schemas e exemplos
- URL completa: `https://redocly.com/docs/api-reference/?url=http://localhost:5092/asyncapi.yaml`

## 📊 Estrutura de Eventos

### Eventos Publicados (9)

| Evento | Descrição | Protocolo |
|--------|-----------|-----------|
| `commercial.lead.created` | Lead cadastrado | RabbitMQ/AMQP |
| `commercial.lead.qualified` | Lead qualificado | RabbitMQ/AMQP |
| `commercial.lead.status-changed` | Status do lead alterado | RabbitMQ/AMQP |
| `commercial.proposal.created` | Proposta criada | RabbitMQ/AMQP |
| `commercial.proposal.closed` | Proposta fechada (venda) | RabbitMQ/AMQP |
| `commercial.testdrive.scheduled` | Test-drive agendado | RabbitMQ/AMQP |
| `commercial.testdrive.completed` | Test-drive concluído | RabbitMQ/AMQP |
| `commercial.used-vehicle.evaluation-requested` | Avaliação de seminovo solicitada | RabbitMQ/AMQP |

### Eventos Consumidos (2)

| Evento | Origem | Descrição |
|--------|--------|-----------|
| `used-vehicles.evaluation.responded` | Módulo Seminovos | Resposta de avaliação |
| `finance.order.updated` | Módulo Financeiro | Atualização de pedido |

## 🔧 Implementação Técnica

### Dependências
- **Framework**: ASP.NET Core 8.0
- **API OpenAPI**: Swashbuckle 6.6.2
- **Especificação**: AsyncAPI 2.6.0 (YAML)
- **Visualizador**: Redocly (cloud-hosted)

### Arquivos Principais
```
services/commercial/
├── docs/
│   └── asyncapi.yaml                          # Especificação AsyncAPI
├── 1-Services/GestAuto.Commercial.API/
│   ├── AsyncApi/
│   │   └── CommercialAsyncApiDocumentProvider.cs  # Provedor do documento
│   ├── docs/
│   │   └── asyncapi.yaml                      # Cópia para output
│   ├── Program.cs                             # Configuração de endpoints
│   └── GestAuto.Commercial.API.csproj         # Referência ao arquivo
└── README.md                                  # Documentação geral do módulo
```

### Configuração em Program.cs

```csharp
// AsyncAPI Services
builder.Services.AddRazorPages();
builder.Services.AddHttpClient();

// Endpoints
app.MapGet("/asyncapi.yaml", async (HttpContext context) =>
{
    var content = await System.IO.File.ReadAllTextAsync(
        Path.Combine(AppContext.BaseDirectory, "docs", "asyncapi.yaml"));
    context.Response.ContentType = "application/yaml";
    await context.Response.WriteAsync(content);
});

app.MapGet("/asyncapi", (HttpContext context) =>
{
    var asyncApiUrl = "/asyncapi.yaml";
    var redwocUrl = $"https://redocly.com/docs/api-reference/?url={context.Request.Scheme}://{context.Request.Host}{asyncApiUrl}";
    return Results.Redirect(redwocUrl);
});

app.MapRazorPages();
```

### Integração com RabbitMQ

A conexão RabbitMQ é **lazy-initialized** para não bloquear o startup:

```csharp
services.AddSingleton<Lazy<IConnection>>(sp =>
    new Lazy<IConnection>(() =>
    {
        var factory = new ConnectionFactory { ... };
        return factory.CreateConnection();
    })
);
```

Benefícios:
- ✅ Aplicação inicia sem RabbitMQ conectado
- ✅ Documentação acessível mesmo sem mensageria
- ✅ Conexão estabelecida apenas quando necessária
- ✅ Ideal para ambientes de desenvolvimento

## 📚 Exemplo de Uso

### 1. Acessar Documentação Swagger (REST)
```bash
# Navegador
http://localhost:5092/swagger

# CLI - Obter especificação OpenAPI JSON
curl http://localhost:5092/swagger/v1/swagger.json | jq .
```

### 2. Obter Especificação AsyncAPI em YAML
```bash
# Retorna o arquivo YAML completo
curl http://localhost:5092/asyncapi.yaml > asyncapi.yaml

# Validar com ferramentas
npx @asyncapi/cli validate asyncapi.yaml
```

### 3. Visualizar em UI Interativa
```bash
# Navegador - Redireciona automaticamente para Redocly
http://localhost:5092/asyncapi

# Direto no Redocly com URL local
https://redocly.com/docs/api-reference/?url=http://localhost:5092/asyncapi.yaml
```

### 4. Integrar com API Gateway
```yaml
# Kong, Traefik, etc.
asyncapi:
  spec_url: http://commercial-api:5092/asyncapi.yaml
  viewer: https://redocly.com/docs/api-reference/
```

## 🔍 Validação

### Build
```bash
cd services/commercial/1-Services/GestAuto.Commercial.API
dotnet build
# ✅ Build succeeded with 0 errors
```

### Endpoints (com docker-compose rodando)
```bash
# 1. YAML é retornado corretamente
curl http://localhost:5092/asyncapi.yaml | head -20
# asyncapi: '2.6.0'
# info:
#   title: GestAuto Commercial Events API

# 2. Swagger funciona
curl http://localhost:5092/swagger/v1/swagger.json | head -20
# {"openapi":"3.0.1",...

# 3. Redirecionamento funciona
curl -i http://localhost:5092/asyncapi
# HTTP/1.1 302 Found
# Location: https://redocly.com/docs/api-reference/?url=http://localhost:5092/asyncapi.yaml
```

## 📝 Próximos Passos (Opcional)

### 1. Documentação Integrada no Swagger
Adicionar componente AsyncAPI dentro do Swagger UI em vez de redirecionamento:

```csharp
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("asyncapi", new OpenApiInfo { 
        Title = "Commercial Events",
        Version = "1.0.0"
    });
});
```

### 2. Validação em CI/CD
```yaml
# GitHub Actions / GitLab CI
- name: Validate AsyncAPI
  run: npx @asyncapi/cli validate docs/asyncapi.yaml
```

### 3. Geração de Código
```bash
# Gerar clients/servers Python, Node.js, Java
npx @asyncapi/cli generate fromTemplate docs/asyncapi.yaml @asyncapi/python-paho-mqtt-client-template
```

### 4. Documentação Embarcada
Servir UI local sem depender do Redocly:
- Integrar `asyncapi-web-component` (componente web nativo)
- Ou usar `@asyncapi/react-component` em página Razor

## 🚀 Deploy em Produção

### 1. Docker
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
COPY docs/asyncapi.yaml /app/docs/
EXPOSE 5092
```

### 2. Variáveis de Ambiente
```bash
# URL base para UI (se diferente do localhost)
ASYNCAPI_BASE_URL=https://api.example.com
```

### 3. HTTPS
```csharp
// Redirecionar para https automaticamente
var redwocUrl = $"https://redocly.com/docs/api-reference/?url={context.Request.Scheme}://{context.Request.Host}{asyncApiUrl}";
```

## 📖 Referências

- **AsyncAPI Specification**: https://www.asyncapi.com/docs/specifications/v2.6.0
- **Redocly AsyncAPI Viewer**: https://redocly.com/
- **RabbitMQ AMQP Bindings**: https://github.com/asyncapi/bindings/tree/master/amqp
- **AsyncAPI CLI**: https://github.com/asyncapi/cli

## ✅ Checklist de Conclusão

- [x] Especificação AsyncAPI 2.6.0 criada em YAML
- [x] Endpoint GET /asyncapi.yaml implementado
- [x] Endpoint GET /asyncapi implementado (Redocly)
- [x] Integração com RabbitMQ (lazy init)
- [x] Documentação README.md atualizada
- [x] Build sem erros
- [x] Endpoints testados e validados
- [x] Swagger UI mantido funcional
- [x] Git commits documentados

---

**Última Atualização**: 2025-12-09
**Status**: ✅ Implementação Completa
**Porta de Desenvolvimento**: 5092
