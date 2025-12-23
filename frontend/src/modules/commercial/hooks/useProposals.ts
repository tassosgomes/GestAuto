import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { proposalService } from '../services/proposalService';
import type { CreateProposalRequest, UpdateProposalRequest } from '../types';

export const useProposals = (params?: {
  page?: number;
  pageSize?: number;
  status?: string;
  leadId?: string;
}) => {
  return useQuery({
    queryKey: ['proposals', params],
    queryFn: () => proposalService.getAll(params),
  });
};

export const useProposal = (id: string) => {
  return useQuery({
    queryKey: ['proposal', id],
    queryFn: () => proposalService.getById(id),
    enabled: !!id,
  });
};

export const useCreateProposal = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateProposalRequest) => proposalService.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['proposals'] });
    },
  });
};

export const useUpdateProposal = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateProposalRequest }) =>
      proposalService.update(id, data),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['proposals'] });
      queryClient.invalidateQueries({ queryKey: ['proposal', data.id] });
    },
  });
};
