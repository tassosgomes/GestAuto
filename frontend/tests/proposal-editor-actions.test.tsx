import { describe, expect, it, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import React from 'react'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'

import { ProposalEditorPage } from '@/modules/commercial/pages/ProposalEditorPage'

const leadsMock = { items: [] as Array<{ id: string; name: string }> }

const proposalMock = {
  id: '11111111-1111-1111-1111-111111111111',
  leadId: '22222222-2222-2222-2222-222222222222',
  status: 'AwaitingDiscountApproval',
  vehicleModel: 'Model X',
  vehicleTrim: 'Trim',
  vehicleColor: 'Black',
  vehicleYear: 2024,
  isReadyDelivery: true,
  vehiclePrice: 100000,
  discountAmount: 6000,
  discountReason: 'Campanha',
  discountApproved: false,
  tradeInValue: 0,
  paymentMethod: 'CASH',
  downPayment: 0,
  installments: undefined as number | undefined,
  items: [] as Array<any>,
  usedVehicleEvaluationId: undefined as string | undefined,
  totalValue: 94000,
  createdAt: '2025-01-01T00:00:00.000Z',
}

vi.mock('@/components/ui/select', async () => {
  const React = await import('react')
  return {
    Select: ({ children }: { children: React.ReactNode }) => <div>{children}</div>,
    SelectTrigger: ({ children }: { children: React.ReactNode }) => <button type="button">{children}</button>,
    SelectValue: ({ placeholder }: { placeholder?: string }) => <span>{placeholder}</span>,
    SelectContent: ({ children }: { children: React.ReactNode }) => <div>{children}</div>,
    SelectItem: ({ children }: { children: React.ReactNode }) => <div>{children}</div>,
  }
})

vi.mock('@/modules/commercial/components/proposal/VehicleSelection', () => ({
  VehicleSelection: () => <div />,
}))
vi.mock('@/modules/commercial/components/proposal/PaymentForm', () => ({
  PaymentForm: () => <div />,
}))
vi.mock('@/modules/commercial/components/proposal/AccessoriesSection', () => ({
  AccessoriesSection: () => <div />,
}))
vi.mock('@/modules/commercial/components/proposal/ProposalSummary', () => ({
  ProposalSummary: () => <div />,
}))
vi.mock('@/modules/commercial/components/proposal/TradeInSection', () => ({
  TradeInSection: () => <div />,
}))

const toastSpy = vi.fn()
vi.mock('@/hooks/use-toast', () => ({
  useToast: () => ({ toast: toastSpy }),
}))

vi.mock('@/modules/commercial/hooks/useLeads', () => ({
  useLeads: () => ({ data: leadsMock, isLoading: false, isError: false }),
}))

vi.mock('@/modules/commercial/services/proposalService', () => ({
  proposalService: {
    addItem: vi.fn(),
    removeItem: vi.fn(),
    applyDiscount: vi.fn(),
    close: vi.fn(),
  },
}))

vi.mock('@/modules/commercial/hooks/useProposals', () => ({
  useCreateProposal: () => ({ mutateAsync: vi.fn(async () => ({ id: '11111111-1111-1111-1111-111111111111' })) }),
  useUpdateProposal: () => ({ mutateAsync: vi.fn(async () => ({})) }),
  useProposal: () => ({
    data: proposalMock,
    isLoading: false,
  }),
}))

describe('ProposalEditorPage actions', () => {
  it('disables Close Sale when awaiting discount approval', async () => {
    const queryClient = new QueryClient({
      defaultOptions: {
        queries: { retry: false },
      },
    })

    render(
      <QueryClientProvider client={queryClient}>
        <MemoryRouter initialEntries={['/commercial/proposals/11111111-1111-1111-1111-111111111111/edit']}>
          <Routes>
            <Route path="/commercial/proposals/:id/edit" element={<ProposalEditorPage />} />
          </Routes>
        </MemoryRouter>
      </QueryClientProvider>,
    )

    expect(await screen.findByText('Editar Proposta')).toBeInTheDocument()

    const closeBtn = screen.getByRole('button', { name: 'Fechar Venda' })
    expect(closeBtn).toBeDisabled()

    const approvalBtn = screen.getByRole('button', { name: 'Solicitar Aprovação' })
    expect(approvalBtn).toBeEnabled()
  })
})
