import { lazy } from 'react';
import type { RouteObject } from 'react-router-dom';
import { Navigate, useParams } from 'react-router-dom';
import { RequireRoles } from '@/auth/RequireRoles';

const CommercialLayout = lazy(() =>
  import('./CommercialLayout').then((module) => ({ default: module.CommercialLayout }))
);
const DashboardPage = lazy(() =>
  import('./pages/DashboardPage').then((module) => ({ default: module.DashboardPage }))
);
const LeadListPage = lazy(() =>
  import('./pages/LeadListPage').then((module) => ({ default: module.LeadListPage }))
);
const LeadDetailsPage = lazy(() =>
  import('./pages/LeadDetailsPage').then((module) => ({ default: module.LeadDetailsPage }))
);
const ProposalListPage = lazy(() =>
  import('./pages/ProposalListPage').then((module) => ({ default: module.ProposalListPage }))
);
const ProposalEditorPage = lazy(() =>
  import('./pages/ProposalEditorPage').then((module) => ({ default: module.ProposalEditorPage }))
);
const TestDrivePage = lazy(() =>
  import('./pages/TestDrivePage').then((module) => ({ default: module.TestDrivePage }))
);
const ProposalApprovalPage = lazy(() =>
  import('./pages/ProposalApprovalPage').then((module) => ({ default: module.ProposalApprovalPage }))
);
const PipelinePage = lazy(() =>
  import('./pages/PipelinePage').then((module) => ({ default: module.PipelinePage }))
);

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
