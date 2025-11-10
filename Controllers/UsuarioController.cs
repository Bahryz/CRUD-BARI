using System;
using System.DirectoryServices.AccountManagement;
using GerenciadorAD_Web.Models;
using Microsoft.AspNetCore.Mvc; // <-- ESTA É A LINHA CORRETA!

namespace GerenciadorAD_Web.Controllers
{
    // Controller que manipula requisições de usuário da interface web
    public class UsuarioController : Controller
    {
        // ----------------------------------------
        // CRIAR USUÁRIO
        // ----------------------------------------

        // GET: /Usuario/Criar
        // Este método MOSTRA o formulário de criação
        [HttpGet]
        public IActionResult Criar()
        {
            return View(); // Retorna o arquivo Views/Usuario/Criar.cshtml
        }

        // POST: /Usuario/Criar
        // Este método RECEBE os dados do formulário de criação
        [HttpPost]
        public IActionResult Criar([FromForm] UserViewModel model)
        {
            try
            {
                using (var contexto = new PrincipalContext(ContextType.Domain, model.Dominio))
                {
                    var usuario = new UserPrincipal(contexto)
                    {
                        SamAccountName = model.Login
                        // Pode adicionar outras propriedades, ex: Name, EmailAddress etc.
                    };
                    usuario.SetPassword(model.Senha);
                    usuario.Save();

                    var grupoAD = GroupPrincipal.FindByIdentity(contexto, model.Grupo);
                    if (grupoAD != null && !string.IsNullOrEmpty(model.Grupo)) // Adicionada verificação
                    {
                        grupoAD.Members.Add(usuario);
                        grupoAD.Save();
                    }
                }
                // MELHORIA: Envia uma mensagem de sucesso de volta para a View
                ViewBag.MensagemSucesso = "Usuário criado com sucesso!";
                return View("Criar"); // Retorna para a página de Criar com a mensagem
            }
            catch (Exception ex)
            {
                // MELHORIA: Envia a mensagem de erro de volta para a View
                ViewBag.MensagemErro = "Erro: " + ex.Message;
                return View("Criar"); // Retorna para a página de Criar com o erro
            }
        }

        // ----------------------------------------
        // RESETAR SENHA
        // ----------------------------------------

        // GET: /Usuario/ResetSenha
        // Este método MOSTRA o formulário de reset de senha
        [HttpGet]
        public IActionResult ResetSenha()
        {
            return View(); // Retorna o arquivo Views/Usuario/ResetSenha.cshtml
        }

        // POST: /Usuario/ResetSenha
        // Este método RECEBE os dados do formulário de reset de senha
        [HttpPost]
        public IActionResult ResetSenha([FromForm] UserViewModel model)
        {
            try
            {
                using (var contexto = new PrincipalContext(ContextType.Domain, model.Dominio))
                {
                    var usuario = UserPrincipal.FindByIdentity(contexto, model.Login);
                    if (usuario == null)
                    {
                        ViewBag.MensagemErro = "Usuário não encontrado.";
                        return View("ResetSenha");
                    }
                    usuario.SetPassword(model.NovoSenha);
                    usuario.Save();

                    ViewBag.MensagemSucesso = "Senha alterada com sucesso.";
                    return View("ResetSenha");
                }
            }
            catch (Exception ex)
            {
                ViewBag.MensagemErro = "Erro: " + ex.Message;
                return View("ResetSenha");
            }
        }

        // ----------------------------------------
        // EXCLUIR USUÁRIO
        // ----------------------------------------

        // GET: /Usuario/Excluir
        // Este método MOSTRA o formulário de exclusão
        [HttpGet]
        public IActionResult Excluir()
        {
            return View(); // Retorna o arquivo Views/Usuario/Excluir.cshtml
        }

        // POST: /Usuario/Excluir
        // Este método RECEBE os dados do formulário de exclusão
        [HttpPost]
        public IActionResult Excluir([FromForm] UserViewModel model)
        {
            try
            {
                using (var contexto = new PrincipalContext(ContextType.Domain, model.Dominio))
                {
                    var usuario = UserPrincipal.FindByIdentity(contexto, model.Login);
                    if (usuario == null)
                    {
                        ViewBag.MensagemErro = "Usuário não encontrado.";
                        return View("Excluir");
                    }
                    usuario.Delete();
                    ViewBag.MensagemSucesso = "Usuário excluído com sucesso.";
                    return View("Excluir");
                }
            }
            catch (Exception ex)
            {
                ViewBag.MensagemErro = "Erro: " + ex.Message;
                return View("Excluir");
            }
        }

        // ----------------------------------------
        // ADICIONAR USUÁRIO A GRUPO
        // ----------------------------------------

        // GET: /Usuario/AdicionarGrupo
        // Este método MOSTRA o formulário de adicionar grupo
        [HttpGet]
        public IActionResult AdicionarGrupo()
        {
            return View(); // Retorna o arquivo Views/Usuario/AdicionarGrupo.cshtml
        }

        // POST: /Usuario/AdicionarGrupo
        // Este método RECEBE os dados do formulário
        [HttpPost]
        public IActionResult AdicionarGrupo([FromForm] UserViewModel model)
        {
            try
            {
                using (var contexto = new PrincipalContext(ContextType.Domain, model.Dominio))
                {
                    var usuario = UserPrincipal.FindByIdentity(contexto, model.Login);
                    var grupoAD = GroupPrincipal.FindByIdentity(contexto, model.Grupo);
                    if (usuario == null || grupoAD == null)
                    {
                        ViewBag.MensagemErro = "Usuário ou grupo não encontrado.";
                        return View("AdicionarGrupo");
                    }
                    grupoAD.Members.Add(usuario);
                    grupoAD.Save();

                    ViewBag.MensagemSucesso = "Usuário adicionado ao grupo.";
                    return View("AdicionarGrupo");
                }
            }
            catch (Exception ex)
            {
                ViewBag.MensagemErro = "Erro: " + ex.Message;
                return View("AdicionarGrupo");
            }
        }

        // ----------------------------------------
        // REMOVER USUÁRIO DE GRUPO
        // ----------------------------------------

        // GET: /Usuario/RemoverGrupo
        // Este método MOSTRA o formulário de remover grupo
        [HttpGet]
        public IActionResult RemoverGrupo()
        {
            return View(); // Retorna o arquivo Views/Usuario/RemoverGrupo.cshtml
        }

        // POST: /Usuario/RemoverGrupo
        // Este método RECEBE os dados do formulário
        [HttpPost]
        public IActionResult RemoverGrupo([FromForm] UserViewModel model)
        {
            try
            {
                using (var contexto = new PrincipalContext(ContextType.Domain, model.Dominio))
                {
                    var usuario = UserPrincipal.FindByIdentity(contexto, model.Login);
                    var grupoAD = GroupPrincipal.FindByIdentity(contexto, model.Grupo);
                    if (usuario == null || grupoAD == null)
                    {
                        ViewBag.MensagemErro = "Usuário ou grupo não encontrado.";
                        return View("RemoverGrupo");
                    }
                    grupoAD.Members.Remove(usuario);
                    grupoAD.Save();

                    ViewBag.MensagemSucesso = "Usuário removido do grupo.";
                    return View("RemoverGrupo");
                }
            }
            catch (Exception ex)
            {
                ViewBag.MensagemErro = "Erro: " + ex.Message;
                return View("RemoverGrupo");
            }
        }
        
        [HttpGet]
        public IActionResult Index()
        {
        return View();
        }

	}
}