namespace GerenciadorAD_Web.Configurations
{
    public class AdConfig
    {
        public string Dominio { get; set; }
        public string UsuarioAdmin { get; set; }
        public string SenhaAdmin { get; set; }
        
        // Configurações de Segurança
        public string GrupoPermitido { get; set; } = "Analistas"; // Padrão
        public string MasterUser { get; set; }
        public string MasterPass { get; set; }
    }
}