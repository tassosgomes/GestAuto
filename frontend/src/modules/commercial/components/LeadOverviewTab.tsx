import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { usePaymentMethods } from '../hooks/usePaymentMethods';
import type { Lead } from '../types';

interface LeadOverviewTabProps {
  lead: Lead;
}

export function LeadOverviewTab({ lead }: LeadOverviewTabProps) {
  const { data: paymentMethods } = usePaymentMethods();
  
  // Criar mapa de códigos para nomes a partir dos dados da API
  const getPaymentMethodName = (code: string | undefined): string => {
    if (!code) return 'Não informado';
    if (!paymentMethods || paymentMethods.length === 0) return code;
    
    // Comparação case-insensitive para maior robustez
    const method = paymentMethods.find(pm => pm.code.toUpperCase() === code.toUpperCase());
    return method?.name || code;
  };

  return (
    <div className="grid gap-6 md:grid-cols-2">
      <Card>
        <CardHeader>
          <CardTitle>Informações de Qualificação</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="space-y-4">
            <div>
              <span className="text-sm text-muted-foreground">Forma de Pagamento</span>
              <p className="font-medium">
                {getPaymentMethodName(lead.qualification?.paymentMethod)}
              </p>
            </div>
            <div>
              <span className="text-sm text-muted-foreground">Previsão de Compra</span>
              <p className="font-medium">
                {lead.qualification?.expectedPurchaseDate
                  ? new Date(lead.qualification.expectedPurchaseDate).toLocaleDateString('pt-BR')
                  : 'Não informado'}
              </p>
            </div>
            <div>
              <span className="text-sm text-muted-foreground">Interessado em Test-Drive</span>
              <p className="font-medium">
                {lead.qualification?.interestedInTestDrive ? 'Sim' : 'Não'}
              </p>
            </div>
            <div>
              <span className="text-sm text-muted-foreground">Veículo na Troca</span>
              <p className="font-medium">
                {lead.qualification?.hasTradeInVehicle ? 'Sim' : 'Não'}
              </p>
            </div>
            {lead.qualification?.hasTradeInVehicle && lead.qualification.tradeInVehicle && (
              <div className="mt-4 border-t pt-4">
                <h4 className="text-sm font-semibold mb-3">Veículo de Troca</h4>
                <div className="space-y-2 text-sm">
                  <div>
                    <span className="text-muted-foreground">Marca/Modelo:</span>{' '}
                    <span className="font-medium">
                      {lead.qualification.tradeInVehicle.brand} {lead.qualification.tradeInVehicle.model}
                    </span>
                  </div>
                  <div>
                    <span className="text-muted-foreground">Ano:</span>{' '}
                    <span className="font-medium">{lead.qualification.tradeInVehicle.year}</span>
                  </div>
                  <div>
                    <span className="text-muted-foreground">KM:</span>{' '}
                    <span className="font-medium">
                      {lead.qualification.tradeInVehicle.mileage?.toLocaleString('pt-BR')}
                    </span>
                  </div>
                </div>
              </div>
            )}
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Interesse</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="space-y-4">
            <div>
              <span className="text-sm text-muted-foreground">Modelo</span>
              <p className="font-medium">{lead.interestedModel || 'Não informado'}</p>
            </div>
            <div>
              <span className="text-sm text-muted-foreground">Versão</span>
              <p className="font-medium">{lead.interestedTrim || 'Não informado'}</p>
            </div>
            <div>
              <span className="text-sm text-muted-foreground">Cor</span>
              <p className="font-medium">{lead.interestedColor || 'Não informado'}</p>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
