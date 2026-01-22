import { commercialApi } from '../../../lib/api';
import type {
  CancelTestDriveRequest,
  CompleteTestDriveRequest,
  PagedResponse,
  ScheduleTestDriveRequest,
  TestDrive,
} from '../types';

const BASE_URL = '/test-drives';

const compactParams = (params: Record<string, unknown>) =>
  Object.fromEntries(
    Object.entries(params).filter(([, value]) => value !== undefined && value !== null && value !== ''),
  );

export const testDriveService = {
  getAll: async (params?: {
    page?: number;
    pageSize?: number;
    status?: string;
    leadId?: string;
    from?: string;
    to?: string;
  }) => {
    const safeParams = compactParams({
      page: params?.page ?? 1,
      pageSize: params?.pageSize ?? 20,
      status: params?.status,
      leadId: params?.leadId,
      from: params?.from,
      to: params?.to,
    });

    const response = await commercialApi.get<PagedResponse<TestDrive>>(BASE_URL, { params: safeParams });
    return response.data;
  },

  getById: async (id: string) => {
    const response = await commercialApi.get<TestDrive>(`${BASE_URL}/${id}`);
    return response.data;
  },

  schedule: async (data: ScheduleTestDriveRequest) => {
    const response = await commercialApi.post<TestDrive>(BASE_URL, data);
    return response.data;
  },

  complete: async (id: string, data: CompleteTestDriveRequest) => {
    const response = await commercialApi.post<TestDrive>(`${BASE_URL}/${id}/complete`, data);
    return response.data;
  },

  cancel: async (id: string, data: CancelTestDriveRequest) => {
    const response = await commercialApi.post<TestDrive>(`${BASE_URL}/${id}/cancel`, data);
    return response.data;
  },
};
