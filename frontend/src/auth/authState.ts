import { createContext } from 'react'
import type { AuthService, UserSession } from './types'

type AuthState =
  | { status: 'loading' }
  | { status: 'ready'; auth: AuthService; session: UserSession }
  | { status: 'error'; error: Error }

export type { AuthState }

export const AuthContext = createContext<AuthState>({ status: 'loading' })
