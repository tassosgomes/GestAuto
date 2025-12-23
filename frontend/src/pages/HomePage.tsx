import { useAppConfig } from '../config/useAppConfig'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'

export function HomePage() {
  const cfg = useAppConfig()

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold tracking-tight">Bem-vindo ao GestAuto</h1>
        <p className="text-muted-foreground">Selecione uma opção no menu lateral para começar.</p>
      </div>

      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
        <Card>
          <CardHeader>
            <CardTitle>Configuração Runtime</CardTitle>
          </CardHeader>
          <CardContent>
            <pre className="text-xs bg-muted p-4 rounded-md overflow-auto">
              {JSON.stringify(cfg, null, 2)}
            </pre>
          </CardContent>
        </Card>
      </div>
    </div>
  )
}
