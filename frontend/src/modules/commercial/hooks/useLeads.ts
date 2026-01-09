import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { leadService } from '../services/leadService';
import type {
  CreateLeadRequest,
  UpdateLeadRequest,
  QualifyLeadRequest,
  RegisterInteractionRequest,
} from '../types';

export const useLeads = (params?: {
  page?: number;
  pageSize?: number;
  status?: string;
  score?: string;
  search?: string;
  createdFrom?: string;
  createdTo?: string;
  salesPersonId?: string;
}) => {
  return useQuery({
    queryKey: ['leads', params],
    queryFn: () => leadService.getAll(params),
  });
};

export const useLead = (id: string) => {
  return useQuery({
    queryKey: ['lead', id],
    queryFn: () => leadService.getById(id),
    enabled: !!id,
  });
};

export const useCreateLead = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateLeadRequest) => leadService.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['leads'] });
    },
  });
};

export const useUpdateLead = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateLeadRequest }) =>
      leadService.update(id, data),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['leads'] });
      queryClient.invalidateQueries({ queryKey: ['lead', data.id] });
    },
  });
};

export const useQualifyLead = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: QualifyLeadRequest }) =>
      leadService.qualify(id, data),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['lead', data.id] });
      queryClient.invalidateQueries({ queryKey: ['leads'] });
    },
  });
};

export const useRegisterInteraction = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      id,
      data,
    }: {
      id: string;
      data: RegisterInteractionRequest;
    }) => leadService.registerInteraction(id, data),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['lead', variables.id] });
    },
  });
};
