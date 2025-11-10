using GerenciadorAD_Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.DirectoryServices.AccountManagement;

namespace GerenciadorAD_Web.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly string _dominio;
        private readonly string _usuarioAdmin;
        private readonly string _senhaAdmin;

        public UsuarioController(IConfiguration config)
        {
            _dominio      = config.GetValue<string>("AdConfig:Dominio")!;
            _usuarioAdmin = config.GetValue<string>("AdConfig:UsuarioAdmin")!;
            _senhaAdmin   = config.GetValue<string>("AdConfig:SenhaAdmin")!;
        }

        private PrincipalContext GetContext()
        {
            return new PrincipalContext(ContextType.Domain, _dominio, _usuarioAdmin, _senhaAdmin);
        }

        [HttpGet]
        public IActionResult Index() => View();

        [HttpGet]
        public IActionResult Criar() => View(new UserViewModel { Dominio = _dominio });

        [HttpPost]
        public IActionResult Criar(UserViewModel model)
        {
            if (string.IsNullOrEmpty(model.Login) || string.IsNullOrEmpty(model.Senha))
            {
                ViewBag.MensagemErro = "Login e Senha são obrigatórios.";
                return View("Criar", model);
            }

            try
            {
                using (var context = GetContext())
                {
                    if (UserPrincipal.FindByIdentity(context, model.Login) != null)
                    {
                        ViewBag.MensagemErro = $"Usuário '{model.Login}' já existe.";
                        return View("Criar", model);
                    }

                    var usuario = new UserPrincipal(context)
                    {
                        SamAccountName = model.Login,
                        UserPrincipalName = $"{model.Login}@{_dominio}"
                    };
                    usuario.SetPassword(model.Senha);
                    usuario.Enabled = true;
                    usuario.Save();

                    if (!string.IsNullOrEmpty(model.Grupo))
                    {
                        var grupo = GroupPrincipal.FindByIdentity(context, model.Grupo);
                        if (grupo != null)
                        {
                            grupo.Members.Add(usuario);
                            grupo.Save();
                        }
                        else
                        {
                            ViewBag.MensagemErro = $"Usuário criado, mas o grupo '{model.Grupo}' não foi encontrado.";
                            return View("Criar", model);
                        }
                    }
                }

                ViewBag.MensagemSucesso = "Usuário criado com sucesso!";
                return View("Criar", new UserViewModel { Dominio = _dominio });
            }
            catch (Exception ex)
            {
                ViewBag.MensagemErro = $"Erro: {ex.Message}";
                return View("Criar", model);
            }
        }

        [HttpGet]
        public IActionResult ResetSenha() => View(new UserViewModel { Dominio = _dominio });

        [HttpPost]
        public IActionResult ResetSenha(UserViewModel model)
        {
            if (string.IsNullOrEmpty(model.Login) || string.IsNullOrEmpty(model.NovoSenha))
            {
                ViewBag.MensagemErro = "Login e nova senha são obrigatórios.";
                return View("ResetSenha", model);
            }

            try
            {
                using (var context = GetContext())
                {
                    var usuario = UserPrincipal.FindByIdentity(context, model.Login);
                    if (usuario == null)
                    {
                        ViewBag.MensagemErro = "Usuário não encontrado.";
                        return View("ResetSenha", model);
                    }
                    usuario.SetPassword(model.NovoSenha);
                    usuario.Save();
                }

                ViewBag.MensagemSucesso = "Senha redefinida com sucesso!";
                return View("ResetSenha", new UserViewModel { Dominio = _dominio });
            }
            catch (Exception ex)
            {
                ViewBag.MensagemErro = $"Erro: {ex.Message}";
                return View("ResetSenha", model);
            }
        }

        [HttpGet]
        public IActionResult Excluir() => View(new UserViewModel { Dominio = _dominio });

        [HttpPost]
        public IActionResult Excluir(UserViewModel model)
        {
            if (string.IsNullOrEmpty(model.Login))
            {
                ViewBag.MensagemErro = "Login é obrigatório.";
                return View("Excluir", model);
            }

            try
            {
                using (var context = GetContext())
                {
                    var usuario = UserPrincipal.FindByIdentity(context, model.Login);
                    if (usuario == null)
                    {
                        ViewBag.MensagemErro = "Usuário não encontrado.";
                        return View("Excluir", model);
                    }
                    usuario.Delete();
                }

                ViewBag.MensagemSucesso = "Usuário excluído com sucesso!";
                return View("Excluir", new UserViewModel { Dominio = _dominio });
            }
            catch (Exception ex)
            {
                ViewBag.MensagemErro = $"Erro: {ex.Message}";
                return View("Excluir", model);
            }
        }

        [HttpGet]
        public IActionResult AdicionarGrupo() => View(new UserViewModel { Dominio = _dominio });

        [HttpPost]
        public IActionResult AdicionarGrupo(UserViewModel model)
        {
            if (string.IsNullOrEmpty(model.Login) || string.IsNullOrEmpty(model.Grupo))
            {
                ViewBag.MensagemErro = "Login e Grupo são obrigatórios.";
                return View("AdicionarGrupo", model);
            }

            try
            {
                using (var context = GetContext())
                {
                    var usuario = UserPrincipal.FindByIdentity(context, model.Login);
                    var grupo = GroupPrincipal.FindByIdentity(context, model.Grupo);
                    if (usuario == null || grupo == null)
                    {
                        ViewBag.MensagemErro = "Usuário ou grupo não encontrado.";
                        return View("AdicionarGrupo", model);
                    }
                    grupo.Members.Add(usuario);
                    grupo.Save();
                }

                ViewBag.MensagemSucesso = "Usuário adicionado ao grupo!";
                return View("AdicionarGrupo", new UserViewModel { Dominio = _dominio });
            }
            catch (Exception ex)
            {
                ViewBag.MensagemErro = $"Erro: {ex.Message}";
                return View("AdicionarGrupo", model);
            }
        }

        [HttpGet]
        public IActionResult RemoverGrupo() => View(new UserViewModel { Dominio = _dominio });

        [HttpPost]
        public IActionResult RemoverGrupo(UserViewModel model)
        {
            if (string.IsNullOrEmpty(model.Login) || string.IsNullOrEmpty(model.Grupo))
            {
                ViewBag.MensagemErro = "Login e Grupo são obrigatórios.";
                return View("RemoverGrupo", model);
            }

            try
            {
                using (var context = GetContext())
                {
                    var usuario = UserPrincipal.FindByIdentity(context, model.Login);
                    var grupo = GroupPrincipal.FindByIdentity(context, model.Grupo);
                    if (usuario == null || grupo == null)
                    {
                        ViewBag.MensagemErro = "Usuário ou grupo não encontrado.";
                        return View("RemoverGrupo", model);
                    }
                    grupo.Members.Remove(usuario);
                    grupo.Save();
                }

                ViewBag.MensagemSucesso = "Usuário removido do grupo!";
                return View("RemoverGrupo", new UserViewModel { Dominio = _dominio });
            }
            catch (Exception ex)
            {
                ViewBag.MensagemErro = $"Erro: {ex.Message}";
                return View("RemoverGrupo", model);
            }
        }
    }
}