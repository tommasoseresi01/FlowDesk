import { render, screen } from '@testing-library/react'
import { expect, test } from 'vitest'
import App from './App.tsx'

test('shows the application name as the main heading', () => {
  render(<App />)

  expect(screen.getByRole('heading', { level: 1, name: 'FlowDesk' })).toBeInTheDocument()
})
