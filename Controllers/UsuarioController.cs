using System;
using System.DirectoryServices.AccountManagement;
using Microsoft.AspNetCore.Mvc;

// Controller que manipula requisições de usuário da interface web
public class UsuarioController : Controller
{
    // Criação e associação em grupo
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
                if (grupoAD != null)
                {
                    grupoAD.Members.Add(usuario);
                    grupoAD.Save();
                }
            }
            return Ok("Usuário criado e adicionado ao grupo.");
        }
        catch (Exception ex)
        {
            return BadRequest("Erro: " + ex.Message);
        }
    }

    // Reset de senha
    [HttpPost]
    public IActionResult ResetSenha([FromForm] UserViewModel model)
    {
        try
        {
            using (var contexto = new PrincipalContext(ContextType.Domain, model.Dominio))
            {
                var usuario = UserPrincipal.FindByIdentity(contexto, model.Login);
                if (usuario == null)
                    return NotFound("Usuário não encontrado.");
                usuario.SetPassword(model.NovoSenha);
                usuario.Save();

                return Ok("Senha alterada com sucesso.");
            }
        }
        catch (Exception ex)
        {
            return BadRequest("Erro: " + ex.Message);
        }
    }

    // Exclusão do usuário
    [HttpPost]
    public IActionResult Excluir([FromForm] UserViewModel model)
    {
        try
        {
            using (var contexto = new PrincipalContext(ContextType.Domain, model.Dominio))
            {
                var usuario = UserPrincipal.FindByIdentity(contexto, model.Login);
                if (usuario == null)
                    return NotFound("Usuário não encontrado.");
                usuario.Delete();
                return Ok("Usuário excluído com sucesso.");
            }
        }
        catch (Exception ex)
        {
            return BadRequest("Erro: " + ex.Message);
        }
    }

    // Adicionar usuário em outro grupo
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
                    return NotFound("Usuário ou grupo não encontrado.");
                grupoAD.Members.Add(usuario);
                grupoAD.Save();

                return Ok("Usuário adicionado ao grupo.");
            }
        }
        catch (Exception ex)
        {
            return BadRequest("Erro: " + ex.Message);
        }
    }

    // Remover usuário de grupo
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
                    return NotFound("Usuário ou grupo não encontrado.");
                grupoAD.Members.Remove(usuario);
                grupoAD.Save();

                return Ok("Usuário removido do grupo.");
            }
        }
        catch (Exception ex)
        {
            return BadRequest("Erro: " + ex.Message);
        }
    }
}
