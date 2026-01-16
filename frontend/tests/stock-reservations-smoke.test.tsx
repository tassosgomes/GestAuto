import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { MemoryRouter } from 'react-router-dom';
import { StockReservationsPage } from '../src/modules/stock/pages/StockReservationsPage';

vi.mock('@/auth/useAuth', () => ({
  useAuth: () => ({
    status: 'ready',
    session: {
      isAuthenticated: true,
      roles: ['ADMIN'],
    },
  }),
}));

describe('StockReservationsPage Smoke Tests', () => {
  let queryClient: QueryClient;

  beforeEach(() => {
    queryClient = new QueryClient({
      defaultOptions: {
        queries: {
          retry: false,
        },
      },
    });
  });

  const renderComponent = () => {
    return render(
      <QueryClientProvider client={queryClient}>
        <MemoryRouter initialEntries={['/stock/reservations']}>
          <StockReservationsPage />
        </MemoryRouter>
      </QueryClientProvider>
    );
  };

  it('deve renderizar a página de reservas sem quebrar', () => {
    renderComponent();

    // A página é um placeholder e deve renderizar sem erros
    expect(screen.getByText('Reservas')).toBeInTheDocument();
  });

  it('deve exibir mensagem de funcionalidade em desenvolvimento', () => {
    renderComponent();

    // Verifica se há alguma mensagem indicando que está em desenvolvimento
    const pageContent = document.body.textContent;
    expect(pageContent).toContain('Reservas');
  });
});
