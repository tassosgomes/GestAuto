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

// Mock data
const MOCK_DASHBOARD_DATA: DashboardData = {
  kpis: {
    newLeads: 12,
    openProposals: 5,
    testDrivesToday: 3,
    conversionRate: 15.4,
  },
  hotLeads: [
    {
      id: '1',
      name: 'Roberto Carlos',
      status: 'New',
      score: 'Diamond',
      salesPersonId: 'user-1',
      createdAt: new Date(Date.now() - 86400000 * 2).toISOString(), // 2 days ago
      phone: '(11) 99999-9999',
      email: 'roberto@example.com',
      source: 'Instagram'
    },
    {
      id: '2',
      name: 'Ana Maria',
      status: 'Contacted',
      score: 'Gold',
      salesPersonId: 'user-1',
      createdAt: new Date(Date.now() - 86400000 * 5).toISOString(), // 5 days ago
      phone: '(11) 88888-8888',
      email: 'ana@example.com',
      source: 'Google'
    }
  ],
  pendingActions: []
};

export const dashboardService = {
  getDashboardData: async (): Promise<DashboardData> => {
    // const { data } = await api.get<DashboardData>('/commercial/dashboard');
    // return data;
    return new Promise((resolve) => {
      setTimeout(() => resolve(MOCK_DASHBOARD_DATA), 500);
    });
  }
};
