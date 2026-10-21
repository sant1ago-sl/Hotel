using Hotel.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Web.Repositorios;

public class ReservaRepositorio
{
    private readonly HotelContext db;

    public ReservaRepositorio(HotelContext db)
    {
        this.db = db;
    }

    public async Task<List<Habitacion>> DisponiblesAsync(DateOnly ingreso, DateOnly salida)
    {
        return await db.Habitacions
            .AsNoTracking()
            .Include(h => h.IdTipoHabitacionNavigation)
            .Where(h => h.Activo
                && !h.Reservas.Any(r => r.Estado != "Cancelada"
                    && r.FechaIngreso < salida
                    && r.FechaSalida > ingreso))
            .OrderBy(h => h.Numero)
            .ToListAsync();
    }

    public async Task<bool> HayCruceAsync(int idHabitacion, DateOnly ingreso, DateOnly salida)
    {
        return await db.Reservas.AnyAsync(r => r.IdHabitacion == idHabitacion
            && r.Estado != "Cancelada"
            && r.FechaIngreso < salida
            && r.FechaSalida > ingreso);
    }

    public async Task<List<Reserva>> ListarAsync()
    {
        return await db.Reservas
            .AsNoTracking()
            .Include(r => r.IdHuespedNavigation)
            .Include(r => r.IdHabitacionNavigation)
                .ThenInclude(h => h.IdTipoHabitacionNavigation)
            .OrderByDescending(r => r.FechaIngreso)
            .ThenByDescending(r => r.IdReserva)
            .ToListAsync();
    }

    public async Task<List<Huesped>> ListarHuespedesAsync()
    {
        return await db.Huespeds
            .AsNoTracking()
            .Where(h => h.Activo)
            .OrderBy(h => h.Apellidos)
            .ThenBy(h => h.Nombres)
            .ToListAsync();
    }

    public async Task<List<Habitacion>> ListarHabitacionesAsync()
    {
        return await db.Habitacions
            .AsNoTracking()
            .Include(h => h.IdTipoHabitacionNavigation)
            .Where(h => h.Activo)
            .OrderBy(h => h.Numero)
            .ToListAsync();
    }

    public async Task<bool> ExisteHuespedAsync(int id)
    {
        return await db.Huespeds.AnyAsync(h => h.IdHuesped == id && h.Activo);
    }

    public async Task<bool> ExisteHabitacionAsync(int id)
    {
        return await db.Habitacions.AnyAsync(h => h.IdHabitacion == id && h.Activo);
    }

    public async Task CrearAsync(Reserva reserva)
    {
        reserva.Estado = "Pendiente";
        db.Reservas.Add(reserva);
        await db.SaveChangesAsync();
    }

    public async Task<bool> CancelarAsync(int id)
    {
        var reserva = await db.Reservas
            .FirstOrDefaultAsync(r => r.IdReserva == id
                && (r.Estado == "Pendiente" || r.Estado == "Confirmada"));
        if (reserva == null) return false;

        reserva.Estado = "Cancelada";
        await db.SaveChangesAsync();
        return true;
    }
}
