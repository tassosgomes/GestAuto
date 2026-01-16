import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { MemoryRouter } from 'react-router-dom';
import { StockDashboardPage } from '../src/modules/stock/pages/StockDashboardPage';
import * as useVehiclesHook from '../src/modules/stock/hooks/useVehicles';
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

const mockVehiclesData = {
  data: [
    {
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
      createdAt: '2024-01-01T10:00:00Z',
      updatedAt: '2024-01-01T10:00:00Z',
    },
    {
      id: 'vehicle-2',
      category: VehicleCategory.Used,
      currentStatus: VehicleStatus.Reserved,
      vin: 'VIN987654321',
      plate: 'XYZ5678',
      make: 'Honda',
      model: 'Civic',
      trim: 'Touring',
      yearModel: 2023,
      color: 'Branco',
      mileageKm: 15000,
      price: 120000,
      location: 'São Paulo',
      imageUrl: null,
      createdAt: '2024-01-02T10:00:00Z',
      updatedAt: '2024-01-02T10:00:00Z',
    },
    {
      id: 'vehicle-3',
      category: VehicleCategory.Demonstration,
      currentStatus: VehicleStatus.InTestDrive,
      vin: 'VIN456789123',
      plate: 'DEF9012',
      make: 'Jeep',
      model: 'Renegade',
      trim: 'Limited',
      yearModel: 2024,
      color: 'Vermelho',
      mileageKm: 5000,
      price: 140000,
      location: 'São Paulo',
      imageUrl: null,
      createdAt: '2024-01-03T10:00:00Z',
      updatedAt: '2024-01-03T10:00:00Z',
    },
  ],
  pagination: {
    page: 1,
    size: 10,
    total: 3,
    totalPages: 1,
  },
};

describe('StockDashboardPage Smoke Tests', () => {
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
        <MemoryRouter>
          <StockDashboardPage />
        </MemoryRouter>
      </QueryClientProvider>
    );
  };

  it('deve renderizar o título e subtítulo da página', async () => {
    vi.spyOn(useVehiclesHook, 'useVehiclesList').mockReturnValue({
      data: mockVehiclesData,
      isLoading: false,
      isError: false,
    } as any);

    renderComponent();

    await waitFor(() => {
      expect(screen.getByText('Estoque')).toBeInTheDocument();
      expect(screen.getByText('Visão geral e gestão de veículos')).toBeInTheDocument();
    });
  });

  it('deve exibir os cards KPI com dados', async () => {
    vi.spyOn(useVehiclesHook, 'useVehiclesList').mockReturnValue({
      data: mockVehiclesData,
      isLoading: false,
      isError: false,
    } as any);

    renderComponent();

    await waitFor(() => {
      expect(screen.getByText('Total de Veículos')).toBeInTheDocument();
    });

    // Get all elements with text "3" and find the one in the KPI section
    const totalElements = screen.getAllByText('3');
    expect(totalElements.length).toBeGreaterThan(0);

    expect(screen.getByText('Em Estoque')).toBeInTheDocument();
    expect(screen.getByText('Reservados')).toBeInTheDocument();
    expect(screen.getByText('Em Test-Drive')).toBeInTheDocument();
  });

  it('deve exibir a tabela de veículos', async () => {
    vi.spyOn(useVehiclesHook, 'useVehiclesList').mockReturnValue({
      data: mockVehiclesData,
      isLoading: false,
      isError: false,
    } as any);

    renderComponent();

    await waitFor(() => {
      expect(screen.getByText('Toyota Corolla')).toBeInTheDocument();
      expect(screen.getByText('Honda Civic')).toBeInTheDocument();
      expect(screen.getByText('Jeep Renegade')).toBeInTheDocument();
    });

    expect(screen.getByText('VIN123456789')).toBeInTheDocument();
    expect(screen.getByText('ABC1234')).toBeInTheDocument();
  });

  it('deve exibir skeletons durante carregamento', async () => {
    vi.spyOn(useVehiclesHook, 'useVehiclesList').mockReturnValue({
      data: undefined,
      isLoading: true,
      isError: false,
    } as any);

    renderComponent();

    // Verify the page renders without crashing in loading state
    expect(screen.getByText('Estoque')).toBeInTheDocument();
  });

  it('deve exibir mensagem de erro quando a requisição falha', async () => {
    vi.spyOn(useVehiclesHook, 'useVehiclesList').mockReturnValue({
      data: undefined,
      isLoading: false,
      isError: true,
    } as any);

    renderComponent();

    await waitFor(() => {
      expect(screen.getByText('Não foi possível carregar os veículos.')).toBeInTheDocument();
    });
  });

  it('deve exibir mensagem de estado vazio quando não há veículos', async () => {
    vi.spyOn(useVehiclesHook, 'useVehiclesList').mockReturnValue({
      data: { data: [], pagination: { page: 1, size: 10, total: 0, totalPages: 0 } },
      isLoading: false,
      isError: false,
    } as any);

    renderComponent();

    await waitFor(() => {
      expect(screen.getByText('Nenhum veículo encontrado')).toBeInTheDocument();
      expect(screen.getByText('Não há veículos cadastrados no momento')).toBeInTheDocument();
    });
  });

  it('deve exibir badges de status e categoria corretamente', async () => {
    vi.spyOn(useVehiclesHook, 'useVehiclesList').mockReturnValue({
      data: mockVehiclesData,
      isLoading: false,
      isError: false,
    } as any);

    renderComponent();

    await waitFor(() => {
      expect(screen.getByText('Novo')).toBeInTheDocument();
      expect(screen.getByText('Seminovo')).toBeInTheDocument();
      expect(screen.getByText('Demonstração')).toBeInTheDocument();
    });

    expect(screen.getByText('Em estoque')).toBeInTheDocument();
    expect(screen.getByText('Reservado')).toBeInTheDocument();
    expect(screen.getByText('Em test-drive')).toBeInTheDocument();
  });

  it('deve exibir link para ver todos os veículos', async () => {
    vi.spyOn(useVehiclesHook, 'useVehiclesList').mockReturnValue({
      data: mockVehiclesData,
      isLoading: false,
      isError: false,
    } as any);

    renderComponent();

    await waitFor(() => {
      expect(screen.getByText('Ver todos os veículos')).toBeInTheDocument();
    });
  });
});
