# Hotel.Web — Reservas de hotel con ASP.NET Core MVC y EF Core (Database First)

Aplicación para gestionar habitaciones y reservas de un hotel.

- Base de datos `HotelDB` en SQL Server LocalDB (`TipoHabitacion`, `Habitacion`, `Huesped`, `Reserva`).
- Modelo generado con scaffold en `Models/` (no se edita a mano).
- Acceso a datos solo en `Repositorios/`; los controladores no usan `HotelContext`.
- Eliminación lógica (`Activo = 0`), Post/Redirect/Get y mensajes con `TempData`.

## Requisitos

- .NET SDK 10
- SQL Server LocalDB con la base `HotelDB`

## Ejecutar

```bash
cd Hotel.Web
dotnet run --urls http://localhost:5390
```

En VS Code: perfil **Hotel.Web** y F5.

Cadena de conexión (en `Hotel.Web/appsettings.json`):

```
Server=(localdb)\MSSQLLocalDB;Database=HotelDB;Trusted_Connection=True;TrustServerCertificate=True
```

## Regenerar el modelo

Desde `Hotel.Web`:

```bash
dotnet tool restore
dotnet ef dbcontext scaffold "Server=(localdb)\MSSQLLocalDB;Database=HotelDB;Trusted_Connection=True;TrustServerCertificate=True" Microsoft.EntityFrameworkCore.SqlServer --output-dir Models --context HotelContext --no-onconfiguring --force
```

## Estructura

```
Hotel.Web/
├── Models/         generado con scaffold
├── Repositorios/   HabitacionRepositorio · ReservaRepositorio
├── Resultados/     HabitacionDisponible (resultado de la búsqueda de disponibilidad)
├── Controllers/    Home · Habitaciones · Reservas
└── Views/
```

## Funciones

- **Habitaciones:** listado con filtro por tipo, detalle, alta, edición y eliminación lógica.
- **Disponibilidad:** habitaciones libres entre dos fechas, con noches y total estimado.
- **Reservas:** alta con validación de fechas y cruces (queda `Pendiente`) y cancelación de las `Pendiente` o `Confirmada`.
