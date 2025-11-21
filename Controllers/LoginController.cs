using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using GerenciadorAD_Web.Configurations;
using GerenciadorAD_Web.Services;

namespace GerenciadorAD_Web.Controllers
{
    public class LoginController : Controller
    {
        private readonly AdConfig _config;
        private readonly IAdService _adService;

        public LoginController(IOptions<AdConfig> config, IAdService adService)
        {
            _config = config.Value;
            _adService = adService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Entrar(string usuario, string senha)
        {
            bool acessoPermitido = false;

            // 1. Verifica o "Master User" (Break-Glass Account)
            if (usuario == _config.MasterUser && senha == _config.MasterPass)
            {
                acessoPermitido = true;
            }
            // 2. Se não for Master, tenta validar no AD
            else if (_adService.ValidarLoginNoAd(usuario, senha))
            {
                // 3. Se validou senha, verifica se pertence ao grupo de Analistas
                if (_adService.UsuarioPertenceAoGrupo(usuario, _config.GrupoPermitido))
                {
                    acessoPermitido = true;
                }
                else
                {
                    ViewBag.Erro = $"Usuário não pertence ao grupo '{_config.GrupoPermitido}'.";
                    return View("Index");
                }
            }

            if (acessoPermitido)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, usuario),
                    new Claim(ClaimTypes.Role, "Admin")
                };
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Erro = "Usuário ou senha inválidos.";
            return View("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Sair()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Login");
        }
    }
}