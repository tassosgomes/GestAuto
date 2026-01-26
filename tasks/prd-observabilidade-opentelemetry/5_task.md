---
status: in_progress
parallelizable: false
blocked_by: ["2.0", "3.0", "4.0"]
---

<task_context>
<domain>frontend</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>medium</complexity>
<dependencies>otel-collector, axios, react</dependencies>
<unblocks>6.0</unblocks>
</task_context>

# Tarefa 5.0: Instrumentação OpenTelemetry - frontend (React)

## Visão Geral

Implementar instrumentação OpenTelemetry no frontend React (Vite), incluindo traces para carregamento de página, interações do usuário e chamadas HTTP via Axios. A telemetria deve propagar o contexto W3C Trace Context nos headers das requisições para permitir rastreamento end-to-end com os backends.

**Importante:** A instrumentação deve ser habilitada **apenas em produção**, não em desenvolvimento local.

<requirements>
- Adicionar pacotes npm do OpenTelemetry Web SDK
- Criar módulo de telemetria com inicialização condicional
- Configurar auto-instrumentação (document-load, fetch, user-interaction)
- Implementar interceptor Axios para propagação de headers W3C
- Criar hook useTracing para spans manuais
- Configurar exportação OTLP/HTTP via Traefik (otel.tasso.dev.br)
- Habilitar apenas em produção (env check)
</requirements>

## Subtarefas

- [x] 5.1 Adicionar pacotes npm do OpenTelemetry
- [x] 5.2 Criar módulo de telemetria (`src/telemetry/index.ts`)
- [x] 5.3 Criar hook `useTracing` para spans manuais
- [x] 5.4 Atualizar interceptor Axios para propagação de contexto
- [x] 5.5 Inicializar telemetria no `main.tsx` (apenas em produção)
- [x] 5.6 Atualizar `app-config.json` com endpoint OTel
- [x] 5.7 Criar testes para interceptor Axios
- [ ] 5.8 Validar traces no Grafana/Tempo após deploy

## Detalhes de Implementação

### 5.1 Pacotes npm

```bash
npm install @opentelemetry/api \
            @opentelemetry/sdk-trace-web \
            @opentelemetry/exporter-trace-otlp-http \
            @opentelemetry/context-zone \
            @opentelemetry/auto-instrumentations-web \
            @opentelemetry/resources \
            @opentelemetry/semantic-conventions \
            @opentelemetry/instrumentation
```

### 5.2 Módulo de Telemetria

Criar `src/telemetry/index.ts`:

```typescript
import { WebTracerProvider } from '@opentelemetry/sdk-trace-web';
import { BatchSpanProcessor } from '@opentelemetry/sdk-trace-base';
import { OTLPTraceExporter } from '@opentelemetry/exporter-trace-otlp-http';
import { ZoneContextManager } from '@opentelemetry/context-zone';
import { registerInstrumentations } from '@opentelemetry/instrumentation';
import { getWebAutoInstrumentations } from '@opentelemetry/auto-instrumentations-web';
import { Resource } from '@opentelemetry/resources';
import { 
  ATTR_SERVICE_NAME, 
  ATTR_SERVICE_VERSION 
} from '@opentelemetry/semantic-conventions';
import { trace, context, SpanStatusCode } from '@opentelemetry/api';

let isInitialized = false;

export interface TelemetryConfig {
  serviceName: string;
  serviceVersion: string;
  otlpEndpoint: string;
}

export function initTelemetry(config: TelemetryConfig): void {
  if (isInitialized) {
    console.warn('Telemetry already initialized');
    return;
  }

  const resource = new Resource({
    [ATTR_SERVICE_NAME]: config.serviceName,
    [ATTR_SERVICE_VERSION]: config.serviceVersion,
  });

  const provider = new WebTracerProvider({
    resource,
  });

  const exporter = new OTLPTraceExporter({
    url: config.otlpEndpoint,
    headers: {},
  });

  provider.addSpanProcessor(
    new BatchSpanProcessor(exporter, {
      maxQueueSize: 100,
      maxExportBatchSize: 50,
      scheduledDelayMillis: 5000,
      exportTimeoutMillis: 5000,
    })
  );

  provider.register({
    contextManager: new ZoneContextManager(),
  });

  registerInstrumentations({
    instrumentations: [
      getWebAutoInstrumentations({
        '@opentelemetry/instrumentation-document-load': {
          enabled: true,
        },
        '@opentelemetry/instrumentation-fetch': {
          enabled: true,
          propagateTraceHeaderCorsUrls: [
            /https:\/\/.*\.tasso\.dev\.br/,
            /http:\/\/localhost:\d+/,
          ],
          clearTimingResources: true,
        },
        '@opentelemetry/instrumentation-user-interaction': {
          enabled: true,
          eventNames: ['click', 'submit'],
        },
        '@opentelemetry/instrumentation-xml-http-request': {
          enabled: false, // Usamos Axios com interceptor próprio
        },
      }),
    ],
  });

  isInitialized = true;
  console.info('OpenTelemetry initialized for frontend');
}

export function getTracer(name: string = 'frontend') {
  return trace.getTracer(name);
}

export function getCurrentContext() {
  return context.active();
}

export { trace, context, SpanStatusCode };
```

### 5.3 Hook useTracing

Criar `src/telemetry/useTracing.ts`:

```typescript
import { useCallback } from 'react';
import { trace, context, SpanStatusCode, Span } from '@opentelemetry/api';

const tracer = trace.getTracer('frontend-hooks');

interface UseTracingOptions {
  spanName: string;
  attributes?: Record<string, string | number | boolean>;
}

export function useTracing() {
  const startSpan = useCallback(
    (options: UseTracingOptions): Span => {
      const span = tracer.startSpan(options.spanName, {
        attributes: options.attributes,
      });
      return span;
    },
    []
  );

  const withSpan = useCallback(
    async <T>(options: UseTracingOptions, fn: () => Promise<T>): Promise<T> => {
      const span = startSpan(options);
      try {
        const result = await context.with(
          trace.setSpan(context.active(), span),
          fn
        );
        span.setStatus({ code: SpanStatusCode.OK });
        return result;
      } catch (error) {
        span.setStatus({
          code: SpanStatusCode.ERROR,
          message: error instanceof Error ? error.message : 'Unknown error',
        });
        if (error instanceof Error) {
          span.recordException(error);
        }
        throw error;
      } finally {
        span.end();
      }
    },
    [startSpan]
  );

  return { startSpan, withSpan };
}
```

### 5.4 Interceptor Axios

Atualizar `src/lib/api.ts`:

```typescript
import axios from 'axios';
import { trace, context, propagation } from '@opentelemetry/api';

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
  timeout: 30000,
});

// Interceptor para propagação de contexto W3C Trace Context
api.interceptors.request.use(
  (config) => {
    // Apenas propagar se telemetria estiver ativa (produção)
    if (import.meta.env.PROD) {
      const activeContext = context.active();
      const headers: Record<string, string> = {};
      
      // Injetar headers W3C Trace Context (traceparent, tracestate)
      propagation.inject(activeContext, headers);
      
      config.headers = {
        ...config.headers,
        ...headers,
      };
    }
    
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Interceptor para registrar erros no span
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (import.meta.env.PROD) {
      const span = trace.getActiveSpan();
      if (span) {
        span.setStatus({
          code: 2, // SpanStatusCode.ERROR
          message: error.message,
        });
        span.recordException(error);
      }
    }
    return Promise.reject(error);
  }
);

export default api;
```

### 5.5 Inicialização no main.tsx

Atualizar `src/main.tsx`:

```typescript
import React from 'react';
import ReactDOM from 'react-dom/client';
import App from './App';
import './index.css';

// Inicializar telemetria apenas em produção
if (import.meta.env.PROD) {
  import('./telemetry').then(({ initTelemetry }) => {
    // Carregar configuração do app-config.json
    fetch('/app-config.json')
      .then((res) => res.json())
      .then((config) => {
        initTelemetry({
          serviceName: 'frontend',
          serviceVersion: config.version || '1.0.0',
          otlpEndpoint: config.otelEndpoint || 'https://otel.tasso.dev.br/v1/traces',
        });
      })
      .catch((err) => {
        console.error('Failed to load telemetry config:', err);
      });
  });
}

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <App />
  </React.StrictMode>
);
```

### 5.6 Atualizar app-config.json

Atualizar `public/app-config.json`:

```json
{
  "version": "1.0.0",
  "apiUrl": "https://api.tasso.dev.br",
  "otelEndpoint": "https://otel.tasso.dev.br/v1/traces"
}
```

## Critérios de Sucesso

- [x] Pacotes npm instalados sem conflitos
- [x] Telemetria inicializa apenas em produção (não em `npm run dev`)
- [x] Carregamento de página gera trace `documentLoad`
- [x] Chamadas Axios propagam headers `traceparent` e `tracestate`
- [ ] Traces do frontend aparecem no Grafana/Tempo
- [ ] É possível ver trace completo frontend → backend
- [x] Hook `useTracing` funciona para spans manuais
- [x] Erros JavaScript são capturados nos spans

## Sequenciamento

- **Bloqueado por:** 2.0, 3.0, 4.0 (pelo menos um backend instrumentado para teste E2E)
- **Desbloqueia:** 6.0 (Testes E2E)
- **Paralelizável:** Não (depende de backends para validação)

## Arquivos Afetados

| Arquivo | Ação | Descrição |
|---------|------|-----------|
| `frontend/package.json` | Modificar | Adicionar dependências npm |
| `frontend/src/telemetry/index.ts` | Criar | Módulo de inicialização |
| `frontend/src/telemetry/useTracing.ts` | Criar | Hook para spans manuais |
| `frontend/src/lib/api.ts` | Modificar | Interceptor de propagação |
| `frontend/src/main.tsx` | Modificar | Inicialização condicional |
| `frontend/public/app-config.json` | Modificar | Adicionar otelEndpoint |

## Considerações Especiais

### CORS

O endpoint `https://otel.tasso.dev.br` deve retornar headers CORS apropriados:

```
Access-Control-Allow-Origin: https://gestauto.tasso.dev.br
Access-Control-Allow-Methods: POST, OPTIONS
Access-Control-Allow-Headers: Content-Type, traceparent, tracestate
```

### Bundle Size

Os pacotes OpenTelemetry adicionam aproximadamente **50-80KB** (gzipped) ao bundle. Considerar:

- Lazy loading do módulo de telemetria
- Tree-shaking das instrumentações não utilizadas

### Debugging em Desenvolvimento

Para debug em desenvolvimento, criar flag opcional:

```typescript
// Forçar telemetria em dev (apenas para debug)
const FORCE_TELEMETRY = import.meta.env.VITE_FORCE_TELEMETRY === 'true';

if (import.meta.env.PROD || FORCE_TELEMETRY) {
  // inicializar telemetria
}
```

## Riscos e Mitigações

| Risco | Probabilidade | Mitigação |
|-------|---------------|-----------|
| CORS bloqueando envio | Média | Verificar configuração do Collector/Traefik na tarefa 1.0 |
| Overhead de bundle size | Baixa | Lazy loading do módulo de telemetria |
| ZoneContextManager incompatível | Baixa | Fallback para AsyncLocalStorageContextManager |
| Conflito com React 18 Concurrent | Baixa | Testar em StrictMode |

## Entregáveis

1. Módulo de telemetria (`src/telemetry/`)
2. Hook `useTracing` para uso manual
3. Interceptor Axios atualizado
4. Atualização do app-config.json
5. Screenshot de trace frontend→backend no Grafana
6. Testes para interceptor
