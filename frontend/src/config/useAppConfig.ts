import { useContext } from 'react'
import { AppConfigContext } from './appConfigState'
import type { FrontendConfig } from './types'

export function useAppConfigState() {
  return useContext(AppConfigContext)
}

export function useAppConfig(): FrontendConfig {
  const state = useAppConfigState()
  if (state.status === 'ready') return state.config
  throw new Error('Config ainda não carregada')
}
