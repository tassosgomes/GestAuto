import { useState } from 'react';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import type { Lead } from '../types';
import { Mail, Phone, Calendar } from 'lucide-react';
import { format } from 'date-fns';
import { ptBR } from 'date-fns/locale';
import { LeadScoreBadge } from './LeadScoreBadge';
import { ScheduleTestDriveDialog } from './ScheduleTestDriveDialog';
import { CreateProposalDialog } from './CreateProposalDialog';

interface LeadHeaderProps {
  lead: Lead;
}

export function LeadHeader({ lead }: LeadHeaderProps) {
  const [isTestDriveDialogOpen, setIsTestDriveDialogOpen] = useState(false);
  const [isProposalDialogOpen, setIsProposalDialogOpen] = useState(false);

  return (
    <>
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
                  <LeadScoreBadge score={lead.score} showSla />
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
              <Button 
                variant="outline"
                onClick={() => setIsTestDriveDialogOpen(true)}
              >
                Agendar Test-Drive
              </Button>
              <Button onClick={() => setIsProposalDialogOpen(true)}>
                Criar Proposta
              </Button>
            </div>
          </div>
        </CardContent>
      </Card>

      <ScheduleTestDriveDialog
        lead={lead}
        open={isTestDriveDialogOpen}
        onOpenChange={setIsTestDriveDialogOpen}
      />

      <CreateProposalDialog
        lead={lead}
        open={isProposalDialogOpen}
        onOpenChange={setIsProposalDialogOpen}
      />
    </>
  );
}
