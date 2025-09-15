'use client';

import { z } from 'zod';

export const TaskStatusEnum = z.enum(['Pending', 'InProgress', 'Done']);
export type TaskStatus = z.infer<typeof TaskStatusEnum>;

// Basic user type used by /users endpoint
export type User = {
  id: string;
  name: string;
  email: string;
};

export const TaskSchema = z.object({
  id: z.string().uuid().optional(),
  title: z.string().min(1, 'Title is required'),
  description: z.string().optional().or(z.literal('')),
  // Use date input (yyyy-mm-dd). Keep as string for transport; backend can parse.
  dueDate: z
    .string()
    .optional()
    .refine((v) => !v || /^\d{4}-\d{2}-\d{2}$/.test(v), {
      message: 'Use a valid date (YYYY-MM-DD)',
    }),
  status: TaskStatusEnum,
  // Backend expects AssignedTo set to the selected user's email
  AssignedTo: z.string().email('Use a valid email').optional().or(z.literal('')),
  createdAt: z.string().datetime().optional(),
  updatedAt: z.string().datetime().optional(),
});

export type Task = z.infer<typeof TaskSchema> & { id: string };

export const NewTaskSchema = TaskSchema.pick({
  title: true,
  description: true,
  dueDate: true,
  status: true,
  AssignedTo: true,
});
export type NewTask = z.infer<typeof NewTaskSchema>;

export const UpdateTaskSchema = NewTaskSchema.partial();
export type UpdateTask = z.infer<typeof UpdateTaskSchema>;

export const TaskQuerySchema = z.object({
  q: z.string().optional(),
  status: TaskStatusEnum.optional(),
  dueDate: z
    .string()
    .optional()
    .refine((v) => !v || /^\d{4}-\d{2}-\d{2}$/.test(v), { message: 'Use a valid date (YYYY-MM-DD)' }),
  // Sorting per backend contract: sortBy in {duedate, createdat, title} (case-insensitive), desc as 'true' for descending
  sortBy: z.string().optional(),
  desc: z.boolean().optional(),
  page: z.number().int().min(1).default(1).optional(),
  pageSize: z.number().int().min(1).max(100).default(10).optional(),
  // Legacy sort param no longer used; kept for compatibility in types only
  sort: z.string().optional(),
});
export type TaskQuery = z.infer<typeof TaskQuerySchema>;

export type PagedTasks = { items: Task[]; total: number };
