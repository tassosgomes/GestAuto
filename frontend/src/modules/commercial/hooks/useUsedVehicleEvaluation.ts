import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { usedVehicleEvaluationService } from '../services/usedVehicleEvaluationService'
import type {
  RequestUsedVehicleEvaluationRequest,
  UsedVehicleEvaluationCustomerResponseRequest,
} from '../types'

export const useUsedVehicleEvaluation = (id?: string) =>
  useQuery({
    queryKey: ['usedVehicleEvaluation', id],
    queryFn: () => usedVehicleEvaluationService.getById(id!),
    enabled: !!id,
    retry: false,
  })

export const useRequestUsedVehicleEvaluation = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (data: RequestUsedVehicleEvaluationRequest) => usedVehicleEvaluationService.request(data),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['usedVehicleEvaluation', data.id] })
      queryClient.invalidateQueries({ queryKey: ['proposal', data.proposalId] })
    },
  })
}

export const useUsedVehicleEvaluationCustomerResponse = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UsedVehicleEvaluationCustomerResponseRequest }) =>
      usedVehicleEvaluationService.customerResponse(id, data),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['usedVehicleEvaluation', data.id] })
      queryClient.invalidateQueries({ queryKey: ['proposal', data.proposalId] })
    },
  })
}
