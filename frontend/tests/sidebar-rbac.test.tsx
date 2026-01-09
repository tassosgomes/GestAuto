import { describe, expect, it, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { Sidebar } from '../src/components/layout/Sidebar'
import * as useAuthModule from '../src/auth/useAuth'

vi.mock('../src/auth/useAuth', () => ({
  useAuth: vi.fn(),
}))

function mockAuth(roles: string[]) {
  ;(useAuthModule.useAuth as unknown as ReturnType<typeof vi.fn>).mockReturnValue({
    status: 'ready',
    session: {
      isAuthenticated: true,
      username: 'testuser',
      roles,
    },
    auth: {
      logout: vi.fn(),
    },
  })
}

describe('Sidebar RBAC (navItems.permissions)', () => {
  it('SALES_PERSON vê Comercial e não vê Aprovações/Pipeline', () => {
    mockAuth(['SALES_PERSON'])

    render(
      <MemoryRouter initialEntries={['/commercial']}>
        <Sidebar />
      </MemoryRouter>
    )

    expect(screen.getByText('Home')).toBeInTheDocument()
    expect(screen.getByText('Comercial')).toBeInTheDocument()
    expect(screen.queryByText('Avaliações')).not.toBeInTheDocument()
    expect(screen.queryByText('Admin')).not.toBeInTheDocument()
    expect(screen.queryByText('Configurações')).not.toBeInTheDocument()

    // submenu deve estar aberto por estar em /commercial
    expect(screen.getByText('Leads')).toBeInTheDocument()
    expect(screen.getByText('Propostas')).toBeInTheDocument()
    expect(screen.getByText('Test-Drives')).toBeInTheDocument()
    expect(screen.queryByText('Aprovações')).not.toBeInTheDocument()
    expect(screen.queryByText('Pipeline')).not.toBeInTheDocument()
  })

  it('VIEWER não vê Comercial e vê Avaliações', () => {
    mockAuth(['VIEWER'])

    render(
      <MemoryRouter initialEntries={['/']}>
        <Sidebar />
      </MemoryRouter>
    )

    expect(screen.getByText('Home')).toBeInTheDocument()
    expect(screen.getByText('Avaliações')).toBeInTheDocument()
    expect(screen.queryByText('Comercial')).not.toBeInTheDocument()
    expect(screen.queryByText('Admin')).not.toBeInTheDocument()
  })

  it('VEHICLE_EVALUATOR não vê Comercial e vê Avaliações', () => {
    mockAuth(['VEHICLE_EVALUATOR'])

    render(
      <MemoryRouter initialEntries={['/evaluations']}>
        <Sidebar />
      </MemoryRouter>
    )

    expect(screen.getByText('Avaliações')).toBeInTheDocument()
    expect(screen.queryByText('Comercial')).not.toBeInTheDocument()
  })
})
