import type { Role } from '../auth/types'

export type AppMenu = 'COMMERCIAL' | 'EVALUATIONS' | 'ADMIN'

export function getVisibleMenus(roles: readonly Role[]): AppMenu[] {
  const isAdmin = roles.includes('ADMIN')
  if (isAdmin) return ['COMMERCIAL', 'EVALUATIONS', 'ADMIN']

  const isManager = roles.includes('MANAGER')
  if (isManager) return ['COMMERCIAL', 'EVALUATIONS']

  const isSales = roles.includes('SALES_PERSON') || roles.includes('SALES_MANAGER')
  if (isSales) return ['COMMERCIAL']

  const isEvaluations =
    roles.includes('VEHICLE_EVALUATOR') || roles.includes('EVALUATION_MANAGER') || roles.includes('VIEWER')
  if (isEvaluations) return ['EVALUATIONS']

  return []
}

export function canAccessMenu(roles: readonly Role[], menu: AppMenu): boolean {
  return getVisibleMenus(roles).includes(menu)
}
