import { render, screen } from '@testing-library/react';
import { describe, it, expect } from 'vitest';
import { LeadScoreBadge } from '../src/modules/commercial/components/LeadScoreBadge';
import { LeadActionFeedback } from '../src/modules/commercial/components/LeadActionFeedback';

describe('LeadScoreBadge', () => {
  it('renders Diamond badge with correct label', () => {
    render(<LeadScoreBadge score="Diamond" />);
    expect(screen.getByText('Diamante')).toBeInTheDocument();
  });

  it('renders Gold badge with correct label', () => {
    render(<LeadScoreBadge score="Gold" />);
    expect(screen.getByText('Ouro')).toBeInTheDocument();
  });

  it('renders Silver badge with correct label', () => {
    render(<LeadScoreBadge score="Silver" />);
    expect(screen.getByText('Prata')).toBeInTheDocument();
  });

  it('renders Bronze badge with correct label', () => {
    render(<LeadScoreBadge score="Bronze" />);
    expect(screen.getByText('Bronze')).toBeInTheDocument();
  });

  it('does not render when score is undefined', () => {
    const { container } = render(<LeadScoreBadge score={undefined} />);
    expect(container.firstChild).toBeNull();
  });

  it('does not render when score is invalid', () => {
    const { container } = render(<LeadScoreBadge score="Invalid" />);
    expect(container.firstChild).toBeNull();
  });

  it('displays SLA text when showSla is true for Diamond', () => {
    render(<LeadScoreBadge score="Diamond" showSla={true} />);
    expect(screen.getByText('Atender em 10 min')).toBeInTheDocument();
  });

  it('displays SLA text when showSla is true for Gold', () => {
    render(<LeadScoreBadge score="Gold" showSla={true} />);
    expect(screen.getByText('Atender em 30 min')).toBeInTheDocument();
  });

  it('displays SLA text when showSla is true for Silver', () => {
    render(<LeadScoreBadge score="Silver" showSla={true} />);
    expect(screen.getByText('Atender em 2h')).toBeInTheDocument();
  });

  it('displays SLA text when showSla is true for Bronze', () => {
    render(<LeadScoreBadge score="Bronze" showSla={true} />);
    expect(screen.getByText('Baixa Prioridade')).toBeInTheDocument();
  });

  it('does not display SLA text when showSla is false', () => {
    render(<LeadScoreBadge score="Diamond" showSla={false} />);
    expect(screen.queryByText('Atender em 10 min')).not.toBeInTheDocument();
  });

  it('does not display SLA text by default', () => {
    render(<LeadScoreBadge score="Diamond" />);
    expect(screen.queryByText('Atender em 10 min')).not.toBeInTheDocument();
  });
});

describe('LeadActionFeedback', () => {
  it('renders Diamond feedback with correct title and description', () => {
    render(<LeadActionFeedback score="Diamond" />);
    expect(screen.getByText('Prioridade Máxima')).toBeInTheDocument();
    expect(
      screen.getByText(/Lead de alto valor.*acompanhamento gerencial/i)
    ).toBeInTheDocument();
  });

  it('renders Gold feedback with correct title and description', () => {
    render(<LeadActionFeedback score="Gold" />);
    expect(screen.getByText('Alta Prioridade')).toBeInTheDocument();
    expect(
      screen.getByText(/Excelente oportunidade.*proposta competitiva/i)
    ).toBeInTheDocument();
  });

  it('renders Silver feedback with correct title and description', () => {
    render(<LeadActionFeedback score="Silver" />);
    expect(screen.getByText('Média Prioridade')).toBeInTheDocument();
    expect(
      screen.getByText(/Boa oportunidade.*financiamento parcial/i)
    ).toBeInTheDocument();
  });

  it('renders Bronze feedback with correct title and description', () => {
    render(<LeadActionFeedback score="Bronze" />);
    expect(screen.getByText('Baixa Prioridade')).toBeInTheDocument();
    expect(
      screen.getByText(/Lead de nutrição.*conteúdos automáticos/i)
    ).toBeInTheDocument();
  });

  it('does not render when score is undefined', () => {
    const { container } = render(<LeadActionFeedback score={undefined} />);
    expect(container.firstChild).toBeNull();
  });

  it('does not render when score is invalid', () => {
    const { container } = render(<LeadActionFeedback score="Invalid" />);
    expect(container.firstChild).toBeNull();
  });
});
