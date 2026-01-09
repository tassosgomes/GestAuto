import type { Role } from './types'
import { useAuth } from './useAuth'
import { AccessDeniedPage } from '../pages/AccessDeniedPage'

export function RequireRoles(props: {
  roles: readonly Role[]
  children: React.ReactNode
}) {
  const authState = useAuth()

  if (authState.status !== 'ready' || !authState.session.isAuthenticated) {
    return <>{props.children}</>
  }

  const hasAnyRole = props.roles.some((role) => authState.session.roles.includes(role))
  if (!hasAnyRole) {
    return <AccessDeniedPage />
  }

  return <>{props.children}</>
}
