import { describe, expect, it } from 'vitest';
import { getLeadStatusPresentation } from './leadStatus';

describe('getLeadStatusPresentation', () => {
  it('maps backend enum values to pt-BR labels', () => {
    expect(getLeadStatusPresentation('InNegotiation')).toEqual({
      label: 'Em Negociação',
      variant: 'outline',
    });

    expect(getLeadStatusPresentation('ProposalSent')).toEqual({
      label: 'Proposta Enviada',
      variant: 'outline',
    });

    expect(getLeadStatusPresentation('Lost')).toEqual({
      label: 'Perdido',
      variant: 'destructive',
    });
  });

  it('keeps a safe fallback for unknown values', () => {
    expect(getLeadStatusPresentation('SomeNewStatus')).toEqual({
      label: 'SomeNewStatus',
      variant: 'secondary',
    });
  });

  it('handles null/undefined', () => {
    expect(getLeadStatusPresentation(undefined)).toEqual({
      label: 'Desconhecido',
      variant: 'secondary',
    });
  });
});
