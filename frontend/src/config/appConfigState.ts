import { createContext } from 'react'
import type { FrontendConfig } from './types'

type AppConfigState =
  | { status: 'loading' }
  | { status: 'ready'; config: FrontendConfig }
  | { status: 'error'; error: Error }

export type { AppConfigState }

export const AppConfigContext = createContext<AppConfigState>({ status: 'loading' })
