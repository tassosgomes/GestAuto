import { render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { describe, it, expect, vi } from 'vitest';
import AppLayout from '../src/components/layout/AppLayout';
import * as useAuthModule from '../src/auth/useAuth';

// Mock useAuth
vi.mock('../src/auth/useAuth', () => ({
  useAuth: vi.fn(),
}));

describe('AppLayout', () => {
  it('renders the sidebar and header', () => {
    // Mock return value
    (useAuthModule.useAuth as any).mockReturnValue({
      status: 'ready',
      session: {
        username: 'testuser',
        roles: ['ADMIN'],
      },
      auth: {
        logout: vi.fn(),
      },
    });

    render(
      <MemoryRouter>
        <AppLayout />
      </MemoryRouter>
    );

    // Check for Sidebar items
    expect(screen.getByText('Home')).toBeInTheDocument();
    expect(screen.getByText('Avaliações')).toBeInTheDocument();
    expect(screen.getByText('Admin')).toBeInTheDocument();
    expect(screen.getByText('Configurações')).toBeInTheDocument();

    // Check for Header elements (Search input)
    expect(screen.getByPlaceholderText('Buscar...')).toBeInTheDocument();
    
    // Check for Logo/Title
    expect(screen.getByText('GestAuto')).toBeInTheDocument();

    // Check for UserNav
    // The username text is inside the dropdown content which is not visible by default.
    // We check for the avatar fallback initials 'TE' (from 'testuser')
    expect(screen.getByText('TE')).toBeInTheDocument();
  });
});
