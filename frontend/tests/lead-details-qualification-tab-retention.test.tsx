import { describe, it, expect, vi, beforeEach } from 'vitest';
import React from 'react';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { LeadDetailsPage } from '../src/modules/commercial/pages/LeadDetailsPage';
import * as leadsHook from '../src/modules/commercial/hooks/useLeads';
import * as paymentMethodsHook from '../src/modules/commercial/hooks/usePaymentMethods';
import * as toastHook from '../src/hooks/use-toast';

describe('COM-LEAD-DETAIL-004 - Salvar qualificação mantém aba', () => {
  let queryClient: QueryClient;
  const toastSpy = vi.fn();

  beforeEach(() => {
    queryClient = new QueryClient({
      defaultOptions: {
        queries: {
          retry: false,
        },
      },
    });

    vi.spyOn(leadsHook, 'useLead').mockReturnValue({
      data: {
        id: 'lead-123',
        name: 'João Silva',
        email: 'joao@example.com',
        phone: '(11) 98765-4321',
        source: 'Website',
        status: 'Qualified',
        score: 'Diamond',
        salesPersonId: 'seller-1',
        qualification: {
          paymentMethod: 'CASH',
          expectedPurchaseDate: '2025-01-20T10:00:00Z',
          interestedInTestDrive: false,
          hasTradeInVehicle: false,
        },
        createdAt: '2025-01-15T10:00:00Z',
        updatedAt: '2025-01-15T12:00:00Z',
      },
      isLoading: false,
      isError: false,
    } as any);

    vi.spyOn(paymentMethodsHook, 'usePaymentMethods').mockReturnValue({
      data: [
        {
          id: 1,
          code: 'CASH',
          name: 'À Vista',
          isActive: true,
          displayOrder: 1,
        },
      ],
      isLoading: false,
      isError: false,
    } as any);

    vi.spyOn(leadsHook, 'useQualifyLead').mockReturnValue({
      isPending: false,
      mutate: (_variables: unknown, options?: { onSuccess?: () => void; onError?: () => void }) => {
        options?.onSuccess?.();
      },
    } as any);

    vi.spyOn(toastHook, 'useToast').mockReturnValue({
      toast: toastSpy,
    } as any);
  });

  it(
    'permanece na aba Qualificação após salvar',
    async () => {
      const user = userEvent.setup();

      render(
        <QueryClientProvider client={queryClient}>
          <MemoryRouter initialEntries={['/leads/lead-123']}>
            <Routes>
              <Route path="/leads/:id" element={<LeadDetailsPage />} />
            </Routes>
          </MemoryRouter>
        </QueryClientProvider>
      );

      await waitFor(() => {
        expect(screen.getByRole('tab', { name: /Qualificação/i })).toBeInTheDocument();
      });

      await user.click(screen.getByRole('tab', { name: /Qualificação/i }));

      const saveButton = await screen.findByRole('button', {
        name: /Salvar Qualificação/i,
      });
      await user.click(saveButton);

      // Antes o fluxo voltava para "Visão Geral". Agora, o conteúdo da Qualificação deve continuar visível.
      expect(
        screen.getByRole('button', { name: /Salvar Qualificação/i })
      ).toBeInTheDocument();
    },
    15000
  );
});
