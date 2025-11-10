using GerenciadorAD_Web.Models; // Importa o ErrorViewModel
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace GerenciadorAD_Web.Controllers
{
    public class HomeController : Controller
    {
        // Este método GET: /Home/Index
        // Será executado quando a aplicação iniciar
        [HttpGet]
        public IActionResult Index()
        {
            // Ele simplesmente retorna a sua View do menu principal
            return View(); // Retorna o ficheiro Views/Home/Index.cshtml
        }

        // Este método é para a página de Erro
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}