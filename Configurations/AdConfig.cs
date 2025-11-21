namespace GerenciadorAD_Web.Configurations
{
    public class AdConfig
    {
        public string Dominio { get; set; } = string.Empty;
        public string UsuarioAdmin { get; set; } = string.Empty;
        public string SenhaAdmin { get; set; } = string.Empty;
        
        public string GrupoPermitido { get; set; } = "Analistas";
        
        public string MasterUser { get; set; } = string.Empty;
        public string MasterPass { get; set; } = string.Empty;
    }
}