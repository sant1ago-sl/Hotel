using Hotel.Web.Models;
using Hotel.Web.Repositorios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hotel.Web.Controllers;

public class HabitacionesController : Controller
{
    private readonly HabitacionRepositorio repo;

    public HabitacionesController(HabitacionRepositorio repo)
    {
        this.repo = repo;
    }

    public async Task<IActionResult> Index(int? idTipoHabitacion)
    {
        await CargarTipos(idTipoHabitacion);
        return View(await repo.ListarAsync(idTipoHabitacion));
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var hab = await repo.ObtenerAsync(id.Value);
        if (hab == null) return NotFound();

        return View(hab);
    }

    public async Task<IActionResult> Create()
    {
        await CargarTipos();
        return View(new Habitacion());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Numero,Piso,IdTipoHabitacion")] Habitacion hab)
    {
        ModelState.Remove(nameof(Habitacion.IdTipoHabitacionNavigation));
        await Validar(hab);

        if (!ModelState.IsValid)
        {
            await CargarTipos(hab.IdTipoHabitacion);
            return View(hab);
        }

        await repo.CrearAsync(hab);
        TempData["Mensaje"] = $"Habitación {hab.Numero} registrada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var hab = await repo.ObtenerAsync(id.Value);
        if (hab == null) return NotFound();

        await CargarTipos(hab.IdTipoHabitacion);
        return View(hab);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("IdHabitacion,Numero,Piso,IdTipoHabitacion")] Habitacion hab)
    {
        if (id != hab.IdHabitacion) return NotFound();

        ModelState.Remove(nameof(Habitacion.IdTipoHabitacionNavigation));
        await Validar(hab);

        if (!ModelState.IsValid)
        {
            await CargarTipos(hab.IdTipoHabitacion);
            return View(hab);
        }

        if (!await repo.EditarAsync(hab))
            return NotFound();

        TempData["Mensaje"] = $"Habitación {hab.Numero} actualizada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var hab = await repo.ObtenerAsync(id.Value);
        if (hab == null) return NotFound();

        return View(hab);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var hab = await repo.ObtenerAsync(id);
        if (hab == null) return NotFound();

        await repo.EliminarAsync(id);
        TempData["Mensaje"] = $"Habitación {hab.Numero} eliminada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    private async Task Validar(Habitacion hab)
    {
        if (string.IsNullOrWhiteSpace(hab.Numero))
        {
            ModelState.Remove(nameof(Habitacion.Numero));
            ModelState.AddModelError(nameof(Habitacion.Numero), "El número es obligatorio.");
        }
        else if (hab.Numero.Length > 10)
        {
            ModelState.AddModelError(nameof(Habitacion.Numero), "El número no puede superar los 10 caracteres.");
        }
        else if (await repo.ExisteNumeroAsync(hab.Numero, hab.IdHabitacion))
        {
            ModelState.AddModelError(nameof(Habitacion.Numero), "Ya existe una habitación con este número.");
        }

        if (hab.Piso < 0 || hab.Piso > 50)
            ModelState.AddModelError(nameof(Habitacion.Piso), "El piso debe estar entre 0 y 50.");

        if (hab.IdTipoHabitacion <= 0)
            ModelState.AddModelError(nameof(Habitacion.IdTipoHabitacion), "Seleccione un tipo de habitación.");
        else if (!await repo.ExisteTipoAsync(hab.IdTipoHabitacion))
            ModelState.AddModelError(nameof(Habitacion.IdTipoHabitacion), "El tipo seleccionado no existe o está inactivo.");
    }

    private async Task CargarTipos(int? elegido = null)
    {
        var tipos = await repo.ListarTiposAsync();
        ViewBag.Tipos = new SelectList(tipos, nameof(TipoHabitacion.IdTipoHabitacion), nameof(TipoHabitacion.Nombre), elegido);
    }
}
