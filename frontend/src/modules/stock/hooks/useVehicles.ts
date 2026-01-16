import { QueryClient, useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { vehicleService } from '../services/vehicleService';
import type {
  ChangeVehicleStatusRequest,
  CheckInCreateRequest,
  CheckOutCreateRequest,
  StartTestDriveRequest,
} from '../types';

const invalidateVehicleQueries = (queryClient: QueryClient, vehicleId?: string) => {
  queryClient.invalidateQueries({ queryKey: ['stock-vehicles'] });

  if (vehicleId) {
    queryClient.invalidateQueries({ queryKey: ['stock-vehicle', vehicleId] });
    queryClient.invalidateQueries({ queryKey: ['stock-vehicle-history', vehicleId] });
  }
};

export const useVehiclesList = (params?: {
  page?: number;
  size?: number;
  status?: string;
  category?: string;
  q?: string;
  includeCompatPagination?: boolean;
}) => {
  return useQuery({
    queryKey: ['stock-vehicles', params],
    queryFn: () => vehicleService.list(params),
  });
};

export const useVehicle = (id?: string) => {
  return useQuery({
    queryKey: ['stock-vehicle', id],
    queryFn: () => vehicleService.getById(id ?? ''),
    enabled: !!id,
  });
};

export const useVehicleHistory = (id?: string) => {
  return useQuery({
    queryKey: ['stock-vehicle-history', id],
    queryFn: () => vehicleService.getHistory(id ?? ''),
    enabled: !!id,
  });
};

export const useChangeVehicleStatus = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: ChangeVehicleStatusRequest }) =>
      vehicleService.changeStatus(id, data),
    onSuccess: (_, variables) => {
      invalidateVehicleQueries(queryClient, variables.id);
    },
  });
};

export const useCheckInVehicle = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: CheckInCreateRequest }) => vehicleService.checkIn(id, data),
    onSuccess: (data, variables) => {
      invalidateVehicleQueries(queryClient, data?.vehicleId ?? variables.id);
    },
  });
};

export const useCheckOutVehicle = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: CheckOutCreateRequest }) =>
      vehicleService.checkOut(id, data),
    onSuccess: (data, variables) => {
      invalidateVehicleQueries(queryClient, data?.vehicleId ?? variables.id);
    },
  });
};

export const useStartTestDrive = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: StartTestDriveRequest }) =>
      vehicleService.startTestDrive(id, data),
    onSuccess: (data, variables) => {
      invalidateVehicleQueries(queryClient, data?.vehicleId ?? variables.id);
    },
  });
};
