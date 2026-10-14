# ProductInventory API

API REST para la gestión de productos e inventario, desarrollada con ASP.NET Core 10, C# y PostgreSQL. Implementa operaciones CRUD, consulta paginada y ajustes de stock con validaciones de entrada y reglas de negocio.

## Demo pública

- **Swagger UI:** https://productinventory.sbtechgroup.com/swagger/index.html
- **Repositorio:** https://github.com/SantiagoB59/ProductInventory

La ruta pública de OpenAPI se debe consultar según la configuración publicada de la aplicación.

## Objetivo

Desarrollar una API REST mantenible y replicable que permita administrar un catálogo de productos y controlar el inventario, aplicando principios REST, separación de responsabilidades, validación de datos y persistencia relacional.

## Tecnologías

- C# y ASP.NET Core 10
- Entity Framework Core 10
- PostgreSQL
- Swagger / OpenAPI
- xUnit para pruebas automatizadas
- Git y GitHub
- Linux, Nginx y HTTPS para el despliegue público

## Arquitectura

La solución está organizada en los siguientes proyectos:

- **Products.Api:** endpoints HTTP, contratos de solicitud, configuración y exposición de la API.
- **Products.Application:** servicios, casos de uso y contratos de persistencia.
- **Products.Domain:** entidades y reglas fundamentales del dominio.
- **Products.Infrastructure:** persistencia, Entity Framework Core, repositorios y acceso a PostgreSQL.
- **Products.Tests:** pruebas automatizadas.

Esta separación reduce el acoplamiento, facilita las pruebas y permite que los cambios en una capa afecten lo menos posible a las demás.

## Funcionalidades

| Método | Endpoint | Descripción |
|---|---|---|
| POST | `/api/products` | Crear un producto |
| GET | `/api/products` | Listar productos con paginación |
| GET | `/api/products/{id}` | Consultar un producto por ID |
| PUT | `/api/products/{id}` | Actualizar la información general de un producto |
| DELETE | `/api/products/{id}` | Eliminar un producto |
| PATCH | `/api/products/{id}/stock` | Incrementar o disminuir el stock |

### Paginación

El listado acepta los parámetros `page` y `pageSize`.

Ejemplo:

`GET /api/products?page=1&pageSize=10`

El tamaño de página permitido está entre 1 y 100.

### Ajuste de inventario

El endpoint `PATCH /api/products/{id}/stock` recibe una cantidad entera:

```json
{
  "quantity": 10
}
```

- Un valor positivo incrementa las existencias.
- Un valor negativo disminuye las existencias.
- Un ajuste que produzca stock negativo es rechazado.
- El ajuste se realiza mediante una operación transaccional para proteger las actualizaciones concurrentes.

La actualización general mediante `PUT` no modifica las existencias; el stock se gestiona por separado mediante el endpoint de inventario.

## Reglas de negocio y validaciones

- El nombre del producto es obligatorio y tiene una longitud máxima de 120 caracteres.
- La descripción es opcional y tiene una longitud máxima de 500 caracteres.
- El precio debe estar dentro del rango positivo permitido por la validación.
- El stock inicial no puede ser negativo.
- La paginación debe usar una página igual o superior a 1 y un tamaño entre 1 y 100.
- El ajuste de inventario no puede dejar el stock en un valor negativo.
- Los productos inexistentes deben producir una respuesta HTTP adecuada.
- Los errores de validación deben comunicarse al consumidor de la API.

## Requisitos previos

- SDK de .NET 10.
- PostgreSQL.
- Git.
- Herramienta `dotnet-ef` compatible con Entity Framework Core 10 para administrar las migraciones.

## Ejecución local

### 1. Clonar el repositorio

```bash
git clone https://github.com/SantiagoB59/ProductInventory.git
cd ProductInventory
```

### 2. Restaurar y compilar

```bash
dotnet restore ProductInventory.slnx
dotnet build ProductInventory.slnx --configuration Release
```

### 3. Configurar PostgreSQL

Crea una base de datos y un usuario con los permisos necesarios.

La API obtiene la cadena de conexión desde `ConnectionStrings:ProductDatabase`. Puedes configurarla mediante una variable de entorno.

En PowerShell, reemplaza los valores por los de tu entorno:

```powershell
$env:ConnectionStrings__ProductDatabase = "Host=localhost;Port=5432;Database=productinventory;Username=TU_USUARIO;Password=TU_PASSWORD"
```

No guardes contraseñas reales ni cadenas de conexión sensibles en el repositorio.

### 4. Aplicar las migraciones

Si aún no tienes instalada la herramienta:

```bash
dotnet tool install --global dotnet-ef --version 10.0.13
```

Si ya está instalada, no necesitas volver a instalarla.

Desde la raíz del repositorio, ejecuta:

```bash
dotnet ef database update \
  --project src/Products.Infrastructure/Products.Infrastructure.csproj \
  --startup-project src/Products.Api/Products.Api.csproj
```

En PowerShell también puedes ejecutar el comando en una sola línea:

```powershell
dotnet ef database update --project .\src\Products.Infrastructure\Products.Infrastructure.csproj --startup-project .\src\Products.Api\Products.Api.csproj
```

### 5. Ejecutar la API

```bash
dotnet run --project src/Products.Api/Products.Api.csproj
```

Abre la dirección local indicada por la consola y añade `/swagger` para acceder a la documentación interactiva.

## Pruebas automatizadas

Ejecuta las pruebas desde la raíz del repositorio:

```bash
dotnet test ProductInventory.slnx --configuration Release
```

Para compilar la solución:

```bash
dotnet build ProductInventory.slnx --configuration Release
```

La ejecución local verificada durante el desarrollo obtuvo **13 pruebas aprobadas, 0 fallidas y 0 omitidas**.

## Códigos HTTP

La API utiliza códigos HTTP según el resultado de cada operación. Entre los códigos aplicables se encuentran:

- `200 OK`: operación completada con respuesta.
- `201 Created`: producto creado.
- `204 No Content`: eliminación completada, si así lo implementa el endpoint.
- `400 Bad Request`: solicitud inválida o error de validación.
- `404 Not Found`: producto inexistente.
- `409 Conflict`: conflicto de negocio, como un ajuste que produciría existencias negativas.

El código concreto depende de la operación y de su implementación.

## Despliegue en producción

La API cuenta con una versión pública disponible en:

- **Swagger UI:** https://productinventory.sbtechgroup.com/swagger/index.html

La infraestructura utiliza Linux, PostgreSQL y Nginx como proxy inverso con HTTPS.

Para reproducir el despliegue en un servidor nuevo:

1. Instalar el runtime de .NET compatible con la versión de la aplicación y las herramientas necesarias para publicar el proyecto.
2. Instalar y configurar PostgreSQL, creando la base de datos y el usuario de aplicación.
3. Clonar el repositorio e instalar las dependencias del proyecto.
4. Configurar la variable de entorno `ConnectionStrings__ProductDatabase` con la cadena de conexión correspondiente al servidor.
5. Aplicar las migraciones de Entity Framework Core para crear o actualizar el esquema de la base de datos.
6. Publicar la API en modo Release.
7. Configurar un servicio de Linux para ejecutar la aplicación y reiniciarla cuando corresponda.
8. Configurar Nginx como proxy inverso y habilitar HTTPS.
9. Verificar la disponibilidad de la API y probar los endpoints mediante Swagger.

Las credenciales, cadenas de conexión y demás secretos deben configurarse en el entorno de ejecución, sin incluirlos en el repositorio.

Los nombres de servicios, puertos, rutas y archivos de configuración deben ajustarse a la infraestructura de destino.

## Decisiones técnicas

- Separación de responsabilidades para facilitar el mantenimiento.
- PostgreSQL como sistema de persistencia relacional.
- Contratos HTTP y documentación OpenAPI para describir la API.
- Validaciones de entrada y reglas de negocio para proteger la integridad de los datos.
- Operaciones transaccionales para los ajustes de inventario.
- Pruebas automatizadas para verificar el comportamiento de la solución.

## Autoría

Desarrollado como solución técnica para la prueba de Analista Desarrollador de Aplicaciones.