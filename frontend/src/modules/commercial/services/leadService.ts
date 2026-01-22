import { commercialApi } from '../../../lib/api';
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
    createdFrom?: string;
    createdTo?: string;
    salesPersonId?: string;
  }) => {
    const response = await commercialApi.get<PagedResponse<Lead>>(BASE_URL, { params });
    return response.data;
  },

  getById: async (id: string) => {
    const response = await commercialApi.get<Lead>(`${BASE_URL}/${id}`);
    return response.data;
  },

  create: async (data: CreateLeadRequest) => {
    const response = await commercialApi.post<Lead>(BASE_URL, data);
    return response.data;
  },

  update: async (id: string, data: UpdateLeadRequest) => {
    const response = await commercialApi.put<Lead>(`${BASE_URL}/${id}`, data);
    return response.data;
  },

  qualify: async (id: string, data: QualifyLeadRequest) => {
    const response = await commercialApi.post<Lead>(`${BASE_URL}/${id}/qualify`, data);
    return response.data;
  },

  registerInteraction: async (id: string, data: RegisterInteractionRequest) => {
    const response = await commercialApi.post<void>(`${BASE_URL}/${id}/interactions`, data);
    return response.data;
  },
};
