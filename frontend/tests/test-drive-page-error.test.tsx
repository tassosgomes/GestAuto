import { describe, expect, it, vi } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import React from 'react'

import { TestDrivePage } from '@/modules/commercial/pages/TestDrivePage'

vi.mock('@/modules/commercial/services/testDriveService', () => ({
  testDriveService: {
    getAll: vi.fn(async () => {
      throw Object.assign(new Error('Bad Request'), { response: { status: 400 } })
    }),
    schedule: vi.fn(),
    complete: vi.fn(),
    cancel: vi.fn(),
    getById: vi.fn(),
  },
}))

const toastSpy = vi.fn()
vi.mock('@/hooks/use-toast', () => ({
  useToast: () => ({ toast: toastSpy }),
}))

describe('TestDrivePage', () => {
  it('shows toast on load error and keeps empty state', async () => {
    render(<TestDrivePage />)

    expect(await screen.findByText('Test-Drives')).toBeInTheDocument()

    await waitFor(() => {
      expect(toastSpy).toHaveBeenCalled()
    })

    expect(screen.getByText('Nenhum test-drive agendado para este dia.')).toBeInTheDocument()
  })
})
