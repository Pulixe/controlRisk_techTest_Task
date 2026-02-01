import { render, screen, waitFor } from '@testing-library/react';
import { ToastProvider, useToast } from '@/components/Toast';
import { act } from 'react';

// Componente de prueba que usa el hook useToast
function TestComponent() {
    const { toast } = useToast();

    return (
        <button onClick={() => toast({ title: 'Test Toast', variant: 'success' })}>
            Show Toast
        </button>
    );
}

describe('Toast', () => {
    beforeEach(() => {
        jest.useFakeTimers();
    });

    afterEach(() => {
        // Envolver runOnlyPendingTimers en act() para evitar warnings
        act(() => {
            jest.runOnlyPendingTimers();
        });
        jest.useRealTimers();
    });

    it('debe renderizar un toast cuando se llama a la función toast', () => {
        render(
            <ToastProvider>
                <TestComponent />
            </ToastProvider>
        );

        const button = screen.getByText('Show Toast');
        act(() => {
            button.click();
        });

        expect(screen.getByText('Test Toast')).toBeInTheDocument();
    });

    it('debe aplicar la clase correcta según el variant', () => {
        function ErrorToastComponent() {
            const { toast } = useToast();
            return (
                <button onClick={() => toast({ title: 'Error Toast', variant: 'error' })}>
                    Show Error
                </button>
            );
        }

        render(
            <ToastProvider>
                <ErrorToastComponent />
            </ToastProvider>
        );

        act(() => {
            screen.getByText('Show Error').click();
        });

        const toastElement = screen.getByRole('status');
        expect(toastElement).toHaveClass('border-red-300', 'bg-red-50', 'text-red-900');
    });
});
