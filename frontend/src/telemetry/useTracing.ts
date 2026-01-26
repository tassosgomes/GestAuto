import { useCallback } from 'react'
import { trace, context, SpanStatusCode, type Span } from '@opentelemetry/api'

const tracer = trace.getTracer('frontend-hooks')

interface UseTracingOptions {
  spanName: string
  attributes?: Record<string, string | number | boolean>
}

export function useTracing() {
  const startSpan = useCallback((options: UseTracingOptions): Span => {
    return tracer.startSpan(options.spanName, {
      attributes: options.attributes,
    })
  }, [])

  const withSpan = useCallback(
    async <T,>(options: UseTracingOptions, fn: () => Promise<T>): Promise<T> => {
      const span = startSpan(options)
      try {
        const result = await context.with(trace.setSpan(context.active(), span), fn)
        span.setStatus({ code: SpanStatusCode.OK })
        return result
      } catch (error) {
        const message = error instanceof Error ? error.message : 'Unknown error'
        span.setStatus({ code: SpanStatusCode.ERROR, message })
        if (error instanceof Error) {
          span.recordException(error)
        }
        throw error
      } finally {
        span.end()
      }
    },
    [startSpan]
  )

  return { startSpan, withSpan }
}
