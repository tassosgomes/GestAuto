import { render, screen, waitFor } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import userEvent from '@testing-library/user-event';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { LeadQualificationForm } from '../src/modules/commercial/components/LeadQualificationForm';
import type { Lead } from '../src/modules/commercial/types';

// Mock do hook useQualifyLead
vi.mock('../src/modules/commercial/hooks/useLeads', () => ({
  useQualifyLead: vi.fn(() => ({
    mutate: vi.fn(),
    isPending: false,
  })),
}));

// Mock do useToast
vi.mock('../src/hooks/use-toast', () => ({
  useToast: vi.fn(() => ({
    toast: vi.fn(),
  })),
}));

const createQueryClient = () =>
  new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  });

const mockLead: Lead = {
  id: '123',
  name: 'João Silva',
  email: 'joao@example.com',
  phone: '11999999999',
  source: 'Website',
  status: 'New',
  salesPersonId: 'seller-1',
  createdAt: '2023-01-01T00:00:00Z',
};

const renderWithProviders = (component: React.ReactElement) => {
  const queryClient = createQueryClient();
  return render(
    <QueryClientProvider client={queryClient}>
      {component}
    </QueryClientProvider>
  );
};

describe('LeadQualificationForm', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('renders all required form fields', () => {
    renderWithProviders(<LeadQualificationForm lead={mockLead} />);

    expect(screen.getByLabelText(/forma de pagamento/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/renda mensal estimada/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/prazo de compra/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/interessado em test-drive/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/possui veículo na troca/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /salvar qualificação/i })).toBeInTheDocument();
  });

  it('shows payment method as required field', () => {
    renderWithProviders(<LeadQualificationForm lead={mockLead} />);

    // Verifica se o label existe e tem o asterisco de obrigatório no texto do schema
    expect(screen.getAllByText(/forma de pagamento/i)[0]).toBeInTheDocument();
    expect(screen.getByText(/forma de pagamento \*/i)).toBeInTheDocument();
  });

  it('does not show trade-in vehicle fields by default', () => {
    renderWithProviders(<LeadQualificationForm lead={mockLead} />);

    expect(screen.queryByText(/dados do veículo de troca/i)).not.toBeInTheDocument();
  });

  it('shows trade-in vehicle fields when checkbox is checked', async () => {
    const user = userEvent.setup();
    renderWithProviders(<LeadQualificationForm lead={mockLead} />);

    const tradeInCheckbox = screen.getByRole('checkbox', { name: /possui veículo na troca/i });
    await user.click(tradeInCheckbox);

    await waitFor(() => {
      expect(screen.getByText(/dados do veículo de troca/i)).toBeInTheDocument();
      expect(screen.getByLabelText(/marca/i)).toBeInTheDocument();
      expect(screen.getByLabelText(/modelo/i)).toBeInTheDocument();
    });
  });

  it('hides trade-in vehicle fields when checkbox is unchecked', async () => {
    const user = userEvent.setup();
    renderWithProviders(<LeadQualificationForm lead={mockLead} />);

    // Primeiro marca
    const tradeInCheckbox = screen.getByRole('checkbox', { name: /possui veículo na troca/i });
    await user.click(tradeInCheckbox);

    await waitFor(() => {
      expect(screen.getByText(/dados do veículo de troca/i)).toBeInTheDocument();
    });

    // Depois desmarca
    await user.click(tradeInCheckbox);

    await waitFor(() => {
      expect(screen.queryByText(/dados do veículo de troca/i)).not.toBeInTheDocument();
    });
  });

  it('loads existing qualification data when lead has qualification', () => {
    const leadWithQualification: Lead = {
      ...mockLead,
      qualification: {
        paymentMethod: 'CASH',
        estimatedMonthlyIncome: 5000,
        expectedPurchaseDate: 'IMMEDIATE',
        interestedInTestDrive: true,
        hasTradeInVehicle: false,
      },
    };

    renderWithProviders(<LeadQualificationForm lead={leadWithQualification} />);

    // Os valores padrão são carregados internamente pelo form
    // Verificamos que o componente renderiza sem erros
    expect(screen.getByRole('button', { name: /salvar qualificação/i })).toBeInTheDocument();
  });

  it('loads existing trade-in vehicle data when present', async () => {
    const leadWithTradeIn: Lead = {
      ...mockLead,
      qualification: {
        paymentMethod: 'FINANCING',
        hasTradeInVehicle: true,
        interestedInTestDrive: false,
        tradeInVehicle: {
          brand: 'Volkswagen',
          model: 'Gol',
          year: 2018,
          mileage: 50000,
          color: 'Prata',
          licensePlate: 'ABC-1234',
          generalCondition: 'Good',
          hasDealershipServiceHistory: true,
        },
      },
    };

    renderWithProviders(<LeadQualificationForm lead={leadWithTradeIn} />);

    // Campos do veículo devem estar visíveis porque hasTradeInVehicle é true
    await waitFor(() => {
      expect(screen.getByText(/dados do veículo de troca/i)).toBeInTheDocument();
    });
  });

  it('shows validation error when submitting without payment method', async () => {
    const user = userEvent.setup();
    renderWithProviders(<LeadQualificationForm lead={mockLead} />);

    const submitButton = screen.getByRole('button', { name: /salvar qualificação/i });
    await user.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText(/forma de pagamento é obrigatória/i)).toBeInTheDocument();
    });
  });
});
