using System.Diagnostics;
using Hotel.Web.Models;
using Hotel.Web.Repositorios;
using Microsoft.AspNetCore.Mvc;

namespace Hotel.Web.Controllers;

public class HomeController : Controller
{
    private readonly HabitacionRepositorio repo;

    public HomeController(HabitacionRepositorio repo)
    {
        this.repo = repo;
    }

    public async Task<IActionResult> Index()
    {
        return View(await repo.ListarTiposAsync());
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
