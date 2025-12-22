import { describe, expect, it } from 'vitest'
import { canAccessMenu, getVisibleMenus, type AppMenu } from '../src/rbac/rbac'
import type { Role } from '../src/auth/types'

function menusOf(roles: Role[]): AppMenu[] {
  return getVisibleMenus(roles)
}

describe('RBAC - getVisibleMenus', () => {
  it('SALES_PERSON vê Comercial', () => {
    expect(menusOf(['SALES_PERSON'])).toEqual(['COMMERCIAL'])
  })

  it('VIEWER vê somente Avaliações', () => {
    expect(menusOf(['VIEWER'])).toEqual(['EVALUATIONS'])
  })

  it('MANAGER vê Comercial e Avaliações', () => {
    expect(menusOf(['MANAGER'])).toEqual(['COMMERCIAL', 'EVALUATIONS'])
  })

  it('ADMIN vê todos', () => {
    expect(menusOf(['ADMIN'])).toEqual(['COMMERCIAL', 'EVALUATIONS', 'ADMIN'])
  })

  it('Sem roles não vê menus', () => {
    expect(menusOf([])).toEqual([])
  })
})

describe('RBAC - canAccessMenu', () => {
  it('bloqueia menu ADMIN quando não é ADMIN', () => {
    expect(canAccessMenu(['MANAGER'], 'ADMIN')).toBe(false)
  })

  it('permite EVALUATIONS para VIEWER', () => {
    expect(canAccessMenu(['VIEWER'], 'EVALUATIONS')).toBe(true)
  })
})
