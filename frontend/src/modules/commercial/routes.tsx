import type { RouteObject } from 'react-router-dom';
import { CommercialLayout } from './CommercialLayout';
import { DashboardPage } from './pages/DashboardPage';
import { LeadListPage } from './pages/LeadListPage';
import { LeadDetailsPage } from './pages/LeadDetailsPage';
import { ProposalListPage } from './pages/ProposalListPage';
import { ProposalEditorPage } from './pages/ProposalEditorPage';
import { TestDrivePage } from './pages/TestDrivePage';
import { ProposalApprovalPage } from './pages/ProposalApprovalPage';
import { PipelinePage } from './pages/PipelinePage';

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
    {
      path: 'test-drives',
      element: <TestDrivePage />,
    },
    {
      path: 'approvals',
      element: <ProposalApprovalPage />,
    },
    {
      path: 'pipeline',
      element: <PipelinePage />,
    },
  ],
};
