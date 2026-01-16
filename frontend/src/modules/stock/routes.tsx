import type { RouteObject } from 'react-router-dom'

import { StockLayout } from './StockLayout'
import { StockDashboardPage } from './pages/StockDashboardPage'
import { StockVehiclesPage } from './pages/StockVehiclesPage'
import { StockVehicleDetailsPage } from './pages/StockVehicleDetailsPage'
import { StockReservationsPage } from './pages/StockReservationsPage'
import { StockMovementsPage } from './pages/StockMovementsPage'
import { StockTestDrivesPage } from './pages/StockTestDrivesPage'
import { StockPreparationPage } from './pages/StockPreparationPage'
import { StockFinancePage } from './pages/StockFinancePage'
import { StockWriteOffsPage } from './pages/StockWriteOffsPage'
import { RequireRoles } from '@/auth/RequireRoles'

export const stockRoutes: RouteObject = {
  path: 'stock',
  element: <StockLayout />,
  children: [
    {
      index: true,
      element: <StockDashboardPage />,
    },
    {
      path: 'vehicles',
      element: <StockVehiclesPage />,
    },
    {
      path: 'vehicles/:id',
      element: <StockVehicleDetailsPage />,
    },
    {
      path: 'reservations',
      element: <StockReservationsPage />,
    },
    {
      path: 'movements',
      element: <StockMovementsPage />,
    },
    {
      path: 'test-drives',
      element: <StockTestDrivesPage />,
    },
    {
      path: 'preparation',
      element: (
        <RequireRoles roles={['ADMIN', 'MANAGER', 'STOCK_MANAGER']}>
          <StockPreparationPage />
        </RequireRoles>
      ),
    },
    {
      path: 'finance',
      element: (
        <RequireRoles roles={['ADMIN', 'MANAGER', 'STOCK_MANAGER']}>
          <StockFinancePage />
        </RequireRoles>
      ),
    },
    {
      path: 'write-offs',
      element: (
        <RequireRoles roles={['ADMIN', 'MANAGER', 'STOCK_MANAGER']}>
          <StockWriteOffsPage />
        </RequireRoles>
      ),
    },
  ],
}
