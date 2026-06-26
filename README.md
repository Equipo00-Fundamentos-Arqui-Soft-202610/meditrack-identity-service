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

La sección `Jwt` de `appsettings.json` (Issuer, Audience, Key) debe ser idéntica
en todos los microservicios para que la validación de firma compartida funcione.
La clave actual es provisional y debe gestionarse fuera del repositorio antes de
cualquier despliegue real.

## Base de datos

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
