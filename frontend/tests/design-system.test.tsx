import { render, screen } from '@testing-library/react';
import { describe, it, expect } from 'vitest';
import { DesignSystemPage } from '../src/pages/DesignSystemPage';

describe('DesignSystemPage', () => {
  it('renders the design system page', () => {
    render(<DesignSystemPage />);

    // Check for main title
    expect(screen.getByText('Design System')).toBeInTheDocument();

    // Check for sections
    expect(screen.getByText('Cores')).toBeInTheDocument();
    expect(screen.getByText('Tipografia')).toBeInTheDocument();
    expect(screen.getByText('Componentes')).toBeInTheDocument();

    // Check for specific components
    expect(screen.getByText('Buttons')).toBeInTheDocument();
    expect(screen.getByText('Inputs')).toBeInTheDocument();
    expect(screen.getByText('Cards')).toBeInTheDocument();
  });
});
