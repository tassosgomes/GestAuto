import type { RouteObject } from 'react-router-dom';
import { Navigate, useParams } from 'react-router-dom';
import { CommercialLayout } from './CommercialLayout';
import { DashboardPage } from './pages/DashboardPage';
import { LeadListPage } from './pages/LeadListPage';
import { LeadDetailsPage } from './pages/LeadDetailsPage';
import { ProposalListPage } from './pages/ProposalListPage';
import { ProposalEditorPage } from './pages/ProposalEditorPage';
import { TestDrivePage } from './pages/TestDrivePage';
import { ProposalApprovalPage } from './pages/ProposalApprovalPage';
import { PipelinePage } from './pages/PipelinePage';
import { RequireRoles } from '@/auth/RequireRoles';

function ProposalIdRedirect() {
  const { id } = useParams();
  return <Navigate to={`/commercial/proposals/${id}/edit`} replace />;
}

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
      path: 'leads/new',
      element: <Navigate to="/commercial/leads?create=1" replace />,
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
      path: 'proposals/:id/edit',
      element: <ProposalEditorPage />,
    },
    {
      path: 'proposals/:id',
      element: <ProposalIdRedirect />,
    },
    {
      path: 'test-drives',
      element: <TestDrivePage />,
    },
    {
      path: 'approvals',
      element: (
        <RequireRoles roles={["ADMIN", "MANAGER", "SALES_MANAGER"]}>
          <ProposalApprovalPage />
        </RequireRoles>
      ),
    },
    {
      path: 'pipeline',
      element: (
        <RequireRoles roles={["ADMIN", "MANAGER", "SALES_MANAGER"]}>
          <PipelinePage />
        </RequireRoles>
      ),
    },
  ],
};
