import axios from 'axios';
import { api } from '../../../lib/api';
import type { CompleteTestDriveRequest, CompleteTestDriveResponse } from '../types';
import { handleProblemDetailsError } from './problemDetails';

const BASE_URL = '/test-drives';

const getNonVersionedBaseUrl = () => {
  const baseUrl = api.defaults.baseURL ?? '';
  return baseUrl.replace(/\/api\/v1\/?$/i, '');
};

export const testDriveService = {
  complete: async (testDriveId: string, data: CompleteTestDriveRequest) => {
    try {
      const response = await api.post<CompleteTestDriveResponse>(`${BASE_URL}/${testDriveId}/complete`, data);
      return response.data;
    } catch (error) {
      if (axios.isAxiosError(error) && error.response?.status === 404) {
        try {
          const fallbackBaseUrl = getNonVersionedBaseUrl();
          const response = await api.post<CompleteTestDriveResponse>(`${BASE_URL}/${testDriveId}/complete`, data, {
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
