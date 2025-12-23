import type { RouteObject } from 'react-router-dom';
import { CommercialLayout } from './CommercialLayout';
import { DashboardPage } from './pages/DashboardPage';
import { LeadListPage } from './pages/LeadListPage';
import { LeadDetailsPage } from './pages/LeadDetailsPage';
import { ProposalListPage } from './pages/ProposalListPage';
import { ProposalEditorPage } from './pages/ProposalEditorPage';

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
      path: 'leads/:id',
      element: <LeadDetailsPage />,
    },
    {
      path: 'proposals',
      element: <ProposalListPage />,
    },
    {
      path: 'proposals/new',
      element: <ProposalEditorPage />,
    },
  ],
};
