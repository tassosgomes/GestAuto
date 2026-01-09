import { describe, expect, it, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { RequireRoles } from '../src/auth/RequireRoles'
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

describe('RequireRoles', () => {
  it('bloqueia quando não possui role requerida', () => {
    mockAuth(['SALES_PERSON'])

    render(
      <MemoryRouter>
        <RequireRoles roles={['MANAGER', 'ADMIN', 'SALES_MANAGER'] as any}>
          <div>Conteúdo restrito</div>
        </RequireRoles>
      </MemoryRouter>
    )

    expect(screen.getByText('Acesso negado')).toBeInTheDocument()
    expect(screen.queryByText('Conteúdo restrito')).not.toBeInTheDocument()
  })

  it('permite quando possui ao menos uma role requerida', () => {
    mockAuth(['MANAGER'])

    render(
      <MemoryRouter>
        <RequireRoles roles={['MANAGER', 'ADMIN'] as any}>
          <div>Conteúdo restrito</div>
        </RequireRoles>
      </MemoryRouter>
    )

    expect(screen.getByText('Conteúdo restrito')).toBeInTheDocument()
    expect(screen.queryByText('Acesso negado')).not.toBeInTheDocument()
  })
})
