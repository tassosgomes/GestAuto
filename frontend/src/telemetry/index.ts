import { WebTracerProvider } from '@opentelemetry/sdk-trace-web'
import { BatchSpanProcessor } from '@opentelemetry/sdk-trace-base'
import { OTLPTraceExporter } from '@opentelemetry/exporter-trace-otlp-http'
import { ZoneContextManager } from '@opentelemetry/context-zone'
import { registerInstrumentations } from '@opentelemetry/instrumentation'
import { getWebAutoInstrumentations } from '@opentelemetry/auto-instrumentations-web'
import { Resource } from '@opentelemetry/resources'
import { ATTR_SERVICE_NAME, ATTR_SERVICE_VERSION } from '@opentelemetry/semantic-conventions'
import { context, trace, SpanStatusCode } from '@opentelemetry/api'

let isInitialized = false

export interface TelemetryConfig {
  serviceName: string
  serviceVersion: string
  otlpEndpoint: string
}

function registerGlobalErrorHandlers() {
  window.addEventListener('error', (event) => {
    const tracer = trace.getTracer('frontend')
    const span = tracer.startSpan('window.error')
    if (event.error instanceof Error) {
      span.recordException(event.error)
      span.setStatus({ code: SpanStatusCode.ERROR, message: event.error.message })
    } else {
      span.recordException({ name: 'ErrorEvent', message: event.message })
      span.setStatus({ code: SpanStatusCode.ERROR, message: event.message })
    }
    span.end()
  })

  window.addEventListener('unhandledrejection', (event) => {
    const tracer = trace.getTracer('frontend')
    const span = tracer.startSpan('window.unhandledrejection')
    const reason = event.reason
    if (reason instanceof Error) {
      span.recordException(reason)
      span.setStatus({ code: SpanStatusCode.ERROR, message: reason.message })
    } else {
      span.recordException({ name: 'UnhandledRejection', message: String(reason) })
      span.setStatus({ code: SpanStatusCode.ERROR, message: String(reason) })
    }
    span.end()
  })
}

export function initTelemetry(config: TelemetryConfig): void {
  if (isInitialized) {
    console.warn('Telemetry already initialized')
    return
  }

  const resource = new Resource({
    [ATTR_SERVICE_NAME]: config.serviceName,
    [ATTR_SERVICE_VERSION]: config.serviceVersion,
  })

  const provider = new WebTracerProvider({ resource })

  const exporter = new OTLPTraceExporter({
    url: config.otlpEndpoint,
    headers: {},
  })

  provider.addSpanProcessor(
    new BatchSpanProcessor(exporter, {
      maxQueueSize: 100,
      maxExportBatchSize: 50,
      scheduledDelayMillis: 5000,
      exportTimeoutMillis: 5000,
    })
  )

  provider.register({
    contextManager: new ZoneContextManager(),
  })

  registerInstrumentations({
    instrumentations: [
      getWebAutoInstrumentations({
        '@opentelemetry/instrumentation-document-load': {
          enabled: true,
        },
        '@opentelemetry/instrumentation-fetch': {
          enabled: true,
          propagateTraceHeaderCorsUrls: [/https:\/\/.*\.tasso\.dev\.br/, /http:\/\/localhost:\d+/],
          clearTimingResources: true,
        },
        '@opentelemetry/instrumentation-user-interaction': {
          enabled: true,
          eventNames: ['click', 'submit'],
        },
        '@opentelemetry/instrumentation-xml-http-request': {
          enabled: false,
        },
      }),
    ],
  })

  registerGlobalErrorHandlers()

  isInitialized = true
  console.info('OpenTelemetry initialized for frontend')
}

export function getTracer(name: string = 'frontend') {
  return trace.getTracer(name)
}

export function getCurrentContext() {
  return context.active()
}

export { trace, context, SpanStatusCode }
