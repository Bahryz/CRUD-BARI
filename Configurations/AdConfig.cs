namespace GerenciadorAD_Web.Configurations
{
    public class AdConfig
    {
        public string Dominio { get; set; }
        public string UsuarioAdmin { get; set; }
        public string SenhaAdmin { get; set; } // Será preenchido via Env Var
        public string ContainerOuOu { get; set; } // Opcional: Unidade Organizacional base
    }
}