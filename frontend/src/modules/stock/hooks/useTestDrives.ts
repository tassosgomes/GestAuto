import { QueryClient, useMutation, useQueryClient } from '@tanstack/react-query';
import { testDriveService } from '../services/testDriveService';
import type { CompleteTestDriveRequest } from '../types';

const invalidateVehicleQueries = (queryClient: QueryClient, vehicleId?: string) => {
  queryClient.invalidateQueries({ queryKey: ['stock-vehicles'] });

  if (vehicleId) {
    queryClient.invalidateQueries({ queryKey: ['stock-vehicle', vehicleId] });
    queryClient.invalidateQueries({ queryKey: ['stock-vehicle-history', vehicleId] });
  }
};

export const useCompleteTestDrive = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ testDriveId, data }: { testDriveId: string; data: CompleteTestDriveRequest }) =>
      testDriveService.complete(testDriveId, data),
    onSuccess: (data) => {
      invalidateVehicleQueries(queryClient, data?.vehicleId);
    },
  });
};
