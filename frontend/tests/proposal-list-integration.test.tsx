import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { MemoryRouter } from 'react-router-dom';

import { ProposalListPage } from '../src/modules/commercial/pages/ProposalListPage';
import * as useProposalsHook from '../src/modules/commercial/hooks/useProposals';
import * as useLeadsHook from '../src/modules/commercial/hooks/useLeads';

const toastSpy = vi.fn();
vi.mock('../src/hooks/use-toast', () => ({
  useToast: () => ({ toast: toastSpy }),
}));

describe('ProposalListPage Integration - COM-PROP-001', () => {
  let queryClient: QueryClient;

  beforeEach(() => {
    toastSpy.mockClear();

    queryClient = new QueryClient({
      defaultOptions: {
        queries: {
          retry: false,
        },
      },
    });

    vi.spyOn(useProposalsHook, 'useProposals').mockReturnValue({
      data: {
        items: [
          {
            id: '11111111-1111-1111-1111-111111111111',
            leadId: 'lead-1',
            status: 'Draft',
            vehicleModel: 'Civic Touring',
            totalValue: 123456.78,
            createdAt: '2025-01-15T10:00:00Z',
          },
        ],
        page: 1,
        pageSize: 20,
        totalCount: 1,
        totalPages: 1,
        hasNextPage: false,
        hasPreviousPage: false,
      },
      isLoading: false,
      isError: false,
    } as any);

    vi.spyOn(useLeadsHook, 'useLeads').mockReturnValue({
      data: {
        items: [
          {
            id: 'lead-1',
            name: 'João Silva',
            status: 'Qualified',
            salesPersonId: 'seller-1',
            createdAt: '2025-01-01T10:00:00Z',
          },
        ],
        page: 1,
        pageSize: 1000,
        totalCount: 1,
        totalPages: 1,
        hasNextPage: false,
        hasPreviousPage: false,
      },
      isLoading: false,
      isError: false,
    } as any);
  });

  const renderComponent = () =>
    render(
      <QueryClientProvider client={queryClient}>
        <MemoryRouter>
          <ProposalListPage />
        </MemoryRouter>
      </QueryClientProvider>
    );

  it('deve renderizar a tabela com colunas do PRD', async () => {
    renderComponent();

    await waitFor(() => {
      expect(screen.getByText('Nº Proposta')).toBeInTheDocument();
    });

    expect(screen.getByText('Cliente')).toBeInTheDocument();
    expect(screen.getByText('Veículo')).toBeInTheDocument();
    expect(screen.getByText('Valor Total')).toBeInTheDocument();
    expect(screen.getByText('Status')).toBeInTheDocument();
    expect(screen.getByText('Data')).toBeInTheDocument();
  });

  it('deve renderizar itens da lista com cliente, veículo, status e valor', async () => {
    renderComponent();

    await waitFor(() => {
      expect(screen.getByText('João Silva')).toBeInTheDocument();
    });

    expect(screen.getByText('Civic Touring')).toBeInTheDocument();
    expect(screen.getByText('Rascunho')).toBeInTheDocument();
    expect(
      screen.getByText((content) =>
        content.replace(/\u00a0/g, ' ').includes('R$ 123.456,78')
      )
    ).toBeInTheDocument();
  });

  it('deve disparar toaster quando houver erro ao carregar propostas', async () => {
    (useProposalsHook.useProposals as any).mockReturnValueOnce({
      data: undefined,
      isLoading: false,
      isError: true,
    });

    renderComponent();

    await waitFor(() => {
      expect(toastSpy).toHaveBeenCalledWith(
        expect.objectContaining({
          variant: 'destructive',
          title: 'Erro ao carregar propostas',
        })
      );
    });

    expect(screen.getByText('Erro ao carregar propostas.')).toBeInTheDocument();
  });
});
