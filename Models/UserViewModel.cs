using System.ComponentModel.DataAnnotations;

namespace GerenciadorAD_Web.Models
{
    /*
      CORREÇÃO: Centralizamos todos os textos das labels aqui.
      O texto "Senha Temporária" e "Nome do Grupo (Opcional)"
      agora vêm daqui, e não do HTML.
    */
    public class UserViewModel
    {
        [Display(Name = "Nome de Usuário (Login)")]
        public string? Login { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Senha Temporária")] // Alterado de "Senha"
        public string? Senha { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Nova Senha")]
        public string? NovoSenha { get; set; }

        [Display(Name = "Nome do Grupo (Opcional)")] // Alterado de "Nome do Grupo"
        public string? Grupo { get; set; }

        // Propriedade para passar o nome do domínio para a View
        public string? Dominio { get; set; }
    }
}