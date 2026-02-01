# Documentación de Pruebas Unitarias - Azure Functions

## Índice
1. [Resumen General](#resumen-general)
2. [Estructura de las Pruebas](#estructura-de-las-pruebas)
3. [Pruebas de TasksFunction](#pruebas-de-tasksfunction)
4. [Pruebas de UsersFunction](#pruebas-de-usersfunction)
5. [Aspectos Adicionales Probados](#aspectos-adicionales-probados)
6. [Cómo Ejecutar las Pruebas](#cómo-ejecutar-las-pruebas)
7. [Recomendaciones para el Proyecto de Entrevista](#recomendaciones-para-el-proyecto-de-entrevista)

---
## Cómo Ejecutar las Pruebas

### 1: Línea de Comandos

```powershell
# Navegar a la carpeta del proyecto de pruebas
cd "c:\Users\Pulixe\Desktop\Control Risk\controlRisk_techTest_Task\api\src\TestTaskApi"

# Ejecutar todas las pruebas
dotnet test

# Ejecutar con verbosidad detallada
dotnet test --verbosity detailed

# Ejecutar solo pruebas de TasksFunction
dotnet test --filter "FullyQualifiedName~TasksFunctionTests"

# Ejecutar solo pruebas de UsersFunction
dotnet test --filter "FullyQualifiedName~UsersFunctionTests"

# Generar reporte de cobertura
dotnet test --collect:"XPlat Code Coverage"
```


## Resumen General

Este proyecto de pruebas unitarias implementa una cobertura completa de las Azure Functions siguiendo el patrón **Factory Repository**. Se utilizan **xUnit** como framework de pruebas y **Moq** para crear mocks de las dependencias.

### Cobertura Total
- **TasksFunctionTests**: 31 casos de prueba
- **UsersFunctionTests**: 11 casos de prueba
- **Total**: 42 pruebas unitarias

### Tecnologías Utilizadas
- **xUnit 2.5.3**: Framework de testing
- **Moq 4.20.72**: Biblioteca de mocking
- **.NET 8.0**: Target framework
- **Partial Classes**: Organización modular de tests

---




### Patrón AAA (Arrange-Act-Assert)

Todas las pruebas siguen el patrón AAA:
- **Arrange**: Configuración de mocks y datos de prueba
- **Act**: Ejecución del método bajo prueba
- **Assert**: Verificación del resultado esperado

---

## Organización con Partial Classes

Ambas suites de pruebas utilizan **clases parciales** para dividir los tests en archivos separados:

### TasksFunction
- **`TasksFunctionTests.Helpers.cs`**: Constructor y métodos auxiliares compartidos
- **`TasksFunctionTests.GetTasks.cs`**: Tests del endpoint GET /tasks (5 pruebas)
- **`TasksFunctionTests.GetTaskById.cs`**: Tests del endpoint GET /tasks/{id} (6 pruebas)
- **`TasksFunctionTests.CreateTask.cs`**: Tests del endpoint POST /tasks (5 pruebas)
- **`TasksFunctionTests.UpdateTask.cs`**: Tests del endpoint PUT /tasks/{id} (6 pruebas)
- **`TasksFunctionTests.DeleteTask.cs`**: Tests del endpoint DELETE /tasks/{id} (6 pruebas)

### UsersFunction
- **`UsersFunctionTests.Helpers.cs`**: Constructor y métodos auxiliares compartidos
- **`UsersFunctionTests.GetUsers.cs`**: Tests del endpoint GET /users (11 pruebas)

**¿Qué comparten las clases parciales?**
- Los mismos campos privados (mocks de repositorios, factory, logger, función)
- El constructor que inicializa estos mocks
- Los métodos helper para crear mocks de requests y contexts

---

## Pruebas de TasksFunction

### 1. GetTasks (6 pruebas)

#### `GetTasks_WithAuthorizedUser_ReturnsOkWithTasks`
- **Propósito**: Verifica que un usuario autenticado pueda obtener su lista de tareas
- **Verifica**: Status HTTP 200 y que el repositorio se llame con el userId correcto

#### `GetTasks_WithoutAuthorization_ReturnsUnauthorized`
- **Propósito**: Valida que sin autenticación se rechace la solicitud
- **Verifica**: Status HTTP 401 y que el repositorio nunca se llame

#### `GetTasks_WithQueryParameters_PassesCorrectFilters`
- **Propósito**: Asegura que los filtros de búsqueda se pasen correctamente al repositorio
- **Verifica**: Parámetros como `status`, `assignedTo`, `sortBy`, `desc`

#### `GetTasks_WithPagination_PassesCorrectSkipAndTake`
- **Propósito**: Valida que la paginación funcione correctamente
- **Verifica**: Valores de `skip` y `take` se pasen al repositorio

#### `GetTasks_WhenExceptionOccurs_ReturnsInternalServerError`
- **Propósito**: Manejo de errores del repositorio
- **Verifica**: Status HTTP 500 cuando ocurre una excepción

---

### 2. GetTaskById (6 pruebas)

#### `GetTaskById_WithValidIdAndAuthorizedUser_ReturnsOkWithTask`
- **Propósito**: Recuperación exitosa de una tarea específica
- **Verifica**: Status HTTP 200 y que se llame al repositorio con el ID correcto

#### `GetTaskById_WithNonExistentId_ReturnsNotFound`
- **Propósito**: Manejo de tareas inexistentes
- **Verifica**: Status HTTP 404

#### `GetTaskById_WithoutAuthorization_ReturnsUnauthorized`
- **Propósito**: Protección contra acceso no autenticado
- **Verifica**: Status HTTP 401

#### `GetTaskById_WithDifferentUser_ReturnsForbidden`
- **Propósito**: Valida que un usuario no pueda ver tareas de otro usuario
- **Verifica**: Status HTTP 403

#### `GetTaskById_WithInvalidGuid_ReturnsBadRequest`
- **Propósito**: Validación de formato de ID
- **Verifica**: Status HTTP 400 con GUID inválido

#### `GetTaskById_WhenExceptionOccurs_ReturnsInternalServerError`
- **Propósito**: Manejo de errores inesperados
- **Verifica**: Status HTTP 500

---

### 3. CreateTask (5 pruebas)

#### `CreateTask_WithValidData_ReturnsCreated`
- **Propósito**: Creación exitosa de una tarea
- **Verifica**: Status HTTP 201, llamada al factory y repositorio, asignación de userId

#### `CreateTask_WithoutAuthorization_ReturnsUnauthorized`
- **Propósito**: Protección de endpoint de creación
- **Verifica**: Status HTTP 401

#### `CreateTask_WithEmptyTitle_ReturnsBadRequest`
- **Propósito**: Validación de campo obligatorio
- **Verifica**: Status HTTP 400 con título vacío

#### `CreateTask_WithNullPayload_ReturnsBadRequest`
- **Propósito**: Validación de payload
- **Verifica**: Status HTTP 400 con payload inválido

#### `CreateTask_WhenExceptionOccurs_ReturnsInternalServerError`
- **Propósito**: Manejo de errores durante la creación
- **Verifica**: Status HTTP 500

---

### 4. UpdateTask (7 pruebas)

#### `UpdateTask_WithValidData_ReturnsOk`
- **Propósito**: Actualización exitosa de una tarea
- **Verifica**: Status HTTP 200 y que los campos se actualicen correctamente

#### `UpdateTask_WithNonExistentId_ReturnsNotFound`
- **Propósito**: Intento de actualizar tarea inexistente
- **Verifica**: Status HTTP 404

#### `UpdateTask_WithoutAuthorization_ReturnsUnauthorized`
- **Propósito**: Endpoint protegido contra acceso no autenticado
- **Verifica**: Status HTTP 401

#### `UpdateTask_WithDifferentUser_ReturnsForbidden`
- **Propósito**: Un usuario no puede actualizar tareas de otro
- **Verifica**: Status HTTP 403

#### `UpdateTask_WithInvalidGuid_ReturnsBadRequest`
- **Propósito**: Validación de formato de ID
- **Verifica**: Status HTTP 400

#### `UpdateTask_WhenExceptionOccurs_ReturnsInternalServerError`
- **Propósito**: Manejo de errores durante actualización
- **Verifica**: Status HTTP 500

---

### 5. DeleteTask (7 pruebas)

#### `DeleteTask_WithValidId_ReturnsNoContent`
- **Propósito**: Eliminación exitosa de una tarea
- **Verifica**: Status HTTP 204

#### `DeleteTask_WithNonExistentId_ReturnsNoContent`
- **Propósito**: Comportamiento idempotente (DELETE de tarea ya eliminada)
- **Verifica**: Status HTTP 204 sin llamar al repositorio

#### `DeleteTask_WithoutAuthorization_ReturnsUnauthorized`
- **Propósito**: Protección del endpoint de eliminación
- **Verifica**: Status HTTP 401

#### `DeleteTask_WithDifferentUser_ReturnsForbidden`
- **Propósito**: Un usuario no puede eliminar tareas de otro
- **Verifica**: Status HTTP 403

#### `DeleteTask_WithInvalidGuid_ReturnsBadRequest`
- **Propósito**: Validación de formato de ID
- **Verifica**: Status HTTP 400

#### `DeleteTask_WhenExceptionOccurs_ReturnsInternalServerError`
- **Propósito**: Manejo de errores durante eliminación
- **Verifica**: Status HTTP 500

---

## Pruebas de UsersFunction

### GetUsers (11 pruebas)

#### `GetUsers_WithAuthorizedUser_ReturnsOkWithUsers`
- **Propósito**: Listado exitoso de usuarios
- **Verifica**: Status HTTP 200 y parámetros por defecto

#### `GetUsers_WithoutAuthorization_ReturnsUnauthorized`
- **Propósito**: Protección del endpoint
- **Verifica**: Status HTTP 401

#### `GetUsers_WithSearchQuery_PassesQueryToRepository`
- **Propósito**: Funcionalidad de búsqueda por nombre/email
- **Verifica**: Parámetro `q` se pasa correctamente

#### `GetUsers_WithPagination_PassesCorrectSkipAndTake`
- **Propósito**: Paginación de resultados
- **Verifica**: Valores de `skip` y `take`

#### `GetUsers_WithDefaultPagination_UsesDefaultValues`
- **Propósito**: Valores por defecto en ausencia de parámetros
- **Verifica**: skip=0, take=50

#### `GetUsers_WithInvalidPagination_UsesDefaultValues`
- **Propósito**: Manejo de parámetros inválidos
- **Verifica**: Fallback a valores por defecto

#### `GetUsers_ReturnsOnlyIdNameAndEmail`
- **Propósito**: Filtrado de campos sensibles
- **Verifica**: Solo se retornan id, name y email (no Sub, CreatedAt, etc.)

#### `GetUsers_WhenExceptionOccurs_ReturnsInternalServerError`
- **Propósito**: Manejo de errores del repositorio
- **Verifica**: Status HTTP 500

#### `GetUsers_WithEmptyResult_ReturnsOkWithEmptyList`
- **Propósito**: Manejo de resultados vacíos
- **Verifica**: Status HTTP 200 con lista vacía

#### `GetUsers_WithCombinedQueryAndPagination_PassesAllParameters`
- **Propósito**: Combinación de búsqueda y paginación
- **Verifica**: Todos los parámetros se pasan correctamente

---



---




---


