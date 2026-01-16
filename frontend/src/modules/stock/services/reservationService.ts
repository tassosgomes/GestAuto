import { api } from '../../../lib/api';
import type {
  CancelReservationRequest,
  CreateReservationRequest,
  ExtendReservationRequest,
  ReservationResponse,
} from '../types';
import { handleProblemDetailsError } from './problemDetails';

const VEHICLES_URL = '/vehicles';
const RESERVATIONS_URL = '/reservations';

export const reservationService = {
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
