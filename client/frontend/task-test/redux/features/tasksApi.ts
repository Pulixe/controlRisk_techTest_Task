import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import type { PagedTasks, Task, TaskQuery, NewTask, UpdateTask, User } from '@/lib/types';
import { acquireToken, msalInstance } from '@/lib/msalClient';

const baseUrl = `${process.env.NEXT_PUBLIC_API_BASE ?? ''}/api`;

export const tasksApi = createApi({
  reducerPath: 'tasksApi',
  baseQuery: fetchBaseQuery({
    baseUrl,
    prepareHeaders: async (headers) => {
      try {
        const account = msalInstance.getAllAccounts()[0];
        const token = await acquireToken(account);
        if (token) headers.set('Authorization', `Bearer ${token}`);
      } catch (e) {
        // Silent failure; callers will see 401 if any
        console.warn('prepareHeaders token error', e);
      }
      headers.set('Content-Type', 'application/json');
      return headers;
    },
  }),
  tagTypes: ['Tasks', 'Task'],
  endpoints: (builder) => ({
    // Users lookup for assignment
    getUsers: builder.query<User[], { q?: string; skip?: number; take?: number } | void>({
      query: (args) => {
        const p = new URLSearchParams();
        const { q, skip, take } = args || {};
        if (q) p.set('q', q);
        if (typeof skip === 'number') p.set('skip', String(skip));
        if (typeof take === 'number') p.set('take', String(take));
        const qs = p.toString();
        return `users${qs ? `?${qs}` : ''}`;
      },
      transformResponse: (resp: any): User[] => {
        if (Array.isArray(resp)) return resp as User[];
        if (resp && Array.isArray(resp.items)) return resp.items as User[];
        if (resp && Array.isArray(resp.value)) return resp.value as User[];
        return [];
      },
    }),
    getTasks: builder.query<PagedTasks, TaskQuery | void>({
      query: (args) => {
        const p = new URLSearchParams();
        const { q, status, dueDate, sortBy, desc, page = 1, pageSize = 10 } = args || {};
        if (q) p.set('q', q);
        if (status) p.set('status', status);
        if (dueDate) p.set('dueDate', dueDate);
        if (sortBy) p.set('sortBy', sortBy);
        if (typeof desc === 'boolean') p.set('desc', String(desc));
        if (page) p.set('page', String(page));
        if (pageSize) p.set('pageSize', String(pageSize));
        const qs = p.toString();
        return `tasks${qs ? `?${qs}` : ''}`;
      },
      transformResponse: (resp: any): PagedTasks => {
        if (Array.isArray(resp)) {
          return { items: resp, total: resp.length };
        }
        if (resp && Array.isArray(resp.items)) {
          return { items: resp.items, total: typeof resp.total === 'number' ? resp.total : resp.items.length };
        }
        if (resp && Array.isArray(resp.value)) {
          return { items: resp.value, total: typeof resp.total === 'number' ? resp.total : resp.value.length };
        }
        // Fallback to empty
        return { items: [], total: 0 };
      },
      providesTags: (result) => {
        const items = result?.items ?? [];
        return [{ type: 'Tasks', id: 'LIST' }, ...items.map((t) => ({ type: 'Task' as const, id: t.id }))];
      },
    }),
    getTaskById: builder.query<Task, string>({
      query: (id) => `tasks/${id}`,
      providesTags: (_, __, id) => [{ type: 'Task', id }],
    }),
    createTask: builder.mutation<Task, NewTask>({
      query: (data) => ({ url: 'tasks', method: 'POST', body: data }),
      invalidatesTags: [{ type: 'Tasks', id: 'LIST' }],
    }),
    updateTask: builder.mutation<Task, { id: string; data: UpdateTask }>({
      query: ({ id, data }) => ({ url: `tasks/${id}`, method: 'PUT', body: data }),
      invalidatesTags: (_, __, { id }) => [{ type: 'Task', id }, { type: 'Tasks', id: 'LIST' }],
    }),
    deleteTask: builder.mutation<{ id: string }, string>({
      query: (id) => ({ url: `tasks/${id}`, method: 'DELETE' }),
      invalidatesTags: (_, __, id) => [{ type: 'Task', id }, { type: 'Tasks', id: 'LIST' }],
    }),
  }),
});

export const {
  useGetUsersQuery,
  useGetTasksQuery,
  useGetTaskByIdQuery,
  useCreateTaskMutation,
  useUpdateTaskMutation,
  useDeleteTaskMutation,
} = tasksApi;
