import { describe, it, expect } from 'vitest'
import { matchRoutes, type RouteObject } from 'react-router-dom'
import { commercialRoutes } from '../src/modules/commercial/routes'

describe('Commercial routes - /commercial/leads/new', () => {
  it('deve casar com a rota estática leads/new (não com leads/:id)', () => {
    const routes: RouteObject[] = [{ path: '/', children: [commercialRoutes] }]

    const matches = matchRoutes(routes, '/commercial/leads/new')

    expect(matches).not.toBeNull()
    expect(matches?.at(-1)?.route.path).toBe('leads/new')
  })
})
