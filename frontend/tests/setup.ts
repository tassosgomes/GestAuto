import '@testing-library/jest-dom/vitest';

// Mock do ResizeObserver para testes com Radix UI
global.ResizeObserver = class ResizeObserver {
  observe() {}
  unobserve() {}
  disconnect() {}
};