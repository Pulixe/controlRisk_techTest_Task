import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import ConfirmDialog from '@/components/ConfirmDialog';

describe('ConfirmDialog', () => {
    it('no debe renderizar cuando open es false', () => {
        const mockConfirm = jest.fn();
        const mockCancel = jest.fn();

        render(
            <ConfirmDialog
                open={false}
                title="Test Dialog"
                onConfirm={mockConfirm}
                onCancel={mockCancel}
            />
        );

        expect(screen.queryByText('Test Dialog')).not.toBeInTheDocument();
    });

    it('debe renderizar cuando open es true', () => {
        const mockConfirm = jest.fn();
        const mockCancel = jest.fn();

        render(
            <ConfirmDialog
                open={true}
                title="Confirm Action"
                description="Are you sure?"
                onConfirm={mockConfirm}
                onCancel={mockCancel}
            />
        );

        expect(screen.getByText('Confirm Action')).toBeInTheDocument();
        expect(screen.getByText('Are you sure?')).toBeInTheDocument();
    });

    it('debe llamar onConfirm cuando se hace click en el botón de confirmación', async () => {
        const mockConfirm = jest.fn();
        const mockCancel = jest.fn();
        const user = userEvent.setup();

        render(
            <ConfirmDialog
                open={true}
                title="Delete Item"
                confirmText="Delete"
                cancelText="Cancel"
                onConfirm={mockConfirm}
                onCancel={mockCancel}
            />
        );

        const confirmButton = screen.getByText('Delete');
        await user.click(confirmButton);

        expect(mockConfirm).toHaveBeenCalledTimes(1);
    });

    it('debe llamar onCancel cuando se hace click en el botón de cancelación', async () => {
        const mockConfirm = jest.fn();
        const mockCancel = jest.fn();
        const user = userEvent.setup();

        render(
            <ConfirmDialog
                open={true}
                title="Delete Item"
                onConfirm={mockConfirm}
                onCancel={mockCancel}
            />
        );

        const cancelButton = screen.getByText('Cancel');
        await user.click(cancelButton);

        expect(mockCancel).toHaveBeenCalledTimes(1);
    });
});
