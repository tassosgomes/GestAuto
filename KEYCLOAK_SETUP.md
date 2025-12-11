# Configuração do Keycloak para GestAuto

Este documento descreve as configurações necessárias no Keycloak para suportar os serviços `commercial` e `vehicle-evaluation`.

> **📋 Convenção de Roles:** Todas as roles seguem o padrão `SCREAMING_SNAKE_CASE` conforme definido em [rules/ROLES_NAMING_CONVENTION.md](rules/ROLES_NAMING_CONVENTION.md).

## 1. Realm

Crie um novo Realm (ou use o existente) com o nome:
*   **Realm Name:** `gestauto`

## 2. Clients

É necessário configurar clientes para as APIs.

### 2.1. Commercial API
Baseado na configuração encontrada em `services/commercial/.../appsettings.json` e `Program.cs`.

*   **Client ID:** `gestauto-commercial-api`
*   **Client Protocol:** `openid-connect`
*   **Access Type:** `bearer-only`

**Configuração de Mappers (Obrigatório para todos os clients):**
Criar um **Protocol Mapper** para padronizar a claim de roles:
*   **Name:** `Roles Mapper`
*   **Mapper Type:** `User Realm Role`
*   **Token Claim Name:** `roles`
*   **Claim JSON Type:** `String` (Multivalued: On)
*   **Add to access token:** `On`
*   **Add to ID token:** `On`

**Configuração no Projeto (.NET):**
*   Authority: `http://localhost:8080/realms/gestauto`
*   Audience: `gestauto-commercial-api`

### 2.2. Vehicle Evaluation API
Baseado na análise do código em `services/vehicle-evaluation`. O projeto utiliza Spring Security.

*   **Client ID:** `vehicle-evaluation-api`
*   **Client Protocol:** `openid-connect`
*   **Access Type:** `bearer-only`

**Configuração de Mappers:**
Aplicar o mesmo mapper descrito na seção 2.1 (Token Claim Name: `roles`).

## 3. Roles (Realm Roles)

Crie as seguintes roles no nível do Realm (padrão `SCREAMING_SNAKE_CASE`):

### 3.1. Roles Globais
| Role | Descrição |
|------|-----------|
| `ADMIN` | Acesso administrativo geral |
| `MANAGER` | Gerentes - acesso cross-service |
| `VIEWER` | Apenas visualização |

### 3.2. Roles do Commercial
| Role | Descrição |
|------|-----------|
| `SALES_PERSON` | Vendedores |
| `SALES_MANAGER` | Gerente de vendas |

### 3.3. Roles do Vehicle Evaluation
| Role | Descrição |
|------|-----------|
| `VEHICLE_EVALUATOR` | Avaliadores de veículos |
| `EVALUATION_MANAGER` | Gerente de avaliações |

## 4. Usuários de Teste

Sugestão de usuários para validar os perfis:

| Username | Password | Roles |
| :--- | :--- | :--- |
| `admin` | `admin` | `ADMIN`, `MANAGER` |
| `sales_manager` | `123456` | `MANAGER`, `SALES_MANAGER`, `SALES_PERSON` |
| `seller` | `123456` | `SALES_PERSON` |
| `eval_manager` | `123456` | `MANAGER`, `EVALUATION_MANAGER`, `VEHICLE_EVALUATOR` |
| `evaluator` | `123456` | `VEHICLE_EVALUATOR` |

## 5. Configuração por Framework

### 5.1. Commercial Service (.NET)

**Arquivo:** `appsettings.json`
```json
{
  "Keycloak": {
    "Authority": "http://localhost:8080/realms/gestauto",
    "Audience": "gestauto-commercial-api"
  }
}
```

**Arquivo:** `Program.cs`
```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Keycloak:Authority"];
        options.Audience = builder.Configuration["Keycloak:Audience"];
        options.TokenValidationParameters = new TokenValidationParameters
        {
            RoleClaimType = "roles" // Claim padronizada
        };
    });
```

### 5.2. Vehicle Evaluation Service (Java/Spring)

**Dependência Maven (pom.xml):**
```xml
<dependency>
    <groupId>org.springframework.boot</groupId>
    <artifactId>spring-boot-starter-oauth2-resource-server</artifactId>
</dependency>
```

**Arquivo:** `application.yml`
```yaml
spring:
  security:
    oauth2:
      resourceserver:
        jwt:
          issuer-uri: http://localhost:8080/realms/gestauto
          jwk-set-uri: http://localhost:8080/realms/gestauto/protocol/openid-connect/certs
```

**Configuração do JwtAuthenticationConverter:**
```java
@Bean
public JwtAuthenticationConverter jwtAuthenticationConverter() {
    JwtGrantedAuthoritiesConverter grantedAuthoritiesConverter = new JwtGrantedAuthoritiesConverter();
    grantedAuthoritiesConverter.setAuthoritiesClaimName("roles");
    grantedAuthoritiesConverter.setAuthorityPrefix("ROLE_");

    JwtAuthenticationConverter jwtAuthenticationConverter = new JwtAuthenticationConverter();
    jwtAuthenticationConverter.setJwtGrantedAuthoritiesConverter(grantedAuthoritiesConverter);
    return jwtAuthenticationConverter;
}
```

**Uso no SecurityConfig:**
```java
// hasRole() adiciona ROLE_ automaticamente, usar role sem prefixo
.requestMatchers("/api/evaluations/**").hasAnyRole("VEHICLE_EVALUATOR", "EVALUATION_MANAGER", "ADMIN")
.requestMatchers("/api/admin/**").hasRole("ADMIN")
```

## 6. Troubleshooting

### Token não contém as roles
- Verifique se o Protocol Mapper está configurado com `Token Claim Name: roles`
- Confirme que `Add to access token` está habilitado

### Spring Security não reconhece as roles
- Verifique se o `JwtAuthenticationConverter` está configurado corretamente
- Lembre-se: `hasRole("ADMIN")` espera `ROLE_ADMIN` internamente

### .NET não autoriza usuário com role correta
- Confirme que `RoleClaimType = "roles"` está configurado no `TokenValidationParameters`
