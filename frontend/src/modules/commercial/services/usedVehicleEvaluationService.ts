import { commercialApi } from '../../../lib/api'
import type {
  RequestUsedVehicleEvaluationRequest,
  UsedVehicleEvaluation,
  UsedVehicleEvaluationCustomerResponseRequest,
} from '../types'

const BASE_URL = '/used-vehicle-evaluations'

export const usedVehicleEvaluationService = {
  request: async (data: RequestUsedVehicleEvaluationRequest) => {
    const response = await commercialApi.post<UsedVehicleEvaluation>(BASE_URL, data)
    return response.data
  },

  getById: async (id: string) => {
    const response = await commercialApi.get<UsedVehicleEvaluation>(`${BASE_URL}/${id}`)
    return response.data
  },

  customerResponse: async (id: string, data: UsedVehicleEvaluationCustomerResponseRequest) => {
    const response = await commercialApi.post<UsedVehicleEvaluation>(`${BASE_URL}/${id}/customer-response`, data)
    return response.data
  },
}
