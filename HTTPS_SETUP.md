# Configuração HTTPS para Desenvolvimento

## Visão Geral

O GestAuto agora utiliza HTTPS para todos os serviços em desenvolvimento para resolver problemas de segurança relacionados à Web Crypto API (necessária para autenticação com Keycloak).

## Certificados Autoassinados

Os certificados estão localizados em `traefik/certs/`:
- `cert.pem` - Certificado público
- `key.pem` - Chave privada

Esses certificados são válidos para:
- `*.tasso.local`
- `keycloak.tasso.local`
- `vehicle-evaluation.tasso.local`
- `commercial.tasso.local`
- `gestauto.tasso.local`

## Como Aceitar o Certificado no Navegador

### Google Chrome / Chromium / Edge

1. Abra `https://gestauto.tasso.local/` no navegador
2. Você verá um aviso "Sua conexão não é particular"
3. Clique em "Avançado"
4. Clique em "Continuar para gestauto.tasso.local (não seguro)"

**OU** (para adicionar permanentemente):

1. Abra Chrome e vá para `chrome://settings/certificates`
2. Vá para a aba "Autoridades"
3. Clique em "Importar"
4. Selecione `traefik/certs/cert.pem`
5. Marque "Confiar neste certificado para identificar sites"
6. Clique em "OK"

### Firefox

1. Abra `https://gestauto.tasso.local/` no navegador
2. Você verá um aviso "Aviso: Risco potencial de segurança à frente"
3. Clique em "Avançado"
4. Clique em "Aceitar o risco e continuar"

**OU** (para adicionar permanentemente):

1. Abra Firefox e vá para `about:preferences#privacy`
2. Role até "Certificados" e clique em "Ver certificados"
3. Vá para a aba "Autoridades"
4. Clique em "Importar"
5. Selecione `traefik/certs/cert.pem`
6. Marque "Confiar neste certificado para identificar sites"
7. Clique em "OK"

## Acessando os Serviços

Todos os serviços agora devem ser acessados via HTTPS:

- Frontend: https://gestauto.tasso.local/
- Keycloak: https://keycloak.tasso.local/
- Vehicle Evaluation API: https://vehicle-evaluation.tasso.local/
- Commercial API: https://commercial.tasso.local/

### Configuração das APIs

As APIs foram configuradas para aceitar certificados autoassinados em desenvolvimento:

**Java (Vehicle Evaluation):**
- Flags JVM adicionadas: `-Djavax.net.ssl.trustAll=true -Djdk.internal.httpclient.disableHostnameVerification=true`
- URLs do Keycloak atualizadas para HTTPS

**C# (.NET - Commercial):**
- `HttpClientHandler` configurado para aceitar certificados autoassinados em modo Development
- `JwtBearer` configurado com `BackchannelHttpHandler` customizado para aceitar certificados autoassinados
- URLs do Keycloak atualizadas para HTTPS

## Resolução de Problemas

### Erro "Web Crypto API is not available"

Este erro ocorre quando o site é acessado via HTTP em vez de HTTPS. Certifique-se de usar `https://` nos URLs.

### Erro "NET::ERR_CERT_AUTHORITY_INVALID"

Este é o comportamento esperado com certificados autoassinados. Siga as instruções acima para aceitar o certificado no seu navegador.

### Keycloak não aceita redirect

Certifique-se de que o Keycloak foi reconfigurado com as URLs HTTPS:

```bash
cd /home/tsgomes/github-tassosgomes/GestAuto
KEYCLOAK_BASE_URL=https://keycloak.tasso.local \
KEYCLOAK_ADMIN_USER=admin \
KEYCLOAK_ADMIN_PASSWORD=admin \
./scripts/keycloak/configure_gestauto.sh
```

## Regenerar Certificados

Se necessário regenerar os certificados:

```bash
cd traefik/certs
openssl req -x509 -newkey rsa:4096 -keyout key.pem -out cert.pem -days 365 -nodes \
  -subj "/CN=*.tasso.local" \
  -addext "subjectAltName=DNS:*.tasso.local,DNS:tasso.local,DNS:keycloak.tasso.local,DNS:vehicle-evaluation.tasso.local,DNS:commercial.tasso.local,DNS:gestauto.tasso.local"

# Reiniciar Traefik
cd ../..
docker compose restart traefik
```
