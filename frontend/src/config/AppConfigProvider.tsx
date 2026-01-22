import { useContext, useEffect, useMemo, useState } from 'react'
import { loadAppConfig } from './loadAppConfig'
import { AppConfigContext, type AppConfigState } from './appConfigState'
import {
  setCommercialApiBaseUrl,
  setStockApiBaseUrl,
  setVehicleEvaluationApiBaseUrl,
} from '../lib/api'

export function AppConfigProvider(props: { children: React.ReactNode }) {
  const [state, setState] = useState<AppConfigState>({ status: 'loading' })

  useEffect(() => {
    let cancelled = false

    void (async () => {
      try {
        const config = await loadAppConfig()
        if (!cancelled) {
          setCommercialApiBaseUrl(config.commercialApiBaseUrl)
          setStockApiBaseUrl(config.stockApiBaseUrl)
          setVehicleEvaluationApiBaseUrl(config.vehicleEvaluationApiBaseUrl)
          setState({ status: 'ready', config })
        }
      } catch (e) {
        const err = e instanceof Error ? e : new Error(String(e))
        if (!cancelled) setState({ status: 'error', error: err })
      }
    })()

    return () => {
      cancelled = true
    }
  }, [])

  const value = useMemo(() => state, [state])
  return <AppConfigContext.Provider value={value}>{props.children}</AppConfigContext.Provider>
}

export function AppConfigGate(props: { children: React.ReactNode }) {
  const ctx = useContext(AppConfigContext)

  if (ctx.status === 'loading') {
    return <div style={{ padding: 16 }}>Carregando configuração…</div>
  }

  if (ctx.status === 'error') {
    return (
      <div style={{ padding: 16 }}>
        <h1>Erro ao iniciar</h1>
        <p>Falha ao carregar configuração de runtime.</p>
        <pre style={{ whiteSpace: 'pre-wrap' }}>{ctx.error.message}</pre>
      </div>
    )
  }

  return <>{props.children}</>
}
