using System.DirectoryServices.AccountManagement;
using System.Runtime.Versioning;  
using Microsoft.Extensions.Options;
using GerenciadorAD_Web.Configurations;
using GerenciadorAD_Web.Models;

namespace GerenciadorAD_Web.Services
{
    [SupportedOSPlatform("windows")]
    public class AdService : IAdService
    {
        private readonly AdConfig _config;

        public AdService(IOptions<AdConfig> config)
        {
            _config = config.Value;
        }

        private PrincipalContext GetContext()
        {
            return new PrincipalContext(ContextType.Domain, _config.Dominio, _config.UsuarioAdmin, _config.SenhaAdmin);
        }

        // --- NOVOS MÉTODOS DE SEGURANÇA ---

        public bool ValidarLoginNoAd(string usuario, string senha)
        {
            try
            {
                using var context = GetContext();
                return context.ValidateCredentials(usuario, senha);
            }
            catch
            {
                return false; 
            }
        }

        public bool UsuarioPertenceAoGrupo(string login, string grupoNome)
        {
            try
            {
                using var context = GetContext();
                using var usuario = UserPrincipal.FindByIdentity(context, login);
                using var grupo = GroupPrincipal.FindByIdentity(context, grupoNome);

                if (usuario == null || grupo == null) return false;

                return usuario.IsMemberOf(grupo);
            }
            catch
            {
                return false;
            }
        }


        public void CriarUsuario(UserViewModel model)
        {
             using var context = GetContext();
             if (UserPrincipal.FindByIdentity(context, model.Login) != null)
                throw new Exception($"Usuário '{model.Login}' já existe.");

            using var usuario = new UserPrincipal(context)
            {
                SamAccountName = model.Login,
                UserPrincipalName = $"{model.Login}@{_config.Dominio}",
                Enabled = true
            };
            usuario.SetPassword(model.Senha);
            usuario.Save();
            
            if (!string.IsNullOrEmpty(model.Grupo)) AdicionarAoGrupo(model.Login, model.Grupo);
        }

        public void ResetarSenha(string login, string novaSenha)
        {
            using var context = GetContext();
            var usuario = UserPrincipal.FindByIdentity(context, login) ?? throw new Exception("Usuário não encontrado.");
            usuario.SetPassword(novaSenha);
            usuario.Save();
        }

        public void ExcluirUsuario(string login)
        {
            using var context = GetContext();
            var usuario = UserPrincipal.FindByIdentity(context, login) ?? throw new Exception("Usuário não encontrado.");
            usuario.Delete();
        }

        public void AdicionarAoGrupo(string login, string nomeGrupo)
        {
            using var context = GetContext();
            var usuario = UserPrincipal.FindByIdentity(context, login);
            var grupo = GroupPrincipal.FindByIdentity(context, nomeGrupo);
            if (usuario == null || grupo == null) throw new Exception("Usuário ou Grupo não encontrado.");
            if (!grupo.Members.Contains(usuario)) { grupo.Members.Add(usuario); grupo.Save(); }
        }

        public void RemoverDoGrupo(string login, string nomeGrupo)
        {
            using var context = GetContext();
            var usuario = UserPrincipal.FindByIdentity(context, login);
            var grupo = GroupPrincipal.FindByIdentity(context, nomeGrupo);
            if (usuario == null || grupo == null) throw new Exception("Usuário ou Grupo não encontrado.");
            if (grupo.Members.Contains(usuario)) { grupo.Members.Remove(usuario); grupo.Save(); }
        }
    }
}