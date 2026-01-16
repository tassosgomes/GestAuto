import { Outlet } from 'react-router-dom'

export function StockLayout() {
  return (
    <div className="flex flex-col h-full">
      <div className="flex-1 overflow-auto">
        <Outlet />
      </div>
    </div>
  )
}
