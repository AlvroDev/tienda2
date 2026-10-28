# Tienda — Sistema distribuido: e-commerce + PaymentService

Trabajo Final Integrador de **Backend (Optativa II)** — UCCuyo.

Dos servicios .NET 8 independientes que se comunican por HTTP:

- **E-commerce** (`tienda`): productos, usuarios y órdenes, con autenticación JWT y roles.
- **PaymentService**: aprueba o rechaza el pago de una orden según una regla de negocio.

Cuando un cliente **confirma una orden**, el e-commerce le pide el pago al PaymentService mediante `HttpClient` y, según la respuesta, marca la orden como `Paid` o `PaymentRejected`.

**Opción elegida:** Opción 1 — PaymentService.

---

## Estructura del repositorio

Un solo repositorio con **dos carpetas**, cada una con su propia solución:

```
tienda2/
├── tienda.sln                       ← E-commerce
├── src/
│   ├── tienda.Domain/
│   ├── tienda.Application/
│   ├── tienda.Infrastructure/
│   └── tienda.WebApi/
└── PaymentService/
    ├── PaymentService.sln           ← Segundo servicio (independiente)
    └── src/
        ├── PaymentService.Domain/
        ├── PaymentService.Application/
        ├── PaymentService.Infrastructure/
        └── PaymentService.Api/
```

---

## Requisitos previos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- (Opcional, solo para crear migraciones nuevas) EF Core CLI:
  ```bash
  dotnet tool install --global dotnet-ef --version 8.0.11
  ```

---

## Cómo levantar los dos servicios

Se necesitan **dos terminales**, una por servicio. Conviene levantar primero el PaymentService.

**Terminal 1 — PaymentService** (puerto **5290**):

```bash
cd PaymentService
dotnet run --project src/PaymentService.Api --launch-profile http
```

Swagger: http://localhost:5290/swagger — para probarlo solo, primero **Authorize** con la API key `payments-dev-key-2026` (ver [Seguridad entre servicios](#decisión-la-llamada-entre-servicios-se-protege-con-una-api-key)).

**Terminal 2 — E-commerce** (puerto **5284**):

```bash
dotnet run --project src/tienda.WebApi --launch-profile http
```

Swagger: http://localhost:5284/swagger

### Base de datos

El e-commerce usa **SQLite**. El archivo `tienda.db` **no se versiona**: se crea solo al arrancar, porque `Program.cs` ejecuta `Database.MigrateAsync()`, que aplica todas las migraciones pendientes. No hace falta correr `dotnet ef database update` a mano.

### Puertos y URLs

| Servicio | URL | Dónde se configura |
|---|---|---|
| E-commerce | http://localhost:5284 | `src/tienda.WebApi/Properties/launchSettings.json` |
| PaymentService | http://localhost:5290 | `PaymentService/src/PaymentService.Api/Properties/launchSettings.json` |

La URL y la API key que usa el e-commerce para llamar al PaymentService están en `src/tienda.WebApi/appsettings.json`:

```json
"Services": {
  "Payment": "http://localhost:5290/",
  "PaymentApiKey": "payments-dev-key-2026"
}
```

> Si se cambia el puerto del PaymentService, hay que actualizar también `Payment`; si no, el e-commerce no lo encuentra y responde 503. Lo mismo con `PaymentApiKey`: tiene que coincidir con `ServiceAuth:ApiKey` del PaymentService.

---

## Usuario Admin de prueba

Al arrancar, el e-commerce crea automáticamente un usuario **Admin** (si todavía no existe), con los datos de la sección `SeedAdmin` de `appsettings.json`:

| Email | Contraseña | Rol |
|---|---|---|
| `admin@tienda.com` | `Admin123!` | Admin |

Los usuarios creados con `POST /api/Auth/register` siempre son **Customer**: nadie puede registrarse como Admin desde la API.

---

## Regla de negocio del PaymentService

> **Se aprueban los pagos de hasta $100.000 inclusive. Los pagos de más de $100.000 se rechazan.**

- La regla vive en el **dominio**: `Payment.Process()` en `PaymentService.Domain/Entities/Payment.cs`, con la constante `MaxApprovedAmount = 100_000`.
- Cada pago aprobado o rechazado recibe un `transactionId` (por ejemplo `TX-9F3C2A1B`).
- **Idempotencia:** si llega dos veces el pago de la misma orden (por ejemplo, por un reintento), se devuelve el resultado ya registrado y no se cobra de nuevo.
- **Almacenamiento en memoria:** el PaymentService no usa base de datos; guarda los pagos en un `ConcurrentDictionary` registrado como **Singleton**. Los pagos se pierden al reiniciar el servicio. La consigna lo permite para un servicio simple.

### Contrato HTTP entre los servicios

```http
POST http://localhost:5290/api/payments/process
Content-Type: application/json
X-Api-Key: payments-dev-key-2026

{ "orderId": "a1b2...", "amount": 45000 }
```

Respuesta:

```json
{ "orderId": "a1b2...", "amount": 45000, "status": "Approved", "transactionId": "TX-9F3C2A1B" }
```

Con `"amount": 150000` → `"status": "Rejected"`.

---

## Comunicación entre servicios

- **`HttpClient` con `IHttpClientFactory`** (typed client): `AddHttpClient<IPaymentClient, PaymentClient>` en `tienda.Infrastructure/InfrastructureServiceRegistration.cs`. No se instancia `HttpClient` con `new`.
- **URL en configuración**: `BaseAddress` sale de `Services:Payment` en `appsettings.json`.
- **Timeout**: 10 segundos.
- **API key**: cada llamada lleva el header `X-Api-Key`, que se agrega automáticamente en el `AddHttpClient` (`DefaultRequestHeaders`).
- **Contratos**: DTOs propios (`ProcessPaymentRequest` y `ProcessPaymentResponse` en `tienda.Infrastructure/ExternalServices/PaymentContracts.cs`), serializados con `System.Text.Json`. No se exponen entidades de dominio.
- **Capas**: Application solo conoce la interfaz `IPaymentClient`; la implementación con HTTP (`PaymentClient`) está en Infrastructure.

### Decisión: la llamada entre servicios va sin token

El PaymentService es un servicio **interno**: solo lo llama el e-commerce, no los clientes finales. La seguridad está en la **puerta de entrada**: para confirmar una orden, el cliente tiene que estar autenticado con JWT en el e-commerce, y solo puede confirmar sus propias órdenes. En un entorno real, el PaymentService no estaría expuesto a internet (red interna) o se protegería con un token propio entre servicios.

### Qué pasa si el PaymentService no responde

Si el PaymentService está apagado, la URL es incorrecta o tarda más de 10 segundos:

- `PaymentClient` captura el error (`try/catch`) y lanza `ServiceUnavailableException`.
- El cliente recibe **503 Service Unavailable** con el mensaje: *"El servicio de pagos no está disponible. La orden sigue pendiente; intentá confirmarla más tarde."*
- La orden **queda en `Pending`**, sin cambios ni stock perdido, y se puede volver a confirmar cuando el servicio vuelva.

---

## Estados de la orden

```
            ┌──── pago aprobado ────→  Paid
Pending ────┤
            └──── pago rechazado ───→  PaymentRejected  (se devuelve el stock)
```

- Al **crear** la orden se descuenta el stock de cada producto (regla en `Order.AddItem`).
- Solo se puede confirmar una orden `Pending`; confirmar otra vez una orden ya procesada devuelve **422**.
- Cada ítem guarda el precio y el nombre del producto **al momento de la compra**, así el historial no cambia si después cambia el producto.

---

## Reproducir el flujo completo (Swagger)

Con los dos servicios levantados, en http://localhost:5284/swagger:

1. **Login** → `POST /api/Auth/login` (por ejemplo, con el Admin). Copiar el `token`.
2. **Authorize** (botón arriba a la derecha) → escribir `Bearer {token}`.
3. **Crear un producto** (solo Admin) → `POST /api/Productos`:
   ```json
   { "nombre": "Mouse Logitech", "descripcion": "Mouse inalámbrico", "precio": 15000, "stock": 20 }
   ```
4. **Crear una orden** → `POST /api/Ordenes` con el id del producto:
   ```json
   { "items": [ { "productoId": "ID-DEL-PRODUCTO", "cantidad": 2 } ] }
   ```
   La orden queda `Pending`, con total $30.000.
5. **Confirmar la orden** → `POST /api/Ordenes/{id}/confirmar` → estado **`Paid`** con `transaccionId`.
6. **Caso rechazado**: crear una orden de 7 unidades ($105.000) y confirmarla → **`PaymentRejected`**, y el stock del producto vuelve a su valor anterior.
7. **Caso de fallo**: apagar el PaymentService (Ctrl+C), crear una orden y confirmarla → **503**; la orden sigue `Pending`. Levantar el servicio y confirmarla de nuevo → `Paid`.

---

## Endpoints

### E-commerce (http://localhost:5284)

| Método | Endpoint | Acceso | Tipo |
|---|---|---|---|
| POST | `/api/Auth/register` | Público | Command |
| POST | `/api/Auth/login` | Público | Command |
| GET | `/api/Productos` | Autenticado | Query |
| GET | `/api/Productos/{id}` | Autenticado | Query |
| POST | `/api/Productos` | **Solo Admin** | Command |
| POST | `/api/Ordenes` | Autenticado | Command |
| POST | `/api/Ordenes/{id}/confirmar` | Autenticado (dueño de la orden) | Command |
| GET | `/api/Ordenes/{id}` | Autenticado (dueño de la orden) | Query |
| GET | `/api/Ordenes/mis-ordenes` | Autenticado | Query |

### PaymentService (http://localhost:5290)

| Método | Endpoint | Tipo |
|---|---|---|
| POST | `/api/payments/process` | Command |
| GET | `/api/payments/{orderId}` | Query |

### Códigos de error

Los errores se manejan en un único `GlobalExceptionHandler` (`IExceptionHandler`) y se responden en formato **ProblemDetails** (RFC 7807).

| Código | Cuándo |
|---|---|
| 400 | Datos inválidos (FluentValidation) |
| 401 | Sin token, token inválido o credenciales incorrectas |
| 403 | Token válido pero sin el rol necesario |
| 404 | Recurso inexistente (o una orden de otro usuario) |
| 409 | Email ya registrado |
| 422 | Regla de negocio violada (stock insuficiente, orden ya procesada) |
| 503 | PaymentService no disponible |

---

## Arquitectura

Los **dos servicios** siguen Clean Architecture con las mismas cuatro capas y CQRS con MediatR.

| Capa | Responsabilidad | Depende de |
|---|---|---|
| **Domain** | Entidades, Value Objects, excepciones y reglas de negocio | Nada |
| **Application** | Commands, Queries y Handlers (MediatR), validadores, DTOs, interfaces (repositorios, `IPaymentClient`) | Domain |
| **Infrastructure** | Implementaciones: EF Core, repositorios, JWT, BCrypt, `PaymentClient` (e-commerce); repositorio en memoria (PaymentService) | Application, Domain |
| **Api / WebApi** | Controllers, `GlobalExceptionHandler`, `Program.cs` | Application, Infrastructure |

Patrones utilizados:

- **Clean Architecture** — regla de dependencia: Domain no depende de nada; Application no depende de Infrastructure.
- **CQRS con MediatR** — Commands y Queries separados, con un `ValidationBehavior` (FluentValidation) en el pipeline.
- **Repository Pattern** — interfaces en Application, implementaciones en Infrastructure.
- **Value Objects** — `Email` y `Money` (e-commerce); `Money` (PaymentService).
- **Manejo global de errores** — `GlobalExceptionHandler` con ProblemDetails.

---

## Tecnologías

| Tecnología | Versión | Uso |
|---|---|---|
| .NET | 8.0 | Framework base (ambos servicios) |
| Entity Framework Core (+ Sqlite, Design, Tools) | 8.0.11 | ORM y migraciones (e-commerce) |
| SQLite | — | Base de datos (e-commerce) |
| MediatR | 14.1.0 | CQRS (ambos servicios) |
| FluentValidation | 12.1.1 | Validación (ambos servicios) |
| BCrypt.Net-Next | 4.2.0 | Hash de contraseñas |
| Microsoft.AspNetCore.Authentication.JwtBearer | 8.0.0 | Autenticación JWT |
| Microsoft.Extensions.Http | 8.0.1 | `IHttpClientFactory` |
| Swashbuckle | — | Swagger (ambos servicios) |
