import { QueryClient, useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { testDriveService } from '../services/testDriveService';
import type { CompleteTestDriveRequest } from '../types';

const invalidateVehicleQueries = (queryClient: QueryClient, vehicleId?: string) => {
  queryClient.invalidateQueries({ queryKey: ['stock-vehicles'] });

  if (vehicleId) {
    queryClient.invalidateQueries({ queryKey: ['stock-vehicle', vehicleId] });
    queryClient.invalidateQueries({ queryKey: ['stock-vehicle-history', vehicleId] });
  }
};

const invalidateTestDriveQueries = (queryClient: QueryClient, testDriveId?: string) => {
  queryClient.invalidateQueries({ queryKey: ['stock-test-drives'] });

  if (testDriveId) {
    queryClient.invalidateQueries({ queryKey: ['stock-test-drive', testDriveId] });
  }
};

export const useTestDrives = (
  params?: {
    page?: number;
    pageSize?: number;
    status?: string;
    leadId?: string;
    from?: string;
    to?: string;
  },
  options?: { enabled?: boolean }
) => {
  return useQuery({
    queryKey: ['stock-test-drives', params],
    queryFn: () => testDriveService.list(params),
    enabled: options?.enabled ?? true,
    retry: false,
  });
};

export const useTestDrive = (id?: string) => {
  return useQuery({
    queryKey: ['stock-test-drive', id],
    queryFn: () => testDriveService.getById(id!),
    enabled: !!id,
    retry: false,
  });
};

export const useCompleteTestDrive = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ testDriveId, data }: { testDriveId: string; data: CompleteTestDriveRequest }) =>
      testDriveService.complete(testDriveId, data),
    onSuccess: (data) => {
      invalidateVehicleQueries(queryClient, data?.vehicleId);
      invalidateTestDriveQueries(queryClient, data?.testDriveId);
    },
  });
};
