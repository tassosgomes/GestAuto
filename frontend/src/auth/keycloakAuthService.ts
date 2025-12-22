import Keycloak from 'keycloak-js'
import type { FrontendConfig } from '../config/types'
import type { AuthService, Role, UserSession } from './types'

type TokenParsed = Record<string, unknown>

type KeycloakLike = {
  authenticated?: boolean
  token?: string
  tokenParsed?: TokenParsed
  idTokenParsed?: TokenParsed
  subject?: string
  login: (opts?: Record<string, unknown>) => Promise<void>
  logout: (opts?: Record<string, unknown>) => Promise<void>
  init: (opts: Record<string, unknown>) => Promise<boolean>
  updateToken: (minValidity: number) => Promise<boolean>
  onTokenExpired?: () => void
}

function toRoles(value: unknown): Role[] {
  if (!Array.isArray(value)) return []
  return value.filter((v): v is Role => typeof v === 'string') as Role[]
}

function extractRoles(parsed?: TokenParsed): Role[] {
  // Prefer custom claim "roles" provisioned by Keycloak protocol mapper.
  const roles = parsed?.roles
  const fromCustom = toRoles(roles)
  if (fromCustom.length > 0) return fromCustom

  // Fallback for Keycloak defaults (realm_access.roles) if needed.
  const realmAccess = parsed?.realm_access
  if (typeof realmAccess === 'object' && realmAccess !== null) {
    const ra = realmAccess as Record<string, unknown>
    return toRoles(ra.roles)
  }

  return []
}

function extractUsername(parsed?: TokenParsed): string | undefined {
  const preferred = parsed?.preferred_username
  if (typeof preferred === 'string' && preferred.trim().length > 0) return preferred
  const sub = parsed?.sub
  if (typeof sub === 'string' && sub.trim().length > 0) return sub
  return undefined
}

export function createKeycloakAuthService(config: FrontendConfig): AuthService {
  const kc: KeycloakLike = new (Keycloak as unknown as new (opts: unknown) => KeycloakLike)({
    url: config.keycloakBaseUrl,
    realm: config.keycloakRealm,
    clientId: config.keycloakClientId,
  })

  let session: UserSession = { isAuthenticated: false, roles: [] }

  const normalizeIsAuthenticated = (authenticated?: boolean) => {
    if (authenticated === true) return true
    if (authenticated === false) return false
    if (kc.authenticated === true) return true
    // Keycloak-js pode não preencher kc.authenticated consistentemente; token é a fonte de verdade.
    return typeof kc.token === 'string' && kc.token.length > 0
  }

  const syncSession = (authenticated?: boolean) => {
    const parsed = kc.tokenParsed
    const isAuthenticated = normalizeIsAuthenticated(authenticated)
    session = {
      isAuthenticated,
      username: extractUsername(parsed),
      roles: extractRoles(parsed),
      // Não persistimos token. Mantemos em memória e apenas se autenticado.
      accessToken: isAuthenticated ? kc.token : undefined,
    }
  }

  const clearOAuthCallbackFromUrlIfPresent = () => {
    // Quando o Keycloak retorna, costuma enviar code/state no fragment (#...)
    // e isso pode causar reprocessamento em recarregamentos, gerando loops.
    const hash = window.location.hash
    if (!hash) return

    const looksLikeOAuthCallback =
      hash.includes('code=') || hash.includes('state=') || hash.includes('session_state=') || hash.includes('error=')

    if (!looksLikeOAuthCallback) return

    window.history.replaceState(null, document.title, window.location.pathname + window.location.search)
  }

  kc.onTokenExpired = () => {
    void (async () => {
      try {
        // Tenta renovar silenciosamente; se falhar, redireciona para login.
        const refreshed = await kc.updateToken(30)
        void refreshed
        syncSession(true)
      } catch {
        session = { isAuthenticated: false, roles: [] }
        await kc.login({ redirectUri: window.location.href })
      }
    })()
  }

  return {
    async init() {
      // Em ambientes HTTP locais, o fluxo de check-sso pode falhar/travar
      // devido a checagens de 3rd-party cookies/secure context.
      // Para o MVP, inicializamos sem check-sso e deixamos o login explícito.
      const authenticated = await kc.init({
        pkceMethod: 'S256',
        checkLoginIframe: false,
      })
      syncSession(authenticated)
      clearOAuthCallbackFromUrlIfPresent()
    },

    async login() {
      await kc.login({ redirectUri: window.location.href })
    },

    async logout() {
      // Volta para /login (rota não-protegida) após logout.
      await kc.logout({ redirectUri: `${window.location.origin}/login` })
    },

    getSession() {
      return session
    },
  }
}
