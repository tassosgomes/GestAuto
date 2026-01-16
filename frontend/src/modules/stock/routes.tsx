import { lazy } from 'react'
import type { RouteObject } from 'react-router-dom'
import { RequireRoles } from '@/auth/RequireRoles'

const StockLayout = lazy(() =>
  import('./StockLayout').then((module) => ({ default: module.StockLayout }))
)
const StockDashboardPage = lazy(() =>
  import('./pages/StockDashboardPage').then((module) => ({ default: module.StockDashboardPage }))
)
const StockVehiclesPage = lazy(() =>
  import('./pages/StockVehiclesPage').then((module) => ({ default: module.StockVehiclesPage }))
)
const StockVehicleDetailsPage = lazy(() =>
  import('./pages/StockVehicleDetailsPage').then((module) => ({ default: module.StockVehicleDetailsPage }))
)
const StockReservationsPage = lazy(() =>
  import('./pages/StockReservationsPage').then((module) => ({ default: module.StockReservationsPage }))
)
const StockMovementsPage = lazy(() =>
  import('./pages/StockMovementsPage').then((module) => ({ default: module.StockMovementsPage }))
)
const StockTestDrivesPage = lazy(() =>
  import('./pages/StockTestDrivesPage').then((module) => ({ default: module.StockTestDrivesPage }))
)
const StockPreparationPage = lazy(() =>
  import('./pages/StockPreparationPage').then((module) => ({ default: module.StockPreparationPage }))
)
const StockFinancePage = lazy(() =>
  import('./pages/StockFinancePage').then((module) => ({ default: module.StockFinancePage }))
)
const StockWriteOffsPage = lazy(() =>
  import('./pages/StockWriteOffsPage').then((module) => ({ default: module.StockWriteOffsPage }))
)

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
