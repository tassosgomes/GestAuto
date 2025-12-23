import { api } from '../../../lib/api';
import type {
  CreateLeadRequest,
  Lead,
  PagedResponse,
  QualifyLeadRequest,
  RegisterInteractionRequest,
  UpdateLeadRequest,
} from '../types';

const BASE_URL = '/leads';

export const leadService = {
  getAll: async (params?: {
    page?: number;
    pageSize?: number;
    status?: string;
    score?: string;
    search?: string;
  }) => {
    const response = await api.get<PagedResponse<Lead>>(BASE_URL, { params });
    return response.data;
  },

  getById: async (id: string) => {
    const response = await api.get<Lead>(`${BASE_URL}/${id}`);
    return response.data;
  },

  create: async (data: CreateLeadRequest) => {
    const response = await api.post<Lead>(BASE_URL, data);
    return response.data;
  },

  update: async (id: string, data: UpdateLeadRequest) => {
    const response = await api.put<Lead>(`${BASE_URL}/${id}`, data);
    return response.data;
  },

  qualify: async (id: string, data: QualifyLeadRequest) => {
    const response = await api.post<Lead>(`${BASE_URL}/${id}/qualify`, data);
    return response.data;
  },

  registerInteraction: async (id: string, data: RegisterInteractionRequest) => {
    const response = await api.post<void>(`${BASE_URL}/${id}/interactions`, data);
    return response.data;
  },
};
