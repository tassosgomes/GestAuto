import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"

export function AdminPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold tracking-tight">Administração</h1>
        <p className="text-muted-foreground">Gerenciamento do sistema.</p>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Painel Administrativo</CardTitle>
        </CardHeader>
        <CardContent>
          <p>Esta área é restrita a administradores.</p>
        </CardContent>
      </Card>
    </div>
  )
}
