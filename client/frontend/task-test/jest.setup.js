// Learn more: https://github.com/testing-library/jest-dom
import '@testing-library/jest-dom'

// Mock de nanoid para evitar problemas con ES Modules
jest.mock('nanoid', () => ({
    nanoid: () => 'test-id-123',
}));

