export type Role =
  | 'ADMIN'
  | 'MANAGER'
  | 'VIEWER'
  | 'SALES_PERSON'
  | 'SALES_MANAGER'
  | 'VEHICLE_EVALUATOR'
  | 'EVALUATION_MANAGER'

export interface UserSession {
  isAuthenticated: boolean
  username?: string
  roles: Role[]
  accessToken?: string
}

export interface AuthService {
  init(): Promise<void>
  login(): Promise<void>
  logout(): Promise<void>
  getSession(): UserSession
}
