import { render, screen } from '@testing-library/react';
import TaskList from '@/components/TaskList';
import type { Task } from '@/lib/types';

// Mock de Next.js Link
jest.mock('next/link', () => {
    return ({ children, href }: any) => {
        return <a href={href}>{children}</a>;
    };
});

describe('TaskList', () => {
    it('debe renderizar mensaje cuando no hay tareas', () => {
        render(<TaskList tasks={[]} />);
        expect(screen.getByText('No tasks found.')).toBeInTheDocument();
    });

    it('debe renderizar lista de tareas correctamente', () => {
        const mockTasks: Task[] = [
            {
                id: '1',
                title: 'Tarea de Prueba',
                description: 'Descripción de prueba',
                status: 'Pending',
                dueDate: '2024-12-31',
            },
            {
                id: '2',
                title: 'Segunda Tarea',
                status: 'InProgress',
            },
        ];

        const { container } = render(<TaskList tasks={mockTasks} />);

        // Verificar que las tareas aparecen (pueden aparecer múltiples veces por diseño responsive)
        const tareas = screen.getAllByText('Tarea de Prueba');
        expect(tareas.length).toBeGreaterThan(0);

        const segundaTarea = screen.getAllByText('Segunda Tarea');
        expect(segundaTarea.length).toBeGreaterThan(0);
    });
});
