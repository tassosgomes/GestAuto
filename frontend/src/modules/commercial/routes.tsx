import type { RouteObject } from 'react-router-dom';
import { CommercialLayout } from './CommercialLayout';
import { DashboardPage } from './pages/DashboardPage';
import { LeadListPage } from './pages/LeadListPage';
import { ProposalListPage } from './pages/ProposalListPage';

export const commercialRoutes: RouteObject = {
  path: 'commercial',
  element: <CommercialLayout />,
  children: [
    {
      index: true,
      element: <DashboardPage />,
    },
    {
      path: 'leads',
      element: <LeadListPage />,
    },
    {
      path: 'proposals',
      element: <ProposalListPage />,
    },
  ],
};
