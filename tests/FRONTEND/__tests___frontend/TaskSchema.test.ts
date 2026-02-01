import { TaskSchema, TaskStatusEnum, NewTaskSchema } from '@/lib/types';

describe('TaskSchema', () => {
    it('debe validar una tarea completa correctamente', () => {
        const validTask = {
            id: '550e8400-e29b-41d4-a716-446655440000',
            title: 'Tarea de Prueba',
            description: 'Descripción de prueba',
            dueDate: '2024-12-31',
            status: 'Pending',
            AssignedTo: 'user@example.com',
        };

        const result = TaskSchema.safeParse(validTask);
        expect(result.success).toBe(true);
    });

    it('debe rechazar una tarea sin título', () => {
        const invalidTask = {
            title: '',
            status: 'Pending',
        };

        const result = TaskSchema.safeParse(invalidTask);
        expect(result.success).toBe(false);
    });

    it('debe rechazar una fecha en formato incorrecto', () => {
        const invalidTask = {
            title: 'Tarea',
            status: 'Pending',
            dueDate: '31/12/2024', // formato incorrecto
        };

        const result = TaskSchema.safeParse(invalidTask);
        expect(result.success).toBe(false);
    });
});

describe('TaskStatusEnum', () => {
    it('debe aceptar valores válidos de status', () => {
        expect(TaskStatusEnum.safeParse('Pending').success).toBe(true);
        expect(TaskStatusEnum.safeParse('InProgress').success).toBe(true);
        expect(TaskStatusEnum.safeParse('Done').success).toBe(true);
    });

    it('debe rechazar valores inválidos de status', () => {
        expect(TaskStatusEnum.safeParse('Invalid').success).toBe(false);
        expect(TaskStatusEnum.safeParse('').success).toBe(false);
    });
});
