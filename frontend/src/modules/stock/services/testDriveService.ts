import axios from 'axios';
import { stockApi } from '../../../lib/api';
import type { 
  CompleteTestDriveRequest, 
  CompleteTestDriveResponse,
  PagedResponse,
  TestDriveListItem,
  TestDriveDetails
} from '../types';
import { handleProblemDetailsError } from './problemDetails';

const BASE_URL = '/test-drives';

const getNonVersionedBaseUrl = () => {
  const baseUrl = stockApi.defaults.baseURL ?? '';
  return baseUrl.replace(/\/api\/v1\/?$/i, '');
};

const compactParams = (params: Record<string, unknown>) =>
  Object.fromEntries(
    Object.entries(params).filter(([, value]) => value !== undefined && value !== null && value !== ''),
  );

export const testDriveService = {
  list: async (params?: {
    page?: number;
    pageSize?: number;
    status?: string;
    leadId?: string;
    from?: string;
    to?: string;
  }) => {
    try {
      const safeParams = compactParams({
        page: params?.page ?? 1,
        pageSize: params?.pageSize ?? 20,
        status: params?.status,
        leadId: params?.leadId,
        from: params?.from,
        to: params?.to,
      });

      const response = await stockApi.get<PagedResponse<TestDriveListItem>>(BASE_URL, { params: safeParams });
      return response.data;
    } catch (error) {
      if (axios.isAxiosError(error) && error.response?.status === 404) {
        try {
          const fallbackBaseUrl = getNonVersionedBaseUrl();
          const response = await stockApi.get<PagedResponse<TestDriveListItem>>(BASE_URL, {
            baseURL: fallbackBaseUrl,
            params: compactParams({
              page: params?.page ?? 1,
              pageSize: params?.pageSize ?? 20,
              status: params?.status,
              leadId: params?.leadId,
              from: params?.from,
              to: params?.to,
            }),
          });
          return response.data;
        } catch (fallbackError) {
          handleProblemDetailsError(fallbackError, 'Falha ao carregar test-drives');
        }
      }
      handleProblemDetailsError(error, 'Falha ao carregar test-drives');
    }
  },

  getById: async (id: string) => {
    try {
      const response = await stockApi.get<TestDriveDetails>(`${BASE_URL}/${id}`);
      return response.data;
    } catch (error) {
      if (axios.isAxiosError(error) && error.response?.status === 404) {
        try {
          const fallbackBaseUrl = getNonVersionedBaseUrl();
          const response = await stockApi.get<TestDriveDetails>(`${BASE_URL}/${id}`, {
            baseURL: fallbackBaseUrl,
          });
          return response.data;
        } catch (fallbackError) {
          handleProblemDetailsError(fallbackError, 'Falha ao carregar test-drive');
        }
      }
      handleProblemDetailsError(error, 'Falha ao carregar test-drive');
    }
  },

  complete: async (testDriveId: string, data: CompleteTestDriveRequest) => {
    try {
      const response = await stockApi.post<CompleteTestDriveResponse>(`${BASE_URL}/${testDriveId}/complete`, data);
      return response.data;
    } catch (error) {
      if (axios.isAxiosError(error) && error.response?.status === 404) {
        try {
          const fallbackBaseUrl = getNonVersionedBaseUrl();
          const response = await stockApi.post<CompleteTestDriveResponse>(`${BASE_URL}/${testDriveId}/complete`, data, {
            baseURL: fallbackBaseUrl,
          });
          return response.data;
        } catch (fallbackError) {
          handleProblemDetailsError(fallbackError, 'Falha ao finalizar test-drive');
        }
      }

      handleProblemDetailsError(error, 'Falha ao finalizar test-drive');
    }
  },
};
