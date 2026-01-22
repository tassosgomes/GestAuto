import { stockApi } from '../../../lib/api';
import type {
  ChangeVehicleStatusRequest,
  CheckInCreateRequest,
  CheckInResponse,
  CheckOutCreateRequest,
  CheckOutResponse,
  PagedResponse,
  StartTestDriveRequest,
  StartTestDriveResponse,
  VehicleHistoryResponse,
  VehicleListItem,
  VehicleResponse,
} from '../types';
import { handleProblemDetailsError } from './problemDetails';
import { buildPaginationParams, compactParams } from './queryParams';

const BASE_URL = '/vehicles';

export const vehicleService = {
  list: async (params?: {
    page?: number;
    size?: number;
    status?: string;
    category?: string;
    q?: string;
    includeCompatPagination?: boolean;
  }) => {
    const pagination = buildPaginationParams({
      page: params?.page,
      size: params?.size,
      includeCompat: params?.includeCompatPagination,
    });

    const safeParams = compactParams({
      ...pagination,
      status: params?.status,
      category: params?.category,
      q: params?.q,
    });

    try {
      const response = await stockApi.get<PagedResponse<VehicleListItem>>(BASE_URL, { params: safeParams });
      return response.data;
    } catch (error) {
      handleProblemDetailsError(error, 'Falha ao carregar veículos');
    }
  },

  getById: async (id: string) => {
    try {
      const response = await stockApi.get<VehicleResponse>(`${BASE_URL}/${id}`);
      return response.data;
    } catch (error) {
      handleProblemDetailsError(error, 'Falha ao carregar veículo');
    }
  },

  getHistory: async (id: string) => {
    try {
      const response = await stockApi.get<VehicleHistoryResponse>(`${BASE_URL}/${id}/history`);
      return response.data;
    } catch (error) {
      handleProblemDetailsError(error, 'Falha ao carregar histórico do veículo');
    }
  },

  changeStatus: async (id: string, data: ChangeVehicleStatusRequest) => {
    try {
      await stockApi.patch(`${BASE_URL}/${id}/status`, data);
    } catch (error) {
      handleProblemDetailsError(error, 'Falha ao alterar status do veículo');
    }
  },

  checkIn: async (id: string, data: CheckInCreateRequest) => {
    try {
      const response = await stockApi.post<CheckInResponse>(`${BASE_URL}/${id}/check-ins`, data);
      return response.data;
    } catch (error) {
      handleProblemDetailsError(error, 'Falha ao registrar entrada do veículo');
    }
  },

  checkOut: async (id: string, data: CheckOutCreateRequest) => {
    try {
      const response = await stockApi.post<CheckOutResponse>(`${BASE_URL}/${id}/check-outs`, data);
      return response.data;
    } catch (error) {
      handleProblemDetailsError(error, 'Falha ao registrar saída do veículo');
    }
  },

  startTestDrive: async (id: string, data: StartTestDriveRequest) => {
    try {
      const response = await stockApi.post<StartTestDriveResponse>(`${BASE_URL}/${id}/test-drives/start`, data);
      return response.data;
    } catch (error) {
      handleProblemDetailsError(error, 'Falha ao iniciar test-drive');
    }
  },
};
