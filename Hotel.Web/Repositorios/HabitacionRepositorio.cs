using Hotel.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Web.Repositorios;

public class HabitacionRepositorio
{
    private readonly HotelContext db;

    public HabitacionRepositorio(HotelContext db)
    {
        this.db = db;
    }

    public async Task<List<Habitacion>> ListarAsync(int? tipo)
    {
        var lista = db.Habitacions
            .AsNoTracking()
            .Include(h => h.IdTipoHabitacionNavigation)
            .Where(h => h.Activo);

        if (tipo.HasValue)
            lista = lista.Where(h => h.IdTipoHabitacion == tipo.Value);

        return await lista.OrderBy(h => h.Numero).ToListAsync();
    }

    public async Task<Habitacion?> ObtenerAsync(int id)
    {
        return await db.Habitacions
            .AsNoTracking()
            .Include(h => h.IdTipoHabitacionNavigation)
            .FirstOrDefaultAsync(h => h.IdHabitacion == id && h.Activo);
    }

    public async Task<List<TipoHabitacion>> ListarTiposAsync()
    {
        return await db.TipoHabitacions
            .AsNoTracking()
            .Where(t => t.Activo)
            .OrderBy(t => t.Nombre)
            .ToListAsync();
    }

    public async Task<bool> ExisteTipoAsync(int id)
    {
        return await db.TipoHabitacions.AnyAsync(t => t.IdTipoHabitacion == id && t.Activo);
    }

    public async Task<bool> ExisteNumeroAsync(string numero, int idActual = 0)
    {
        return await db.Habitacions.AnyAsync(h => h.Numero == numero && h.IdHabitacion != idActual);
    }

    public async Task CrearAsync(Habitacion hab)
    {
        hab.Activo = true;
        db.Habitacions.Add(hab);
        await db.SaveChangesAsync();
    }

    public async Task<bool> EditarAsync(Habitacion nueva)
    {
        var hab = await db.Habitacions
            .FirstOrDefaultAsync(h => h.IdHabitacion == nueva.IdHabitacion && h.Activo);
        if (hab == null) return false;

        hab.Numero = nueva.Numero;
        hab.Piso = nueva.Piso;
        hab.IdTipoHabitacion = nueva.IdTipoHabitacion;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        var hab = await db.Habitacions
            .FirstOrDefaultAsync(h => h.IdHabitacion == id && h.Activo);
        if (hab == null) return false;

        hab.Activo = false;
        await db.SaveChangesAsync();
        return true;
    }
}
