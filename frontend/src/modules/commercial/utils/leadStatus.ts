export type BadgeVariant = 'default' | 'secondary' | 'destructive' | 'outline';

export interface LeadStatusPresentation {
  label: string;
  variant: BadgeVariant;
}

const DEFAULT_PRESENTATION: LeadStatusPresentation = {
  label: 'Desconhecido',
  variant: 'secondary',
};

function normalizeStatusKey(status: string): string {
  return status
    .trim()
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, '');
}

export function getLeadStatusPresentation(status: string | null | undefined): LeadStatusPresentation {
  if (!status) {
    return DEFAULT_PRESENTATION;
  }

  const raw = String(status);
  const trimmed = raw.trim();
  const key = normalizeStatusKey(raw);

  switch (key) {
    // Canonical values from backend enum: services/commercial/.../LeadStatus.cs
    case 'new':
      return { label: 'Novo', variant: 'default' };
    case 'incontact':
      return { label: 'Contatado', variant: 'secondary' };
    case 'innegotiation':
      return { label: 'Em Negociação', variant: 'outline' };
    case 'testdrivescheduled':
      return { label: 'Test-Drive Agendado', variant: 'outline' };
    case 'proposalsent':
      return { label: 'Proposta Enviada', variant: 'outline' };
    case 'lost':
      return { label: 'Perdido', variant: 'destructive' };
    case 'converted':
      return { label: 'Convertido', variant: 'default' };

    // Legacy/frontend-only aliases (kept for compatibility)
    case 'contacted':
      return { label: 'Contatado', variant: 'secondary' };
    case 'qualified':
      return { label: 'Qualificado', variant: 'secondary' };

    // Some backends may emit these variants
    case 'notqualified':
      return { label: 'Não Qualificado', variant: 'destructive' };

    default:
      return { label: trimmed || DEFAULT_PRESENTATION.label, variant: 'secondary' };
  }
}
