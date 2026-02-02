# Documentación de Pruebas Unitarias - Azure Functions - TaskTestAPI
---
## Cómo Ejecutar las Pruebas

### 1: Línea de Comandos

```powershell
# Navegar a la carpeta del proyecto de pruebas
cd "c:\Users\usuario\Desktop\path\controlRisk_techTest_Task\api\src\TestTaskApi"

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

Este proyecto de pruebas unitarias  sigue el patrón **Factory Repository**. Se utilizan **xUnit** como framework de pruebas y **Moq** para crear mocks de las dependencias.
se dividieron las pruebas en diferentes archivos (partial class) que hace referencia a cada metodo de las Azure Functions.


### Tecnologías Utilizadas
- **xUnit 2.5.3**: Framework de testing
- **Moq 4.20.72**: Biblioteca de mocking
- **.NET 9.0**: Target framework

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
- **`TasksFunctionTests.Helpers.cs`**: Constructor y métodos auxiliares compartidos en las otras clases parciales
- **`TasksFunctionTests.GetTasks.cs`**: Tests del endpoint GET /tasks
- **`TasksFunctionTests.GetTaskById.cs`**: Tests del endpoint GET /tasks/{id} 
- **`TasksFunctionTests.CreateTask.cs`**: Tests del endpoint POST /tasks 
- **`TasksFunctionTests.UpdateTask.cs`**: Tests del endpoint PUT /tasks/{id} 
- **`TasksFunctionTests.DeleteTask.cs`**: Tests del endpoint DELETE /tasks/{id} 

**¿Qué comparten las clases parciales?**
- Los mismos campos privados (mocks de repositorios, factory, logger, función)
- El constructor que inicializa estos mocks
- Los métodos helper para crear mocks de requests y contexts

---



