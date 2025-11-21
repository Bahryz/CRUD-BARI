using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using GerenciadorAD_Web.Models;
using Microsoft.AspNetCore.Authorization; // 1. Importante: Traz a ferramenta de segurança

namespace GerenciadorAD_Web.Controllers;

[Authorize] // 2. O CADEADO: Esta é a linha que faltava! Sem ela, entra qualquer um.
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
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