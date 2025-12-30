import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { LeadDetailsPage } from '../src/modules/commercial/pages/LeadDetailsPage';
import * as useLeadsHook from '../src/modules/commercial/hooks/useLeads';

// Mock do lead com dados de qualificação
const mockLead = {
  id: 'lead-123',
  name: 'João Silva',
  email: 'joao@example.com',
  phone: '(11) 98765-4321',
  source: 'Website',
  status: 'Qualified',
  score: 'Diamond',
  salesPersonId: 'seller-1',
  interestedModel: 'Modelo X',
  interestedTrim: 'Versão Premium',
  interestedColor: 'Azul',
  qualification: {
    hasTradeInVehicle: true,
    tradeInVehicle: {
      brand: 'Honda',
      model: 'Civic',
      year: 2020,
      mileage: 30000,
      generalCondition: 'Excellent',
      hasDealershipServiceHistory: true,
    },
    paymentMethod: 'CASH',
    expectedPurchaseDate: 'IMMEDIATE',
    interestedInTestDrive: true,
  },
  createdAt: '2025-01-15T10:00:00Z',
  updatedAt: '2025-01-15T12:00:00Z',
};

describe('LeadDetailsPage Integration - Task 3.0', () => {
  let queryClient: QueryClient;

  beforeEach(() => {
    queryClient = new QueryClient({
      defaultOptions: {
        queries: {
          retry: false,
        },
      },
    });

    // Mock do hook useLead
    vi.spyOn(useLeadsHook, 'useLead').mockReturnValue({
      data: mockLead,
      isLoading: false,
      isError: false,
    } as any);
  });

  const renderComponent = () => {
    return render(
      <QueryClientProvider client={queryClient}>
        <MemoryRouter initialEntries={['/leads/lead-123']}>
          <Routes>
            <Route path="/leads/:id" element={<LeadDetailsPage />} />
          </Routes>
        </MemoryRouter>
      </QueryClientProvider>
    );
  };

  it('deve renderizar a página de detalhes com header atualizado', async () => {
    renderComponent();

    await waitFor(() => {
      expect(screen.getByText('João Silva')).toBeInTheDocument();
    });

    // Verifica se o nome do lead está presente
    expect(screen.getByText('João Silva')).toBeInTheDocument();
    expect(screen.getByText('Qualificado')).toBeInTheDocument();
  });

  it('deve exibir aba de Qualificação', async () => {
    renderComponent();

    await waitFor(() => {
      expect(screen.getByRole('tab', { name: /Qualificação/i })).toBeInTheDocument();
    });

    const qualificationTab = screen.getByRole('tab', { name: /Qualificação/i });
    expect(qualificationTab).toBeInTheDocument();
  });

  it('deve exibir todas as abas esperadas', async () => {
    renderComponent();

    await waitFor(() => {
      expect(screen.getByRole('tab', { name: /Visão Geral/i })).toBeInTheDocument();
    });

    expect(screen.getByRole('tab', { name: /Visão Geral/i })).toBeInTheDocument();
    expect(screen.getByRole('tab', { name: /Qualificação/i })).toBeInTheDocument();
    expect(screen.getByRole('tab', { name: /Timeline/i })).toBeInTheDocument();
    expect(screen.getByRole('tab', { name: /Propostas/i })).toBeInTheDocument();
  });

  it('deve exibir o badge de score Diamante no header', async () => {
    renderComponent();

    await waitFor(() => {
      expect(screen.getByText('Diamante')).toBeInTheDocument();
    });

    // Verifica se o badge de score está presente
    expect(screen.getByText('Diamante')).toBeInTheDocument();
  });

  it('deve exibir o SLA de atendimento no badge', async () => {
    renderComponent();

    await waitFor(() => {
      expect(screen.getByText('Atender em 10 min')).toBeInTheDocument();
    });

    expect(screen.getByText('Atender em 10 min')).toBeInTheDocument();
  });

  it('deve renderizar lead sem score sem quebrar', async () => {
    const leadSemScore = { ...mockLead, score: undefined };

    vi.spyOn(useLeadsHook, 'useLead').mockReturnValue({
      data: leadSemScore,
      isLoading: false,
      isError: false,
    } as any);

    renderComponent();

    await waitFor(() => {
      expect(screen.getByText('João Silva')).toBeInTheDocument();
    });

    // Não deve haver badge de score
    expect(screen.queryByText('Diamante')).not.toBeInTheDocument();
  });
});
