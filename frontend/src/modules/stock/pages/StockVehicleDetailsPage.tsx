import { useParams } from 'react-router-dom'
import { useVehicle, useVehicleHistory } from '../hooks/useVehicles'

export function StockVehicleDetailsPage() {
  const { id } = useParams()
  const vehicleQuery = useVehicle(id)
  const historyQuery = useVehicleHistory(id)
  const isLoading = vehicleQuery.isLoading || historyQuery.isLoading
  const error = vehicleQuery.error ?? historyQuery.error
  const historyCount = historyQuery.data?.items.length ?? 0

  return (
    <div className="p-6">
      <h1 className="text-2xl font-semibold">Detalhe do veículo</h1>
      <p className="text-muted-foreground">ID: {id}</p>
      <p className="text-muted-foreground">Ficha técnica + histórico (placeholder)</p>
      {isLoading && <p className="text-muted-foreground mt-4">Carregando dados...</p>}
      {error && (
        <p className="text-sm text-red-500 mt-4">
          {error instanceof Error ? error.message : 'Falha ao carregar veículo'}
        </p>
      )}
      {!isLoading && !error && (
        <div className="mt-4 space-y-1 text-sm text-muted-foreground">
          <p>VIN: {vehicleQuery.data?.vin ?? '—'}</p>
          <p>Histórico: {historyCount} eventos</p>
        </div>
      )}
    </div>
  )
}
