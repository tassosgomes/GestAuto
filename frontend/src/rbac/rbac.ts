import type { Role } from '../auth/types'

export type AppMenu = 'COMMERCIAL' | 'EVALUATIONS' | 'ADMIN'

export function getVisibleMenus(roles: readonly Role[]): AppMenu[] {
  const hasAny = (...required: Role[]) => required.some((r) => roles.includes(r))

  const menus: AppMenu[] = []

  if (hasAny('SALES_PERSON', 'SALES_MANAGER', 'MANAGER', 'ADMIN')) {
    menus.push('COMMERCIAL')
  }

  if (hasAny('VEHICLE_EVALUATOR', 'EVALUATION_MANAGER', 'MANAGER', 'VIEWER', 'ADMIN')) {
    menus.push('EVALUATIONS')
  }

  if (hasAny('ADMIN')) {
    menus.push('ADMIN')
  }

  return menus
}

export function canAccessMenu(roles: readonly Role[], menu: AppMenu): boolean {
  return getVisibleMenus(roles).includes(menu)
}
