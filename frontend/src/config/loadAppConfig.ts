import type { FrontendConfig } from './types'

type UnknownRecord = Record<string, unknown>

function isRecord(value: unknown): value is UnknownRecord {
  return typeof value === 'object' && value !== null
}

function asString(value: unknown, field: string): string {
  if (typeof value !== 'string' || value.trim().length === 0) {
    throw new Error(`Config inválida: campo '${field}' ausente ou vazio`)
  }
  return value
}

function asOptionalString(value: unknown): string | undefined {
  if (typeof value !== 'string') return undefined
  const trimmed = value.trim()
  return trimmed.length > 0 ? trimmed : undefined
}

function asRealm(value: unknown): FrontendConfig['keycloakRealm'] {
  if (value === 'gestauto-dev' || value === 'gestauto-hml' || value === 'gestauto') return value
  throw new Error("Config inválida: 'keycloakRealm' deve ser gestauto-dev | gestauto-hml | gestauto")
}

export async function loadAppConfig(): Promise<FrontendConfig> {
  const win = window as unknown as { __APP_CONFIG__?: unknown }
  if (win.__APP_CONFIG__) {
    return normalizeConfig(win.__APP_CONFIG__)
  }

  const res = await fetch('/app-config.json', { cache: 'no-store' })
  if (!res.ok) {
    throw new Error(`Falha ao carregar /app-config.json (HTTP ${res.status})`)
  }

  const json = (await res.json()) as unknown
  return normalizeConfig(json)
}

export function normalizeConfig(raw: unknown): FrontendConfig {
  if (!isRecord(raw)) {
    throw new Error('Config inválida: esperado objeto')
  }

  return {
    keycloakBaseUrl: asString(raw.keycloakBaseUrl, 'keycloakBaseUrl'),
    keycloakRealm: asRealm(raw.keycloakRealm),
    keycloakClientId: asString(raw.keycloakClientId, 'keycloakClientId'),
    commercialApiBaseUrl: asString(raw.commercialApiBaseUrl, 'commercialApiBaseUrl'),
    stockApiBaseUrl: asString(raw.stockApiBaseUrl, 'stockApiBaseUrl'),
    vehicleEvaluationApiBaseUrl: asString(
      raw.vehicleEvaluationApiBaseUrl,
      'vehicleEvaluationApiBaseUrl'
    ),
    version: asOptionalString(raw.version),
    otelEndpoint: asOptionalString(raw.otelEndpoint),
  }
}
