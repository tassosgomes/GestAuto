import { useNavigate } from 'react-router-dom';
import { Plus } from 'lucide-react';
import { Button } from '@/components/ui/button';

export function ProposalListPage() {
  const navigate = useNavigate();

  return (
    <div className="p-6 space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Propostas</h1>
          <p className="text-muted-foreground">
            Gerencie as propostas comerciais.
          </p>
        </div>
        <Button onClick={() => navigate('/commercial/proposals/new')}>
          <Plus className="mr-2 h-4 w-4" />
          Nova Proposta
        </Button>
      </div>
      
      <div className="rounded-md border p-8 text-center text-muted-foreground">
        <p>Listagem de propostas será implementada na próxima tarefa.</p>
      </div>
    </div>
  );
}
