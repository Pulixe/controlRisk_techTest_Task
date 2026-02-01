import { test, expect } from '@playwright/test';

test.describe('Task Management - Create and Edit Flow', () => {
    test('should create a new task and then edit it', async ({ page }) => {
        // Navegar a la página de tareas
        await page.goto('/tasks');

        // Esperar a que la página cargue
        await page.waitForLoadState('networkidle');

        // Hacer clic en el botón "New Task" o similar (ajusta según tu UI)
        const newTaskButton = page.getByRole('link', { name: /new task/i });
        if (await newTaskButton.isVisible()) {
            await newTaskButton.click();
        } else {
            // Si no hay botón visible, navegar directamente
            await page.goto('/tasks/new');
        }

        // Llenar el formulario de nueva tarea
        await page.fill('#title', 'E2E Test Task');
        await page.fill('#description', 'This is a test task created by E2E test');

        // Seleccionar una fecha futura
        const tomorrow = new Date();
        tomorrow.setDate(tomorrow.getDate() + 1);
        const dateString = tomorrow.toISOString().split('T')[0];
        await page.fill('#dueDate', dateString);

        // Seleccionar estado
        await page.selectOption('#status', 'Pending');

        // Enviar el formulario
        await page.click('button[type="submit"]');

        // Esperar navegación a lista de tareas
        await page.waitForURL(/\/tasks$/);

        // Verificar que la tarea aparece en la lista
        await expect(page.getByText('E2E Test Task')).toBeVisible();

        // Hacer clic en el botón de editar de la tarea recién creada
        const taskRow = page.locator('text=E2E Test Task').locator('..');
        await taskRow.getByRole('link', { name: /edit/i }).click();

        // Esperar a que cargue el formulario de edición
        await page.waitForURL(/\/tasks\/.*\/edit$/);

        // Modificar el título
        await page.fill('#title', 'E2E Test Task - Edited');

        // Cambiar el estado
        await page.selectOption('#status', 'InProgress');

        // Guardar cambios
        await page.click('button[type="submit"]');

        // Volver a la lista y verificar cambios
        await page.waitForURL(/\/tasks$/);
        await expect(page.getByText('E2E Test Task - Edited')).toBeVisible();
    });
});
