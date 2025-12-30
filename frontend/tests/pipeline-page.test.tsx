import { render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { describe, expect, it, vi } from 'vitest';
import { PipelinePage } from '../src/modules/commercial/pages/PipelinePage';

vi.mock('../src/auth/useAuth', () => ({
  useAuth: vi.fn(),
}));

vi.mock('../src/modules/commercial/hooks/useLeads', () => ({
  useLeads: vi.fn(),
}));

import * as useAuthModule from '../src/auth/useAuth';
import * as useLeadsModule from '../src/modules/commercial/hooks/useLeads';

describe('PipelinePage', () => {
  it('bloqueia acesso para SALES_PERSON', () => {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    (useAuthModule.useAuth as any).mockReturnValue({
      status: 'ready',
      session: {
        isAuthenticated: true,
        roles: ['SALES_PERSON'],
      },
    });

    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    (useLeadsModule.useLeads as any).mockReturnValue({
      data: { items: [] },
      isLoading: false,
      isError: false,
    });

    render(
      <MemoryRouter>
        <PipelinePage />
      </MemoryRouter>
    );

    expect(screen.getByText('Acesso negado')).toBeInTheDocument();
  });

  it('renderiza kanban e filtro para MANAGER', () => {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    (useAuthModule.useAuth as any).mockReturnValue({
      status: 'ready',
      session: {
        isAuthenticated: true,
        roles: ['MANAGER'],
      },
    });

    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    (useLeadsModule.useLeads as any).mockReturnValue({
      data: {
        items: [
          {
            id: 'lead-1',
            name: 'Cliente A',
            status: 'InNegotiation',
            score: 'Gold',
            salesPersonId: 'seller-1',
            interestedModel: 'Civic',
            createdAt: new Date().toISOString(),
          },
          {
            id: 'lead-2',
            name: 'Cliente B',
            status: 'New',
            score: 'Diamond',
            salesPersonId: 'seller-2',
            interestedModel: 'Corolla',
            createdAt: new Date().toISOString(),
          },
        ],
      },
      isLoading: false,
      isError: false,
    });

    render(
      <MemoryRouter>
        <PipelinePage />
      </MemoryRouter>
    );

    expect(screen.getByText('Pipeline')).toBeInTheDocument();
    expect(screen.getByText('Todos os vendedores')).toBeInTheDocument();

    // Column labels (pt-BR)
    expect(screen.getAllByText('Em Negociação').length).toBeGreaterThan(0);
    expect(screen.getAllByText('Novo').length).toBeGreaterThan(0);

    // Lead cards
    expect(screen.getByText('Cliente A')).toBeInTheDocument();
    expect(screen.getByText('Cliente B')).toBeInTheDocument();
  });
});
