# MediTrack - Identity & Profile Service

Microservicio de identidad de MediTrack. Registra usuarios, autentica credenciales
y emite los JSON Web Tokens (JWT) que el resto de servicios consume.

## Rol en la arquitectura

- Identity Service: emite el token (sign-up / sign-in).
- API Gateway: valida el token y autoriza por rol.
- Cada microservicio: valida la firma del token de forma independiente.

## Stack

- .NET 8 (ASP.NET Core Web API)
- Entity Framework Core 8 + MySQL / TiDB
- Arquitectura DDD por bounded context (IAM) + CQRS
- BCrypt para el hash de contraseñas
- JWT para emisión y validación

## Endpoints

| Método | Ruta | Auth | Descripción |
| ------ | ---- | ---- | ----------- |
| POST | `/api/v1/authentication/sign-up` | Anónimo | Registra un usuario y devuelve su JWT |
| POST | `/api/v1/authentication/sign-in` | Anónimo | Autentica credenciales y devuelve su JWT |
| GET | `/api/v1/users/{id}` | Bearer | Devuelve el perfil del usuario |

Roles disponibles: `Patient` y `TechnicalStaff`.

## Configuración JWT

La sección `Jwt` de `appsettings.json` (Issuer, Audience) debe ser idéntica
en todos los microservicios para que la validación de firma compartida funcione.
`Jwt:Key` se deja vacío a propósito -- cada dev lo configura una vez en su máquina:

```bash
dotnet user-secrets set "Jwt:Key" "<pedile la clave al equipo>" --project MediTrack.IdentityService.API
```

En producción esa misma variable se setea como `Jwt__Key` en el entorno del
proveedor de deploy (Render, etc.) -- nunca en un archivo del repo.

## Base de datos

⚠️ **Acción requerida**: `ConnectionStrings:DefaultConnection` apuntaba a una
cuenta REAL de TiDB Cloud cuya contraseña estaba commiteada en git. Hay que
rotar esa contraseña desde la consola de TiDB Cloud (no se puede hacer por
código) y configurar el connection string nuevo con:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=...;Port=4000;Database=identity_db;User=...;Password=...;SslMode=VerifyFull;" --project MediTrack.IdentityService.API
```

El esquema se crea al arrancar con `EnsureCreatedAsync()`. Para adoptar migraciones
EF: `dotnet ef migrations add InitialCreate` y reemplazar `EnsureCreatedAsync()` por
`MigrateAsync()` en `Program.cs`.

## Ejecución local

```bash
dotnet run --project MediTrack.IdentityService.API
```

Swagger UI en `http://localhost:5090/swagger`. Ejemplos de requests en
`MediTrack.IdentityService.API/MediTrack.IdentityService.API.http`.

## Estructura (bounded context IAM)

```
IAM/
  Domain/         Model (User, UserRole, Commands, Queries), Repositories, Services
  Application/    Internal/CommandServices, QueryServices, OutboundServices
  Infrastructure/ Persistence (EFC), Hashing (BCrypt), Tokens (JWT), Pipeline
  Interfaces/     REST (Controllers, Resources, Transform)
```
