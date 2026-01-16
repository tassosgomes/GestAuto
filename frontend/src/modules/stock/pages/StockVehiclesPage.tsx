import { useVehiclesList } from '../hooks/useVehicles'

export function StockVehiclesPage() {
  const { data, isLoading, isError, error } = useVehiclesList({ page: 1, size: 10 })
  const total = data?.pagination.total ?? 0

  return (
    <div className="p-6">
      <h1 className="text-2xl font-semibold">Veículos</h1>
      <p className="text-muted-foreground">Listagem de veículos (placeholder)</p>
      {isLoading && <p className="text-muted-foreground mt-4">Carregando veículos...</p>}
      {isError && (
        <p className="text-sm text-red-500 mt-4">
          {error instanceof Error ? error.message : 'Falha ao carregar veículos'}
        </p>
      )}
      {!isLoading && !isError && (
        <p className="text-muted-foreground mt-4">Total de veículos: {total}</p>
      )}
    </div>
  )
}
