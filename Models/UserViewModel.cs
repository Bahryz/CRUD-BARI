namespace GerenciadorAD_Web.Models
{
    // Model para entrada de dados nos formulários
    public class UserViewModel
    {
        // Corrigido: Inicializado para string.Empty para evitar aviso
        public string Dominio { get; set; } = string.Empty;

        // Corrigido: Adicionado '?' para indicar que podem ser nulos
        public string? Login { get; set; }
        public string? Senha { get; set; }
        public string? NovoSenha { get; set; }
        public string? Grupo { get; set; }
    }
}