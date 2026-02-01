import { render, screen } from '@testing-library/react';
import { TaskStatusEnum } from '@/lib/types';

// Componente StatusBadge extraído de TaskList para testearlo
function StatusBadge({ status }: { status: 'Pending' | 'InProgress' | 'Done' }) {
    const cls =
        status === 'Done'
            ? 'bg-green-100 text-green-800 border-green-200'
            : status === 'InProgress'
                ? 'bg-yellow-100 text-yellow-800 border-yellow-200'
                : 'bg-slate-100 text-slate-800 border-slate-200';
    return <span className={`inline-flex rounded-full border px-2 py-0.5 text-xs ${cls}`}>{status}</span>;
}

describe('StatusBadge', () => {
    it('debe renderizar badge de estado Pending con clase correcta', () => {
        const { container } = render(<StatusBadge status="Pending" />);
        const badge = container.querySelector('span');
        expect(badge).toHaveClass('bg-slate-100', 'text-slate-800');
        expect(screen.getByText('Pending')).toBeInTheDocument();
    });

    it('debe renderizar badge de estado InProgress con clase correcta', () => {
        const { container } = render(<StatusBadge status="InProgress" />);
        const badge = container.querySelector('span');
        expect(badge).toHaveClass('bg-yellow-100', 'text-yellow-800');
        expect(screen.getByText('InProgress')).toBeInTheDocument();
    });

    it('debe renderizar badge de estado Done con clase correcta', () => {
        const { container } = render(<StatusBadge status="Done" />);
        const badge = container.querySelector('span');
        expect(badge).toHaveClass('bg-green-100', 'text-green-800');
        expect(screen.getByText('Done')).toBeInTheDocument();
    });
});
