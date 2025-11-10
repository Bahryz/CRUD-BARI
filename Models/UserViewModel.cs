using System.ComponentModel.DataAnnotations;

namespace GerenciadorAD_Web.Models
{
    /*
      CORREÇÃO: Este ViewModel agora usa OS MESMOS NOMES
      que o seu UsuarioController.cs (Login, Senha, etc.).
      Removemos os [Required] daqui porque o seu Controller
      já faz essa validação manualmente.
    */
    public class UserViewModel
    {
        [Display(Name = "Nome de Usuário (Login)")]
        public string? Login { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Senha")]
        public string? Senha { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Nova Senha")]
        public string? NovoSenha { get; set; }

        [Display(Name = "Nome do Grupo")]
        public string? Grupo { get; set; }

        // Propriedade para passar o nome do domínio para a View
        public string? Dominio { get; set; }
    }
}