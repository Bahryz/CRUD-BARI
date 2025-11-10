namespace GerenciadorAD_Web.Models // <-- Adicionámos esta linha e as { }
{
    // Model para entrada de dados nos formulários
    public class UserViewModel
    {
        public string Dominio { get; set; }
        public string Login { get; set; }
        public string Senha { get; set; }
        public string NovoSenha { get; set; }
        public string Grupo { get; set; }
    }
}