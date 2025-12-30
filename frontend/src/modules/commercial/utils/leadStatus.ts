export type BadgeVariant = 'default' | 'secondary' | 'destructive' | 'outline';

export interface LeadStatusPresentation {
  label: string;
  variant: BadgeVariant;
}

const DEFAULT_PRESENTATION: LeadStatusPresentation = {
  label: 'Desconhecido',
  variant: 'secondary',
};

export function getLeadStatusPresentation(status: string | null | undefined): LeadStatusPresentation {
  if (!status) {
    return DEFAULT_PRESENTATION;
  }

  switch (status) {
    // Canonical values from backend enum: services/commercial/.../LeadStatus.cs
    case 'New':
      return { label: 'Novo', variant: 'default' };
    case 'InContact':
      return { label: 'Contatado', variant: 'secondary' };
    case 'InNegotiation':
      return { label: 'Em Negociação', variant: 'outline' };
    case 'TestDriveScheduled':
      return { label: 'Test-Drive Agendado', variant: 'outline' };
    case 'ProposalSent':
      return { label: 'Proposta Enviada', variant: 'outline' };
    case 'Lost':
      return { label: 'Perdido', variant: 'destructive' };
    case 'Converted':
      return { label: 'Convertido', variant: 'default' };

    // Legacy/frontend-only aliases (kept for compatibility)
    case 'Contacted':
      return { label: 'Contatado', variant: 'secondary' };
    case 'Qualified':
      return { label: 'Qualificado', variant: 'secondary' };

    // Some backends may emit these variants
    case 'NotQualified':
      return { label: 'Não Qualificado', variant: 'destructive' };

    default:
      return { label: status, variant: 'secondary' };
  }
}
