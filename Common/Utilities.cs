using System.DirectoryServices.AccountManagement; // Importa namespace para manipulação do AD local

internal class Program
{
    // Método principal da aplicação
    private static void Main(string[] args)
    {
        // Chama o método para gerenciar usuário do AD
        GerenciarUsuarioAD();
    }

    // Método para gerenciar usuários no Active Directory
    public static void GerenciarUsuarioAD()
    {
        // Conecta ao domínio do Active Directory local
        using (var contexto = new PrincipalContext(ContextType.Domain, "NOME_DO_DOMINIO"))
        {
            // --- Criação do usuário ---
            var usuario = new UserPrincipal(contexto);              // Cria novo usuário do AD
            usuario.SamAccountName = "novousuario";                 // Define o login (nome de usuário de rede)
            usuario.SetPassword("SenhaForte#2025");                 // Define a senha inicial do usuário
            usuario.Save();                                         // Salva o usuário no AD

            // --- Reset de senha ---
            usuario.SetPassword("NovaSenha#2025");                  // Define uma nova senha para o usuário
            usuario.Save();                                         // Salva a alteração da senha no AD

            // --- Adição em grupo ---
            var grupo = GroupPrincipal.FindByIdentity(contexto, "NOME_DO_GRUPO"); // Procura o grupo pelo nome
            if (grupo != null) grupo.Members.Add(usuario);          // Se grupo existir, adiciona usuário nele
            grupo.Save();                                           // Salva a alteração de membros no grupo

            // --- Remoção do usuário do grupo ---
            grupo.Members.Remove(usuario);                          // Remove o usuário do grupo
            grupo.Save();                                           // Salva a alteração no AD
        }
    }
}
