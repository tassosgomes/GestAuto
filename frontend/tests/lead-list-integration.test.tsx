import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { MemoryRouter } from 'react-router-dom';
import { LeadListPage } from '../src/modules/commercial/pages/LeadListPage';
import * as useLeadsHook from '../src/modules/commercial/hooks/useLeads';

const mockLeadsData = {
  items: [
    {
      id: 'lead-1',
      name: 'João Silva',
      email: 'joao@example.com',
      phone: '(11) 98765-4321',
      source: 'Website',
      status: 'Qualified',
      score: 'Diamond',
      salesPersonId: 'seller-1',
      interestedModel: 'Modelo X',
      createdAt: '2025-01-15T10:00:00Z',
    },
    {
      id: 'lead-2',
      name: 'Maria Santos',
      email: 'maria@example.com',
      phone: '(11) 99999-8888',
      source: 'Instagram',
      status: 'New',
      score: 'Gold',
      salesPersonId: 'seller-2',
      interestedModel: 'Modelo Y',
      createdAt: '2025-01-16T14:30:00Z',
    },
    {
      id: 'lead-3',
      name: 'Pedro Costa',
      email: 'pedro@example.com',
      phone: '(11) 97777-6666',
      source: 'Facebook',
      status: 'Contacted',
      score: 'Silver',
      salesPersonId: 'seller-1',
      interestedModel: 'Modelo Z',
      createdAt: '2025-01-17T09:15:00Z',
    },
    {
      id: 'lead-4',
      name: 'Ana Lima',
      email: 'ana@example.com',
      phone: '(11) 96666-5555',
      source: 'Google',
      status: 'New',
      score: 'Bronze',
      salesPersonId: 'seller-3',
      interestedModel: 'Modelo W',
      createdAt: '2025-01-18T11:45:00Z',
    },
  ],
  page: 1,
  pageSize: 10,
  totalCount: 4,
  totalPages: 1,
  hasNextPage: false,
  hasPreviousPage: false,
};

describe('LeadListPage Integration - Task 4.0', () => {
  let queryClient: QueryClient;

  beforeEach(() => {
    queryClient = new QueryClient({
      defaultOptions: {
        queries: {
          retry: false,
        },
      },
    });

    // Mock do hook useLeads
    vi.spyOn(useLeadsHook, 'useLeads').mockReturnValue({
      data: mockLeadsData,
      isLoading: false,
      isError: false,
    } as any);
  });

  const renderComponent = () => {
    return render(
      <QueryClientProvider client={queryClient}>
        <MemoryRouter>
          <LeadListPage />
        </MemoryRouter>
      </QueryClientProvider>
    );
  };

  it('deve renderizar a listagem de leads', async () => {
    renderComponent();

    await waitFor(() => {
      expect(screen.getByText('João Silva')).toBeInTheDocument();
    });

    expect(screen.getByText('Maria Santos')).toBeInTheDocument();
    expect(screen.getByText('Pedro Costa')).toBeInTheDocument();
    expect(screen.getByText('Ana Lima')).toBeInTheDocument();
  });

  it('deve exibir badges de score para cada lead', async () => {
    renderComponent();

    await waitFor(() => {
      expect(screen.getByText('Diamante')).toBeInTheDocument();
    });

    // Verifica se todos os scores estão visíveis
    expect(screen.getByText('Diamante')).toBeInTheDocument(); // Diamond
    expect(screen.getByText('Ouro')).toBeInTheDocument(); // Gold
    expect(screen.getByText('Prata')).toBeInTheDocument(); // Silver
    expect(screen.getByText('Bronze')).toBeInTheDocument(); // Bronze
  });

  it('não deve exibir SLA nos badges da listagem', async () => {
    renderComponent();

    await waitFor(() => {
      expect(screen.getByText('Diamante')).toBeInTheDocument();
    });

    // Verifica que os textos de SLA não estão presentes
    expect(screen.queryByText('Atender em 10 min')).not.toBeInTheDocument();
    expect(screen.queryByText('Atender em 30 min')).not.toBeInTheDocument();
    expect(screen.queryByText('Atender em 2h')).not.toBeInTheDocument();
    expect(screen.queryByText('Baixa Prioridade')).not.toBeInTheDocument();
  });

  it('deve exibir lead sem score sem quebrar', async () => {
    const leadsSemScore = {
      ...mockLeadsData,
      items: [
        {
          ...mockLeadsData.items[0],
          score: undefined,
        },
      ],
    };

    vi.spyOn(useLeadsHook, 'useLeads').mockReturnValue({
      data: leadsSemScore,
      isLoading: false,
      isError: false,
    } as any);

    renderComponent();

    await waitFor(() => {
      expect(screen.getByText('João Silva')).toBeInTheDocument();
    });

    // Lead deve ser renderizado mesmo sem score
    expect(screen.getByText('João Silva')).toBeInTheDocument();
    // Badge de score não deve aparecer
    expect(screen.queryByText('Diamante')).not.toBeInTheDocument();
  });

  it('deve renderizar status dos leads corretamente', async () => {
    renderComponent();

    await waitFor(() => {
      expect(screen.getByText('Qualificado')).toBeInTheDocument();
    });

    expect(screen.getByText('Qualificado')).toBeInTheDocument();
    expect(screen.getAllByText('Novo')).toHaveLength(2);
    expect(screen.getByText('Contatado')).toBeInTheDocument();
  });

  it('deve exibir coluna de Score na tabela', async () => {
    renderComponent();

    await waitFor(() => {
      expect(screen.getByText('Score')).toBeInTheDocument();
    });

    // Verifica se o cabeçalho da coluna Score está presente
    const scoreHeader = screen.getByText('Score');
    expect(scoreHeader).toBeInTheDocument();
  });
});
