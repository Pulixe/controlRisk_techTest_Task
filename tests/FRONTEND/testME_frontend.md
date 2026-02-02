# Guía de Pruebas Unitarias y E2E - Proyecto Next.js TaskTest Control Risk



## Introducción

Este proyecto incluye **6 pruebas unitarias** y **2 pruebas end-to-end (e2e)** que cubren los componentes y flujos más importantes de la aplicación de gestión de tareas.

### ¿Qué son las pruebas unitarias?
Las **pruebas unitarias** son pruebas que validan el funcionamiento de componentes individuales de forma aislada. Verifican que cada pieza de código funcione correctamente por sí sola.

### ¿Qué son las pruebas E2E?
Las **pruebas end-to-end** simulan el comportamiento real de un usuario en la aplicación completa, navegando por múltiples páginas y realizando acciones completas.



## Configuración del Entorno de Pruebas

### Herramientas Instaladas

1. **Jest** - Framework de testing para JavaScript/TypeScript
   - `jest`: Motor de pruebas
   - `jest-environment-jsdom`: Simula el DOM del navegador

2. **React Testing Library** - Librería para testear componentes React
   - `@testing-library/react`: Utilidades para renderizar componentes
   - `@testing-library/jest-dom`: Matchers personalizados para Jest
   - `@testing-library/user-event`: Simula interacciones del usuario

3. **Playwright** - Framework para pruebas E2E
   - `@playwright/test`: Motor de pruebas E2E
   - `playwright`: Navegador automatizado


## Cómo Ejecutar las Pruebas

### Pruebas Unitarias

```bash
# Ejecutar todas las pruebas unitarias
pnpm test

# Ejecutar en modo watch (re-ejecuta al guardar cambios)
pnpm test:watch

# Ejecutar con reporte de cobertura
pnpm test:coverage
```

### Pruebas E2E

```bash
# Ejecutar pruebas E2E (modo headless)
pnpm test:e2e

# Ejecutar con interfaz visual (recomendado para desarrollo)
pnpm test:e2e:ui
```


---

