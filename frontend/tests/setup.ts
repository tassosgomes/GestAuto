import '@testing-library/jest-dom';

// Mock do ResizeObserver para testes com Radix UI
global.ResizeObserver = class ResizeObserver {
  observe() {}
  unobserve() {}
  disconnect() {}
};