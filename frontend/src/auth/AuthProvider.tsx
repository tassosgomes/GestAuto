import { useEffect, useMemo, useState } from 'react'
import type { FrontendConfig } from '../config/types'
import { createKeycloakAuthService } from './keycloakAuthService'
import { AuthContext, type AuthState } from './authState'
import { setTokenGetter } from '../lib/api'

export function AuthProvider(props: { config: FrontendConfig; children: React.ReactNode }) {
  const [state, setState] = useState<AuthState>({ status: 'loading' })

  useEffect(() => {
    let cancelled = false

    void (async () => {
      try {
        const auth = createKeycloakAuthService(props.config)
        await auth.init()
        if (cancelled) return
        setTokenGetter(() => auth.getSession().accessToken)
        setState({ status: 'ready', auth, session: auth.getSession() })
      } catch (e) {
        const err = e instanceof Error ? e : new Error(String(e))
        if (!cancelled) setState({ status: 'error', error: err })
      }
    })()

    return () => {
      cancelled = true
    }
  }, [props.config])

  const value = useMemo(() => state, [state])

  if (state.status === 'loading') {
    return (
      <AuthContext.Provider value={value}>
        <div style={{ padding: 16 }}>Inicializando autenticação…</div>
      </AuthContext.Provider>
    )
  }

  if (state.status === 'error') {
    return (
      <AuthContext.Provider value={value}>
        <div style={{ padding: 16 }}>
          <h1>Erro de autenticação</h1>
          <pre style={{ whiteSpace: 'pre-wrap' }}>{state.error.message}</pre>
        </div>
      </AuthContext.Provider>
    )
  }

  return <AuthContext.Provider value={value}>{props.children}</AuthContext.Provider>
}
