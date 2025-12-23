import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import type { Lead } from '../types';
import { Mail, Phone, Calendar } from 'lucide-react';
import { format } from 'date-fns';
import { ptBR } from 'date-fns/locale';

interface LeadHeaderProps {
  lead: Lead;
}

export function LeadHeader({ lead }: LeadHeaderProps) {
  const getScoreBadgeColor = (score?: string) => {
    switch (score) {
      case 'Diamond':
        return 'bg-blue-500 hover:bg-blue-600';
      case 'Gold':
        return 'bg-yellow-500 hover:bg-yellow-600';
      case 'Silver':
        return 'bg-gray-400 hover:bg-gray-500';
      case 'Bronze':
        return 'bg-orange-700 hover:bg-orange-800';
      default:
        return 'bg-gray-200 text-gray-800 hover:bg-gray-300';
    }
  };

  return (
    <Card className="mb-6">
      <CardContent className="p-6">
        <div className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
          <div className="flex items-center gap-4">
            <div className="h-16 w-16 rounded-full bg-primary/10 flex items-center justify-center text-2xl font-bold text-primary">
              {lead.name.substring(0, 2).toUpperCase()}
            </div>
            <div>
              <h1 className="text-2xl font-bold flex items-center gap-2">
                {lead.name}
                <Badge variant="outline" className={getScoreBadgeColor(lead.score)}>
                  {lead.score || 'Sem Score'}
                </Badge>
                <Badge variant="secondary">{lead.status}</Badge>
              </h1>
              <div className="flex flex-wrap gap-4 mt-2 text-sm text-muted-foreground">
                <div className="flex items-center gap-1">
                  <Phone className="h-4 w-4" />
                  {lead.phone || 'N/A'}
                </div>
                <div className="flex items-center gap-1">
                  <Mail className="h-4 w-4" />
                  {lead.email || 'N/A'}
                </div>
                <div className="flex items-center gap-1">
                  <Calendar className="h-4 w-4" />
                  {format(new Date(lead.createdAt), "d 'de' MMMM 'às' HH:mm", {
                    locale: ptBR,
                  })}
                </div>
              </div>
            </div>
          </div>
          <div className="flex gap-2">
            <Button variant="outline">Agendar Test-Drive</Button>
            <Button>Criar Proposta</Button>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}
