import { commercialApi } from '../../../lib/api';
import type {
  AddProposalItemRequest,
  ApplyDiscountRequest,
  CreateProposalRequest,
  PagedResponse,
  Proposal,
  ProposalListItem,
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
    const response = await commercialApi.get<PagedResponse<ProposalListItem>>(BASE_URL, { params });
    return response.data;
  },

  getById: async (id: string) => {
    const response = await commercialApi.get<Proposal>(`${BASE_URL}/${id}`);
    return response.data;
  },

  create: async (data: CreateProposalRequest) => {
    const response = await commercialApi.post<Proposal>(BASE_URL, data);
    return response.data;
  },

  update: async (id: string, data: UpdateProposalRequest) => {
    const response = await commercialApi.put<Proposal>(`${BASE_URL}/${id}`, data);
    return response.data;
  },

  addItem: async (id: string, data: AddProposalItemRequest) => {
    const response = await commercialApi.post<Proposal>(`${BASE_URL}/${id}/items`, data);
    return response.data;
  },

  removeItem: async (id: string, itemId: string) => {
    const response = await commercialApi.delete<Proposal>(`${BASE_URL}/${id}/items/${itemId}`);
    return response.data;
  },

  applyDiscount: async (id: string, data: ApplyDiscountRequest) => {
    const response = await commercialApi.post<Proposal>(`${BASE_URL}/${id}/discount`, data);
    return response.data;
  },

  getPendingApprovals: async () => {
    const response = await commercialApi.get<Proposal[]>(`${BASE_URL}/pending-approval`);
    return response.data;
  },

  approveDiscount: async (id: string) => {
    const response = await commercialApi.post<Proposal>(`${BASE_URL}/${id}/approve-discount`);
    return response.data;
  },

  rejectDiscount: async (id: string) => {
    const response = await commercialApi.post<Proposal>(`${BASE_URL}/${id}/reject-discount`);
    return response.data;
  },

  close: async (id: string) => {
    const response = await commercialApi.post<Proposal>(`${BASE_URL}/${id}/close`);
    return response.data;
  },
};
