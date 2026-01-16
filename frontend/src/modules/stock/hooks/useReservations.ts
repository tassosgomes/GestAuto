import { QueryClient, useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { reservationService } from '../services/reservationService';
import type { CancelReservationRequest, CreateReservationRequest, ExtendReservationRequest } from '../types';

const invalidateVehicleQueries = (queryClient: QueryClient, vehicleId?: string) => {
  queryClient.invalidateQueries({ queryKey: ['stock-vehicles'] });

  if (vehicleId) {
    queryClient.invalidateQueries({ queryKey: ['stock-vehicle', vehicleId] });
    queryClient.invalidateQueries({ queryKey: ['stock-vehicle-history', vehicleId] });
  }
};

export const useCreateReservation = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ vehicleId, data }: { vehicleId: string; data: CreateReservationRequest }) =>
      reservationService.create(vehicleId, data),
    onSuccess: (data, variables) => {
      invalidateVehicleQueries(queryClient, data?.vehicleId ?? variables.vehicleId);
    },
  });
};

export const useReservationsList = (params?: {
  page?: number;
  size?: number;
  status?: string;
  type?: string;
  q?: string;
  includeCompatPagination?: boolean;
}) => {
  return useQuery({
    queryKey: ['stock-reservations', params],
    queryFn: () => reservationService.list(params),
  });
};

export const useCancelReservation = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ reservationId, data }: { reservationId: string; data: CancelReservationRequest }) =>
      reservationService.cancel(reservationId, data),
    onSuccess: (data) => {
      invalidateVehicleQueries(queryClient, data?.vehicleId);
    },
  });
};

export const useExtendReservation = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ reservationId, data }: { reservationId: string; data: ExtendReservationRequest }) =>
      reservationService.extend(reservationId, data),
    onSuccess: (data) => {
      invalidateVehicleQueries(queryClient, data?.vehicleId);
    },
  });
};
