import { useQuery } from '@tanstack/react-query';
import { api } from '../../../lib/api';
import type { PaymentMethod } from '../types';

/**
 * Hook para buscar as formas de pagamento ativas do backend
 * Retorna apenas as formas de pagamento ativas, ordenadas por displayOrder
 */
export function usePaymentMethods() {
  return useQuery<PaymentMethod[]>({
    queryKey: ['payment-methods'],
    queryFn: async () => {
      const response = await api.get('/payment-methods');
      return response.data;
    },
    staleTime: 5 * 60 * 1000, // Cache por 5 minutos
    retry: 2,
    refetchOnWindowFocus: false,
  });
}
