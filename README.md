# MediTrack — Identity & Profile Service (IAM)

Microservicio de **Identity & Access Management** del sistema MediTrack. Es la
autoridad de identidad de la plataforma: registra usuarios, autentica credenciales
y **emite los JSON Web Tokens (JWT)** que el resto de microservicios consume.

## Rol dentro de la arquitectura

Según **CON-04** y **AC-01** del informe, la seguridad se reparte en tres capas:

| Responsabilidad | Componente |
| --------------- | ---------- |
| **Emitir** el token (sign-up / sign-in) | **Identity Service** (este repo) |
| **Autorizar por rol** (punto principal) | API Gateway |
| **Validar la firma** del token de forma independiente | Cada microservicio (defensa en profundidad) |

Este servicio también valida la firma localmente, ya que expone el endpoint
protegido `GET /api/v1/users/{id}`.

## Stack

- .NET 8 — ASP.NET Core Web API
- Entity Framework Core 8 + MySQL / TiDB Cloud
- Arquitectura DDD por bounded context (`IAM`) + CQRS
- BCrypt para hashing de contraseñas
- JWT (`System.IdentityModel.Tokens.Jwt`) para emisión y `JwtBearer` para validación

## Endpoints

| Método | Ruta | Auth | Descripción |
| ------ | ---- | ---- | ----------- |
| POST | `/api/v1/authentication/sign-up` | Anónimo | Registra un usuario y devuelve su JWT |
| POST | `/api/v1/authentication/sign-in` | Anónimo | Autentica credenciales y devuelve su JWT |
| GET | `/api/v1/users/{id}` | Bearer | Devuelve el perfil del usuario |

### Roles

`Patient` y `TechnicalStaff` (los dos segmentos objetivo del informe). El rol
viaja como claim dentro del JWT.

## Configuración JWT (importante)

La sección `Jwt` de `appsettings.json` debe ser **idéntica en todos los
microservicios** para que la validación de firma compartida funcione:

```json
"Jwt": {
  "Issuer": "meditrack-gateway",
  "Audience": "meditrack-services",
  "Key": "<misma clave secreta en todos los servicios, mínimo 32 caracteres>",
  "ExpiresInHours": 8
}
```

> ⚠️ La `Key` del repositorio es un placeholder de desarrollo
> (`CHANGE_ME_...`). En cualquier entorno real debe reemplazarse por un secreto
> gestionado fuera del control de versiones.

## Base de datos

El servicio crea el schema al arrancar con `EnsureCreatedAsync()`. Todavía **no
hay migraciones EF**. Para adoptarlas (como en Treatment Service):

```bash
dotnet ef migrations add InitialCreate
# luego cambiar EnsureCreatedAsync() por MigrateAsync() en Program.cs
```

## Ejecución local

```bash
dotnet run --project MediTrack.IdentityService.API
```

Swagger UI: `http://localhost:5090/swagger`. Ejemplos de requests en
`MediTrack.IdentityService.API/MediTrack.IdentityService.API.http`.

## Estructura (bounded context IAM)

```
IAM/
  Domain/         Model (User, UserRole, Commands, Queries), Repositories, Services
  Application/    Internal/CommandServices, QueryServices, OutboundServices
  Infrastructure/ Persistence (EFC), Hashing (BCrypt), Tokens (JWT), Pipeline
  Interfaces/     REST (Controllers, Resources, Transform)
```
