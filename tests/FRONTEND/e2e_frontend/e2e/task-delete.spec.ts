import { test, expect } from '@playwright/test';

test.describe('Task Management - Delete Flow', () => {
    test('should delete a task successfully', async ({ page }) => {
        // Navegar a la página de tareas
        await page.goto('/tasks');

        // Esperar a que la página cargue
        await page.waitForLoadState('networkidle');

        // Crear una tarea de prueba primero
        const newTaskButton = page.getByRole('link', { name: /new task/i });
        if (await newTaskButton.isVisible()) {
            await newTaskButton.click();
        } else {
            await page.goto('/tasks/new');
        }

        // Llenar el formulario
        await page.fill('#title', 'Task to Delete');
        await page.fill('#description', 'This task will be deleted');

        // Enviar el formulario
        await page.click('button[type="submit"]');

        // Esperar navegación a lista de tareas
        await page.waitForURL(/\/tasks$/);

        // Verificar que la tarea existe
        await expect(page.getByText('Task to Delete')).toBeVisible();

        // Hacer clic en el botón de eliminar
        const taskRow = page.locator('text=Task to Delete').locator('..');
        const deleteButton = taskRow.getByRole('button', { name: /delete/i });
        await deleteButton.click();

        // Si hay un diálogo de confirmación, confirmarlo
        // Esperar a que aparezca el diálogo de confirmación
        const confirmDialog = page.locator('role=dialog');
        if (await confirmDialog.isVisible()) {
            // Buscar el botón de confirmación en el diálogo
            const confirmButton = confirmDialog.getByRole('button', { name: /confirm|delete|yes/i });
            await confirmButton.click();
        }

        // Esperar a que desaparezca de la lista
        // Dar tiempo para que se procese la eliminación
        await page.waitForTimeout(1000);

        // Verificar que la tarea ya no está visible
        await expect(page.getByText('Task to Delete')).not.toBeVisible();
    });
});
