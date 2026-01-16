import { api } from '../../../lib/api';
import type {
  CancelReservationRequest,
  CreateReservationRequest,
  ExtendReservationRequest,
  PagedResponse,
  ReservationListItem,
  ReservationResponse,
} from '../types';
import { handleProblemDetailsError } from './problemDetails';
import { buildPaginationParams, compactParams } from './queryParams';

const VEHICLES_URL = '/vehicles';
const RESERVATIONS_URL = '/reservations';

export const reservationService = {
  list: async (params?: {
    page?: number;
    size?: number;
    status?: string;
    type?: string;
    q?: string;
    includeCompatPagination?: boolean;
    salesPersonId?: string;
    vehicleId?: string;
  }) => {
    const pagination = buildPaginationParams({
      page: params?.page,
      size: params?.size,
      includeCompat: params?.includeCompatPagination,
    });

    const safeParams = compactParams({
      ...pagination,
      status: params?.status,
      type: params?.type,
      q: params?.q,
      salesPersonId: params?.salesPersonId,
      vehicleId: params?.vehicleId,
    });

    try {
      const response = await api.get<PagedResponse<ReservationListItem>>(RESERVATIONS_URL, {
        params: safeParams,
      });
      return response.data;
    } catch (error) {
      handleProblemDetailsError(error, 'Falha ao carregar reservas');
    }
  },

  create: async (vehicleId: string, data: CreateReservationRequest) => {
    try {
      const response = await api.post<ReservationResponse>(`${VEHICLES_URL}/${vehicleId}/reservations`, data);
      return response.data;
    } catch (error) {
      handleProblemDetailsError(error, 'Falha ao criar reserva');
    }
  },

  cancel: async (reservationId: string, data: CancelReservationRequest) => {
    try {
      const response = await api.post<ReservationResponse>(`${RESERVATIONS_URL}/${reservationId}/cancel`, data);
      return response.data;
    } catch (error) {
      handleProblemDetailsError(error, 'Falha ao cancelar reserva');
    }
  },

  extend: async (reservationId: string, data: ExtendReservationRequest) => {
    try {
      const response = await api.post<ReservationResponse>(`${RESERVATIONS_URL}/${reservationId}/extend`, data);
      return response.data;
    } catch (error) {
      handleProblemDetailsError(error, 'Falha ao prorrogar reserva');
    }
  },
};
