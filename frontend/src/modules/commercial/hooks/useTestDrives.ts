import { useQuery } from '@tanstack/react-query';
import { testDriveService } from '../services/testDriveService';

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
    queryKey: ['test-drives', params],
    queryFn: () => testDriveService.getAll(params),
    enabled: options?.enabled ?? true,
    retry: false,
  });
};
