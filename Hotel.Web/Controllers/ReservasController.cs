using Hotel.Web.Models;
using Hotel.Web.Repositorios;
using Hotel.Web.Resultados;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hotel.Web.Controllers;

public class ReservasController : Controller
{
    private readonly ReservaRepositorio repo;

    public ReservasController(ReservaRepositorio repo)
    {
        this.repo = repo;
    }

    public async Task<IActionResult> Index()
    {
        return View(await repo.ListarAsync());
    }

    public async Task<IActionResult> Disponibles(DateOnly? ingreso, DateOnly? salida)
    {
        ViewBag.Ingreso = ingreso;
        ViewBag.Salida = salida;

        if (ingreso == null || salida == null)
            return View(null);

        if (salida <= ingreso)
        {
            ModelState.AddModelError(string.Empty, "La fecha de salida debe ser posterior a la de ingreso.");
            return View(null);
        }

        int noches = salida.Value.DayNumber - ingreso.Value.DayNumber;
        var libres = await repo.DisponiblesAsync(ingreso.Value, salida.Value);

        var lista = libres.Select(h => new HabitacionDisponible
        {
            Habitacion = h,
            Noches = noches,
            Total = noches * h.IdTipoHabitacionNavigation.PrecioNoche
        }).ToList();

        return View(lista);
    }

    public async Task<IActionResult> Create()
    {
        await CargarListas();
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        return View(new Reserva { FechaIngreso = hoy, FechaSalida = hoy.AddDays(1) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("IdHuesped,IdHabitacion,FechaIngreso,FechaSalida,Observaciones")] Reserva reserva)
    {
        ModelState.Remove(nameof(Reserva.IdHuespedNavigation));
        ModelState.Remove(nameof(Reserva.IdHabitacionNavigation));
        await Validar(reserva);

        if (!ModelState.IsValid)
        {
            await CargarListas(reserva.IdHuesped, reserva.IdHabitacion);
            return View(reserva);
        }

        await repo.CrearAsync(reserva);
        TempData["Mensaje"] = "Reserva registrada correctamente (Pendiente).";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(int id)
    {
        if (await repo.CancelarAsync(id))
            TempData["Mensaje"] = "Reserva cancelada correctamente.";
        else
            TempData["Mensaje"] = "Solo se pueden cancelar reservas Pendiente o Confirmada.";

        return RedirectToAction(nameof(Index));
    }

    private async Task Validar(Reserva reserva)
    {
        if (reserva.IdHuesped <= 0)
            ModelState.AddModelError(nameof(Reserva.IdHuesped), "Seleccione un huésped.");
        else if (!await repo.ExisteHuespedAsync(reserva.IdHuesped))
            ModelState.AddModelError(nameof(Reserva.IdHuesped), "El huésped seleccionado no existe o está inactivo.");

        bool habOk = false;
        if (reserva.IdHabitacion <= 0)
            ModelState.AddModelError(nameof(Reserva.IdHabitacion), "Seleccione una habitación.");
        else if (!await repo.ExisteHabitacionAsync(reserva.IdHabitacion))
            ModelState.AddModelError(nameof(Reserva.IdHabitacion), "La habitación seleccionada no existe o está inactiva.");
        else
            habOk = true;

        bool fechasOk = true;
        if (reserva.FechaSalida <= reserva.FechaIngreso)
        {
            ModelState.AddModelError(nameof(Reserva.FechaSalida), "La fecha de salida debe ser posterior a la de ingreso.");
            fechasOk = false;
        }

        if (reserva.FechaIngreso < DateOnly.FromDateTime(DateTime.Today))
        {
            ModelState.AddModelError(nameof(Reserva.FechaIngreso), "La fecha de ingreso no puede ser anterior a hoy.");
            fechasOk = false;
        }

        if (reserva.Observaciones?.Length > 200)
            ModelState.AddModelError(nameof(Reserva.Observaciones), "Las observaciones no pueden superar los 200 caracteres.");

        if (habOk && fechasOk
            && await repo.HayCruceAsync(reserva.IdHabitacion, reserva.FechaIngreso, reserva.FechaSalida))
        {
            ModelState.AddModelError(nameof(Reserva.IdHabitacion), "La habitación ya está reservada en ese rango de fechas.");
        }
    }

    private async Task CargarListas(int? huesped = null, int? habitacion = null)
    {
        var huespedes = (await repo.ListarHuespedesAsync())
            .Select(h => new { h.IdHuesped, Texto = $"{h.Apellidos}, {h.Nombres} ({h.Dni})" });
        ViewBag.Huespedes = new SelectList(huespedes, "IdHuesped", "Texto", huesped);

        var habitaciones = (await repo.ListarHabitacionesAsync())
            .Select(h => new
            {
                h.IdHabitacion,
                Texto = $"{h.Numero} - {h.IdTipoHabitacionNavigation.Nombre} (S/ {h.IdTipoHabitacionNavigation.PrecioNoche:0.00}/noche)"
            });
        ViewBag.Habitaciones = new SelectList(habitaciones, "IdHabitacion", "Texto", habitacion);
    }
}
