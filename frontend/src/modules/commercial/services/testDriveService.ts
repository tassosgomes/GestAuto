import { api } from '../../../lib/api';
import type {
  CancelTestDriveRequest,
  CompleteTestDriveRequest,
  PagedResponse,
  ScheduleTestDriveRequest,
  TestDrive,
} from '../types';

const BASE_URL = '/test-drives';

export const testDriveService = {
  getAll: async (params?: {
    page?: number;
    pageSize?: number;
    status?: string;
    leadId?: string;
    from?: string;
    to?: string;
  }) => {
    const response = await api.get<PagedResponse<TestDrive>>(BASE_URL, { params });
    return response.data;
  },

  getById: async (id: string) => {
    const response = await api.get<TestDrive>(`${BASE_URL}/${id}`);
    return response.data;
  },

  schedule: async (data: ScheduleTestDriveRequest) => {
    const response = await api.post<TestDrive>(BASE_URL, data);
    return response.data;
  },

  complete: async (id: string, data: CompleteTestDriveRequest) => {
    const response = await api.post<TestDrive>(`${BASE_URL}/${id}/complete`, data);
    return response.data;
  },

  cancel: async (id: string, data: CancelTestDriveRequest) => {
    const response = await api.post<TestDrive>(`${BASE_URL}/${id}/cancel`, data);
    return response.data;
  },
};
