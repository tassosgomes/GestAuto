import { commercialApi } from '../../../lib/api';
import type { Lead, Proposal } from '../types';

export interface DashboardKPIs {
  newLeads: number;
  openProposals: number;
  testDrivesToday: number;
  conversionRate: number;
}

export interface DashboardData {
  kpis: DashboardKPIs;
  hotLeads: Lead[];
  pendingActions: Proposal[];
}

const BASE_URL = '/dashboard';

export const dashboardService = {
  getDashboardData: async (): Promise<DashboardData> => {
    const response = await commercialApi.get<DashboardData>(BASE_URL);
    return response.data;
  }
};
