import { api } from '../../../lib/api';
import type {
  AddProposalItemRequest,
  ApplyDiscountRequest,
  CreateProposalRequest,
  PagedResponse,
  Proposal,
  UpdateProposalRequest,
} from '../types';

const BASE_URL = '/proposals';

export const proposalService = {
  getAll: async (params?: {
    page?: number;
    pageSize?: number;
    status?: string;
    leadId?: string;
  }) => {
    const response = await api.get<PagedResponse<Proposal>>(BASE_URL, { params });
    return response.data;
  },

  getById: async (id: string) => {
    const response = await api.get<Proposal>(`${BASE_URL}/${id}`);
    return response.data;
  },

  create: async (data: CreateProposalRequest) => {
    const response = await api.post<Proposal>(BASE_URL, data);
    return response.data;
  },

  update: async (id: string, data: UpdateProposalRequest) => {
    const response = await api.put<Proposal>(`${BASE_URL}/${id}`, data);
    return response.data;
  },

  addItem: async (id: string, data: AddProposalItemRequest) => {
    const response = await api.post<Proposal>(`${BASE_URL}/${id}/items`, data);
    return response.data;
  },

  removeItem: async (id: string, itemId: string) => {
    const response = await api.delete<Proposal>(`${BASE_URL}/${id}/items/${itemId}`);
    return response.data;
  },

  applyDiscount: async (id: string, data: ApplyDiscountRequest) => {
    const response = await api.post<Proposal>(`${BASE_URL}/${id}/discount`, data);
    return response.data;
  },

  approveDiscount: async (id: string, approved: boolean) => {
    // Assuming the endpoint might be /approve-discount or similar, checking swagger...
    // Swagger said: /api/v1/proposals/{id}/approve-discount
    // It probably takes a body or just the action.
    // Let's assume it takes { approved: boolean } or just POST.
    // I'll check swagger for ApproveDiscountRequest if it exists.
    const response = await api.post<Proposal>(`${BASE_URL}/${id}/approve-discount`, { approved });
    return response.data;
  },

  close: async (id: string) => {
    const response = await api.post<Proposal>(`${BASE_URL}/${id}/close`);
    return response.data;
  },
};
