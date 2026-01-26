import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

const mockInject = vi.fn((_: unknown, carrier: Record<string, string>) => {
  carrier.traceparent = '00-test-traceparent'
  carrier.tracestate = 'test-tracestate'
})

vi.mock('@opentelemetry/api', () => ({
  context: { active: vi.fn(() => ({})) },
  propagation: { inject: mockInject },
  trace: { getActiveSpan: vi.fn(() => null) },
  SpanStatusCode: { ERROR: 2, OK: 1 },
}))

describe('axios telemetry interceptor', () => {
  beforeEach(() => {
    vi.resetModules()
    mockInject.mockClear()
  })

  afterEach(() => {
    vi.unstubAllEnvs()
  })

  it('injects W3C headers when telemetry is enabled', async () => {
    vi.stubEnv('VITE_FORCE_TELEMETRY', 'true')

    const { commercialApi, setTokenGetter } = await import('../src/lib/api')
    setTokenGetter(() => 'token-123')

    const handler = (commercialApi.interceptors.request as any).handlers?.[0]?.fulfilled
    expect(handler).toBeTypeOf('function')

    const config = await handler({ headers: {} })

    expect(config.headers.Authorization).toBe('Bearer token-123')
    expect(config.headers.traceparent).toBe('00-test-traceparent')
    expect(config.headers.tracestate).toBe('test-tracestate')
    expect(mockInject).toHaveBeenCalledOnce()
  })

  it('does not inject W3C headers when telemetry is disabled', async () => {
    vi.stubEnv('VITE_FORCE_TELEMETRY', '')

    const { commercialApi } = await import('../src/lib/api')
    const handler = (commercialApi.interceptors.request as any).handlers?.[0]?.fulfilled
    expect(handler).toBeTypeOf('function')

    const config = await handler({ headers: {} })

    expect(config.headers.traceparent).toBeUndefined()
    expect(config.headers.tracestate).toBeUndefined()
    expect(mockInject).not.toHaveBeenCalled()
  })
})
