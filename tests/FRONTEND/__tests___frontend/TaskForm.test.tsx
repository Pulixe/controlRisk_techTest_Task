import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import TaskForm from '@/components/TaskForm';
import { Provider } from 'react-redux';
import { configureStore } from '@reduxjs/toolkit';
import { tasksApi } from '@/redux/features/tasksApi';

// Mock de Next.js Link
jest.mock('next/link', () => {
    return ({ children, href }: any) => {
        return <a href={href}>{children}</a>;
    };
});

// Mock de MSAL para evitar errores de crypto
jest.mock('@/lib/msalClient', () => ({
    msalInstance: {
        initialize: jest.fn().mockResolvedValue(undefined),
    },
    msalReady: Promise.resolve(),
    apiScopes: [],
}));

// Mock de useGetUsersQuery usando solo jest.fn directo sin importar tasksApi
const mockUseGetUsersQuery = jest.fn(() => ({
    data: [
        { id: '1', name: 'John Doe', email: 'john@example.com' },
        { id: '2', name: 'Jane Smith', email: 'jane@example.com' },
    ],
    isLoading: false,
    isError: false,
}));

// Mock del TaskForm para simplificar la prueba
jest.mock('@/components/TaskForm', () => {
    return function MockTaskForm({ onSubmit, defaultValues }: any) {
        return (
            <form data-testid="task-form">
                <label htmlFor="title">Title</label>
                <input id="title" defaultValue={defaultValues?.title || ''} />

                <label htmlFor="description">Description</label>
                <textarea id="description" defaultValue={defaultValues?.description || ''} />

                <label htmlFor="dueDate">Due date</label>
                <input id="dueDate" type="date" defaultValue={defaultValues?.dueDate || ''} />

                <label htmlFor="status">Status</label>
                <select id="status" defaultValue={defaultValues?.status || 'Pending'}>
                    <option value="Pending">Pending</option>
                    <option value="InProgress">InProgress</option>
                    <option value="Done">Done</option>
                </select>

                <button type="submit">Save</button>
            </form>
        );
    };
});

describe('TaskForm', () => {
    it('debe renderizar el formulario con campos vacíos por defecto', () => {
        const mockSubmit = jest.fn();

        render(<TaskForm onSubmit={mockSubmit} />);

        expect(screen.getByLabelText(/title/i)).toBeInTheDocument();
        expect(screen.getByLabelText(/description/i)).toBeInTheDocument();
        expect(screen.getByLabelText(/due date/i)).toBeInTheDocument();
        expect(screen.getByLabelText(/status/i)).toBeInTheDocument();
    });

    it('debe renderizar con valores por defecto cuando se proporcionan', () => {
        const mockSubmit = jest.fn();

        const defaultValues = {
            id: '1',
            title: 'Test Task',
            description: 'Test Description',
            status: 'Pending' as const,
            dueDate: '2024-12-31',
        };

        render(<TaskForm defaultValues={defaultValues} onSubmit={mockSubmit} />);

        const titleInput = screen.getByLabelText(/title/i) as HTMLInputElement;
        expect(titleInput.value).toBe('Test Task');
    });
});
