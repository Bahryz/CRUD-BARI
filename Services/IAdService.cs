using GerenciadorAD_Web.Models;

namespace GerenciadorAD_Web.Services
{
    public interface IAdService
    {
    
        bool ValidarLoginNoAd(string usuario, string senha);
        bool UsuarioPertenceAoGrupo(string usuario, string grupo); 
        void CriarUsuario(UserViewModel model);
        void ResetarSenha(string login, string novaSenha);
        void ExcluirUsuario(string login);
        void AdicionarAoGrupo(string login, string grupo);
        void RemoverDoGrupo(string login, string grupo);
    }
}