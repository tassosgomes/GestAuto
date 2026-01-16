import { useParams } from 'react-router-dom'

export function StockVehicleDetailsPage() {
  const { id } = useParams()

  return (
    <div className="p-6">
      <h1 className="text-2xl font-semibold">Detalhe do veículo</h1>
      <p className="text-muted-foreground">ID: {id}</p>
      <p className="text-muted-foreground">Ficha técnica + histórico (placeholder)</p>
    </div>
  )
}
