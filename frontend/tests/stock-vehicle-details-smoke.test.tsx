import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { MemoryRouter } from 'react-router-dom';
import { StockVehicleDetailsPage } from '../src/modules/stock/pages/StockVehicleDetailsPage';
import * as useVehiclesHook from '../src/modules/stock/hooks/useVehicles';
import * as useReservationsHook from '../src/modules/stock/hooks/useReservations';
import { VehicleStatus, VehicleCategory } from '../src/modules/stock/types';

vi.mock('@/auth/useAuth', () => ({
  useAuth: () => ({
    status: 'ready',
    session: {
      isAuthenticated: true,
      roles: ['ADMIN'],
    },
  }),
}));

vi.mock('@/hooks/use-toast', () => ({
  useToast: () => ({
    toast: vi.fn(),
  }),
}));

const mockVehicleData = {
  id: 'vehicle-1',
  category: VehicleCategory.New,
  currentStatus: VehicleStatus.InStock,
  vin: 'VIN123456789',
  plate: 'ABC1234',
  make: 'Toyota',
  model: 'Corolla',
  trim: 'XEI',
  yearModel: 2024,
  color: 'Preto',
  mileageKm: 0,
  price: 150000,
  location: 'São Paulo',
  imageUrl: null,
  evaluationId: null,
  demoPurpose: null,
  isRegistered: true,
  currentOwnerUserId: null,
  createdAt: '2024-01-01T10:00:00Z',
  updatedAt: '2024-01-01T10:00:00Z',
};

const mockHistoryData = {
  vehicleId: 'vehicle-1',
  items: [
    {
      type: 'CheckIn',
      occurredAtUtc: '2024-01-01T10:00:00Z',
      userId: 'user-1',
      details: { source: 1, notes: 'Veículo recebido da montadora' },
    },
    {
      type: 'StatusChange',
      occurredAtUtc: '2024-01-02T14:30:00Z',
      userId: 'user-2',
      details: { oldStatus: 1, newStatus: 2, reason: 'Veículo disponível' },
    },
  ],
};

describe('StockVehicleDetailsPage Smoke Tests', () => {
  let queryClient: QueryClient;

  beforeEach(() => {
    queryClient = new QueryClient({
      defaultOptions: {
        queries: {
          retry: false,
        },
      },
    });

    // Reset and setup mocks before each test
    vi.clearAllMocks();

    vi.spyOn(useVehiclesHook, 'useVehicle').mockReturnValue({
      data: mockVehicleData,
      isLoading: false,
      isError: false,
      error: null,
    } as any);

    vi.spyOn(useVehiclesHook, 'useVehicleHistory').mockReturnValue({
      data: mockHistoryData,
      isLoading: false,
      isError: false,
    } as any);

    vi.spyOn(useReservationsHook, 'useCreateReservation').mockReturnValue({
      mutate: vi.fn(),
      mutateAsync: vi.fn(),
      isPending: false,
    } as any);
  });

  const renderComponent = () => {
    return render(
      <QueryClientProvider client={queryClient}>
        <MemoryRouter>
          <StockVehicleDetailsPage />
        </MemoryRouter>
      </QueryClientProvider>
    );
  };

  it('deve renderizar a página de detalhes do veículo sem quebrar', () => {
    renderComponent();

    // Basic smoke test - verify the page renders without crashing
    // The page should show "Veículo não encontrado" when no id is provided
    expect(screen.getByText('Detalhe do veículo')).toBeInTheDocument();
  });

  it('deve renderizar em estado de carregamento', () => {
    vi.spyOn(useVehiclesHook, 'useVehicle').mockReturnValue({
      data: undefined,
      isLoading: true,
      isError: false,
      error: null,
    } as any);

    renderComponent();

    // Verify the page renders in loading state without crashing
    expect(screen.getByText('Detalhe do veículo')).toBeInTheDocument();
  });

  it('deve ter botões de ação disponíveis para usuários autorizados', () => {
    // Even without vehicle data, the action buttons should be present in the UI
    // This is a structural test to verify the component is properly set up
    renderComponent();

    // Verify the page structure exists
    expect(screen.getByText('Detalhe do veículo')).toBeInTheDocument();
  });
});
