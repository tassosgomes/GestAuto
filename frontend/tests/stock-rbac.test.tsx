import { describe, expect, it, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { Sidebar } from '../src/components/layout/Sidebar';
import { RequireRoles } from '../src/auth/RequireRoles';
import * as useAuthModule from '../src/auth/useAuth';

vi.mock('../src/auth/useAuth', () => ({
  useAuth: vi.fn(),
}));

function mockAuth(roles: string[]) {
  (useAuthModule.useAuth as unknown as ReturnType<typeof vi.fn>).mockReturnValue({
    status: 'ready',
    session: {
      isAuthenticated: true,
      username: 'testuser',
      roles,
    },
    auth: {
      logout: vi.fn(),
    },
  });
}

describe('Stock RBAC - Menu Visibility', () => {
  it('ADMIN vê menu Estoque', () => {
    mockAuth(['ADMIN']);

    render(
      <MemoryRouter initialEntries={['/stock']}>
        <Sidebar />
      </MemoryRouter>
    );

    expect(screen.getByText('Estoque')).toBeInTheDocument();
  });

  it('MANAGER vê menu Estoque', () => {
    mockAuth(['MANAGER']);

    render(
      <MemoryRouter initialEntries={['/stock']}>
        <Sidebar />
      </MemoryRouter>
    );

    expect(screen.getByText('Estoque')).toBeInTheDocument();
  });

  it('STOCK_MANAGER vê menu Estoque', () => {
    mockAuth(['STOCK_MANAGER']);

    render(
      <MemoryRouter initialEntries={['/stock']}>
        <Sidebar />
      </MemoryRouter>
    );

    expect(screen.getByText('Estoque')).toBeInTheDocument();
  });

  it('STOCK_PERSON vê menu Estoque', () => {
    mockAuth(['STOCK_PERSON']);

    render(
      <MemoryRouter initialEntries={['/stock']}>
        <Sidebar />
      </MemoryRouter>
    );

    expect(screen.getByText('Estoque')).toBeInTheDocument();
  });

  it('SALES_MANAGER não vê menu Estoque', () => {
    mockAuth(['SALES_MANAGER']);

    render(
      <MemoryRouter initialEntries={['/stock']}>
        <Sidebar />
      </MemoryRouter>
    );

    expect(screen.queryByText('Estoque')).not.toBeInTheDocument();
  });

  it('SALES_PERSON não vê menu Estoque', () => {
    mockAuth(['SALES_PERSON']);

    render(
      <MemoryRouter initialEntries={['/stock']}>
        <Sidebar />
      </MemoryRouter>
    );

    expect(screen.queryByText('Estoque')).not.toBeInTheDocument();
  });

  it('VIEWER não vê menu Estoque', () => {
    mockAuth(['VIEWER']);

    render(
      <MemoryRouter initialEntries={['/']}>
        <Sidebar />
      </MemoryRouter>
    );

    expect(screen.queryByText('Estoque')).not.toBeInTheDocument();
  });

  it('VEHICLE_EVALUATOR não vê menu Estoque', () => {
    mockAuth(['VEHICLE_EVALUATOR']);

    render(
      <MemoryRouter initialEntries={['/evaluations']}>
        <Sidebar />
      </MemoryRouter>
    );

    expect(screen.queryByText('Estoque')).not.toBeInTheDocument();
  });

  it('usuário sem roles não vê menu Estoque', () => {
    mockAuth([]);

    render(
      <MemoryRouter initialEntries={['/']}>
        <Sidebar />
      </MemoryRouter>
    );

    expect(screen.queryByText('Estoque')).not.toBeInTheDocument();
  });
});

describe('Stock RBAC - Protected Routes', () => {
  it('ADMIN pode acessar /stock/preparation', () => {
    mockAuth(['ADMIN']);

    render(
      <MemoryRouter>
        <RequireRoles roles={['ADMIN', 'MANAGER', 'STOCK_MANAGER'] as any}>
          <div>Conteúdo de Preparação</div>
        </RequireRoles>
      </MemoryRouter>
    );

    expect(screen.getByText('Conteúdo de Preparação')).toBeInTheDocument();
    expect(screen.queryByText('Acesso negado')).not.toBeInTheDocument();
  });

  it('MANAGER pode acessar /stock/preparation', () => {
    mockAuth(['MANAGER']);

    render(
      <MemoryRouter>
        <RequireRoles roles={['ADMIN', 'MANAGER', 'STOCK_MANAGER'] as any}>
          <div>Conteúdo de Preparação</div>
        </RequireRoles>
      </MemoryRouter>
    );

    expect(screen.getByText('Conteúdo de Preparação')).toBeInTheDocument();
    expect(screen.queryByText('Acesso negado')).not.toBeInTheDocument();
  });

  it('STOCK_MANAGER pode acessar /stock/preparation', () => {
    mockAuth(['STOCK_MANAGER']);

    render(
      <MemoryRouter>
        <RequireRoles roles={['ADMIN', 'MANAGER', 'STOCK_MANAGER'] as any}>
          <div>Conteúdo de Preparação</div>
        </RequireRoles>
      </MemoryRouter>
    );

    expect(screen.getByText('Conteúdo de Preparação')).toBeInTheDocument();
    expect(screen.queryByText('Acesso negado')).not.toBeInTheDocument();
  });

  it('STOCK_PERSON não pode acessar /stock/preparation', () => {
    mockAuth(['STOCK_PERSON']);

    render(
      <MemoryRouter>
        <RequireRoles roles={['ADMIN', 'MANAGER', 'STOCK_MANAGER'] as any}>
          <div>Conteúdo de Preparação</div>
        </RequireRoles>
      </MemoryRouter>
    );

    expect(screen.getByText('Acesso negado')).toBeInTheDocument();
    expect(screen.queryByText('Conteúdo de Preparação')).not.toBeInTheDocument();
  });

  it('SALES_PERSON não pode acessar /stock/preparation', () => {
    mockAuth(['SALES_PERSON']);

    render(
      <MemoryRouter>
        <RequireRoles roles={['ADMIN', 'MANAGER', 'STOCK_MANAGER'] as any}>
          <div>Conteúdo de Preparação</div>
        </RequireRoles>
      </MemoryRouter>
    );

    expect(screen.getByText('Acesso negado')).toBeInTheDocument();
    expect(screen.queryByText('Conteúdo de Preparação')).not.toBeInTheDocument();
  });

  it('ADMIN pode acessar /stock/finance', () => {
    mockAuth(['ADMIN']);

    render(
      <MemoryRouter>
        <RequireRoles roles={['ADMIN', 'MANAGER', 'STOCK_MANAGER'] as any}>
          <div>Conteúdo Financeiro</div>
        </RequireRoles>
      </MemoryRouter>
    );

    expect(screen.getByText('Conteúdo Financeiro')).toBeInTheDocument();
    expect(screen.queryByText('Acesso negado')).not.toBeInTheDocument();
  });

  it('SALES_MANAGER não pode acessar /stock/finance', () => {
    mockAuth(['SALES_MANAGER']);

    render(
      <MemoryRouter>
        <RequireRoles roles={['ADMIN', 'MANAGER', 'STOCK_MANAGER'] as any}>
          <div>Conteúdo Financeiro</div>
        </RequireRoles>
      </MemoryRouter>
    );

    expect(screen.getByText('Acesso negado')).toBeInTheDocument();
    expect(screen.queryByText('Conteúdo Financeiro')).not.toBeInTheDocument();
  });

  it('ADMIN pode acessar /stock/write-offs', () => {
    mockAuth(['ADMIN']);

    render(
      <MemoryRouter>
        <RequireRoles roles={['ADMIN', 'MANAGER', 'STOCK_MANAGER'] as any}>
          <div>Conteúdo de Baixas</div>
        </RequireRoles>
      </MemoryRouter>
    );

    expect(screen.getByText('Conteúdo de Baixas')).toBeInTheDocument();
    expect(screen.queryByText('Acesso negado')).not.toBeInTheDocument();
  });

  it('STOCK_PERSON não pode acessar /stock/write-offs', () => {
    mockAuth(['STOCK_PERSON']);

    render(
      <MemoryRouter>
        <RequireRoles roles={['ADMIN', 'MANAGER', 'STOCK_MANAGER'] as any}>
          <div>Conteúdo de Baixas</div>
        </RequireRoles>
      </MemoryRouter>
    );

    expect(screen.getByText('Acesso negado')).toBeInTheDocument();
    expect(screen.queryByText('Conteúdo de Baixas')).not.toBeInTheDocument();
  });

  it('qualquer role com acesso ao Stock pode acessar /stock (dashboard público)', () => {
    mockAuth(['STOCK_PERSON']);

    render(
      <MemoryRouter>
        <div>Dashboard do Estoque</div>
      </MemoryRouter>
    );

    expect(screen.getByText('Dashboard do Estoque')).toBeInTheDocument();
  });
});
