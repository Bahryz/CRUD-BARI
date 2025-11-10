using System;
using System.DirectoryServices.AccountManagement; // Importa namespace do AD local

namespace GerenciadorUsuariosAD
{
    public class Program
    {
        // Método principal da aplicação
        public static void Main(string[] args)
        {
            try
            {
                GerenciarUsuarioAD();
                Console.WriteLine("Operações concluídas. Pressione qualquer tecla para sair.");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro: " + ex.Message);
                Console.ReadLine();
            }
        }

        // Função para gerenciar usuários e grupos do Active Directory
        public static void GerenciarUsuarioAD()
        {
            // Conecta ao domínio do Active Directory local
            using (var contexto = new PrincipalContext(ContextType.Domain, "NOME_DO_DOMINIO"))
            {
                // --- Criação de usuário ---
                var usuario = new UserPrincipal(contexto);              // Cria novo usuário do AD
                usuario.SamAccountName = "novousuario";                 // Define login de rede do usuário
                usuario.SetPassword("SenhaForte#2025");                 // Seta a senha inicial
                usuario.Save();                                         // Salva o usuário no AD

                // --- Reset de senha ---
                usuario.SetPassword("NovaSenha#2025");                  // Altera a senha do usuário
                usuario.Save();                                         // Salva a alteração

                // --- Adição em grupo ---
                var grupo = GroupPrincipal.FindByIdentity(contexto, "NOME_DO_GRUPO"); // Procura o grupo no AD
                if (grupo != null) grupo.Members.Add(usuario);          // Se achar, adiciona usuário no grupo
                grupo.Save();                                           // Salva a alteração

                // --- Remoção do usuário do grupo ---
                grupo.Members.Remove(usuario);                          // Remove usuário do grupo
                grupo.Save();                                           // Salva no AD
            }
        }
    }
}
