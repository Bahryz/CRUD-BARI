using GerenciadorAD_Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration; // Para ler o appsettings.json
using Novell.Directory.Ldap; // A nova biblioteca LDAP
using System;
using System.Collections.Generic;
using System.Text;

namespace GerenciadorAD_Web.Controllers
{
    public class UsuarioController : Controller
    {
        // Variáveis para guardar as configurações do AD
        private readonly string _ldapHost;
        private readonly int _ldapPort;
        private readonly string _adminDN;      // DN do utilizador admin (ex: "CN=Admin,DC=meu,DC=dominio")
        private readonly string _adminPass;    // Senha do utilizador admin
        private readonly string _searchBase;   // Onde criar/procurar utilizadores (ex: "OU=Utilizadores,DC=meu,DC=dominio")
        private readonly string _domainDNS;    // Nome DNS do domínio (ex: "meu.dominio.com")

        // Injeção de Dependência: O Controller "lê" o appsettings.json
        public UsuarioController(IConfiguration config)
        {
            // CORREÇÃO: Adicionamos '!' para corrigir os avisos CS8618 e CS8601
            // Isto diz ao compilador para confiar que estes valores existem no appsettings.json
            _ldapHost = config.GetValue<string>("LdapConfig:Host")!;
            _ldapPort = config.GetValue<int>("LdapConfig:Port");
            _adminDN = config.GetValue<string>("LdapConfig:BindDN")!;
            _adminPass = config.GetValue<string>("LdapConfig:BindPassword")!;
            _searchBase = config.GetValue<string>("LdapConfig:SearchBase")!;
            
            // A correção no _adminDN acima também corrige o aviso CS8602 nesta linha
            _domainDNS = _adminDN.Substring(_adminDN.IndexOf("DC=")).Replace("DC=", "").Replace(",", ".");
        }

        /// <summary>
        /// Cria e autentica uma nova ligação LDAP com as credenciais de admin.
        /// </summary>
        private LdapConnection GetLdapConnection()
        {
            var cn = new LdapConnection();
            cn.Connect(_ldapHost, _ldapPort);
            cn.Bind(LdapVersion.LdapV3, _adminDN, _adminPass);
            return cn;
        }

        /// <summary>
        /// Encontra o DN (Distinguished Name) de um utilizador ou grupo pelo seu login (sAMAccountName).
        /// </summary>
        // CORREÇÃO: Alterado o tipo de retorno para 'string?' para corrigir o aviso CS8603
        private string? FindDnBySamAccountName(LdapConnection cn, string samAccountName)
        {
            try
            {
                var search = cn.Search(
                    _searchBase,
                    LdapConnection.ScopeSub,
                    $"(sAMAccountName={samAccountName})",
                    new[] { "distinguishedName" }, // Pedimos apenas o DN
                    false
                );

                if (search.HasMore())
                {
                    var entry = search.Next();
                    return entry?.Dn; // Retorna o DN (ex: "CN=pedro,OU=Usuarios...")
                }
                else
                {
                    return null; // Nenhum resultado encontrado
                }
            }
            catch (LdapException)
            {
                // Apanha erros de LDAP (ex: searchBase inválido)
                return null;
            }
        }
        
        // ----------------------------------------
        // INDEX (Página Principal)
        // ----------------------------------------
        [HttpGet]
        public IActionResult Index()
        {
            return View(); 
        }


        // ----------------------------------------
        // CRIAR USUÁRIO
        // ----------------------------------------

        [HttpGet]
        public IActionResult Criar()
        {
            return View(new UserViewModel { Dominio = _domainDNS }); 
        }

        [HttpPost]
        public IActionResult Criar([FromForm] UserViewModel model)
        {
            model.Dominio = _domainDNS;
            string novoUserDN = $"CN={model.Login},{_searchBase}";

            try
            {
                using (var cn = GetLdapConnection())
                {
                    byte[] passwordBytes = Encoding.Unicode.GetBytes($"\"{model.Senha}\"");

                    var atributos = new LdapAttributeSet
                    {
                        new LdapAttribute("objectClass", new[] { "top", "person", "organizationalPerson", "user" }),
                        new LdapAttribute("cn", model.Login),
                        new LdapAttribute("sAMAccountName", model.Login),
                        new LdapAttribute("userPrincipalName", $"{model.Login}@{_domainDNS}"),
                        new LdapAttribute("unicodePwd", passwordBytes),
                        new LdapAttribute("userAccountControl", "512") 
                    };

                    var novaEntrada = new LdapEntry(novoUserDN, atributos);
                    cn.Add(novaEntrada);

                    if (!string.IsNullOrEmpty(model.Grupo))
                    {
                        string? grupoDN = FindDnBySamAccountName(cn, model.Grupo); // <-- string?
                        if (grupoDN != null)
                        {
                            var mods = new LdapModification[]
                            {
                                new LdapModification(LdapModification.Add, new LdapAttribute("member", novoUserDN))
                            };
                            cn.Modify(grupoDN, mods);
                        }
                        else
                        {
                            ViewBag.MensagemErro = $"AVISO: Utilizador '{model.Login}' criado, mas o grupo '{model.Grupo}' não foi encontrado.";
                            return View("Criar", model);
                        }
                    }
                }

                ViewBag.MensagemSucesso = "Utilizador criado com sucesso!";
                return View("Criar", new UserViewModel { Dominio = _domainDNS });
            }
            catch (LdapException ldapEx)
            {
                if (ldapEx.ResultCode == LdapException.ENTRY_ALREADY_EXISTS)
                {
                    ViewBag.MensagemErro = $"Erro: O utilizador com o login '{model.Login}' (DN: {novoUserDN}) já existe.";
                }
                else
                {
                    ViewBag.MensagemErro = $"Erro de LDAP: ({ldapEx.ResultCode}) {ldapEx.Message}";
                }
                return View("Criar", model);
            }
            catch (Exception ex)
            {
                ViewBag.MensagemErro = $"Erro geral: {ex.Message}";
                return View("Criar", model);
            }
        }

        // ----------------------------------------
        // RESETAR SENHA
        // ----------------------------------------

        [HttpGet]
        public IActionResult ResetSenha()
        {
            return View(new UserViewModel { Dominio = _domainDNS });
        }

        [HttpPost]
        public IActionResult ResetSenha([FromForm] UserViewModel model)
        {
            model.Dominio = _domainDNS;
            try
            {
                using (var cn = GetLdapConnection())
                {
                    string? userDN = FindDnBySamAccountName(cn, model.Login); // <-- string?
                    if (userDN == null)
                    {
                        ViewBag.MensagemErro = "Utilizador não encontrado.";
                        return View("ResetSenha", model);
                    }

                    byte[] passwordBytes = Encoding.Unicode.GetBytes($"\"{model.NovoSenha}\"");
                    var mod = new LdapModification(LdapModification.Replace, new LdapAttribute("unicodePwd", passwordBytes));
                    
                    cn.Modify(userDN, mod);
                }

                ViewBag.MensagemSucesso = "Senha alterada com sucesso.";
                return View("ResetSenha", new UserViewModel { Dominio = _domainDNS });
            }
            catch (LdapException ldapEx)
            {
                ViewBag.MensagemErro = $"Erro de LDAP: ({ldapEx.ResultCode}) {ldapEx.Message}";
                return View("ResetSenha", model);
            }
            catch (Exception ex)
            {
                ViewBag.MensagemErro = "Erro: " + ex.Message;
                return View("ResetSenha", model);
            }
        }

        // ----------------------------------------
        // EXCLUIR USUÁRIO
        // ----------------------------------------

        [HttpGet]
        public IActionResult Excluir()
        {
            return View(new UserViewModel { Dominio = _domainDNS });
        }

        [HttpPost]
        public IActionResult Excluir([FromForm] UserViewModel model)
        {
            model.Dominio = _domainDNS;
            try
            {
                using (var cn = GetLdapConnection())
                {
                    string? userDN = FindDnBySamAccountName(cn, model.Login); // <-- string?
                    if (userDN == null)
                    {
                        ViewBag.MensagemErro = "Utilizador não encontrado.";
                        return View("Excluir", model);
                    }
                    
                    cn.Delete(userDN);

                    ViewBag.MensagemSucesso = "Utilizador excluído com sucesso.";
                    return View("Excluir", new UserViewModel { Dominio = _domainDNS });
                }
            }
            catch (LdapException ldapEx)
            {
                ViewBag.MensagemErro = $"Erro de LDAP: ({ldapEx.ResultCode}) {ldapEx.Message}";
                return View("Excluir", model);
            }
            catch (Exception ex)
            {
                ViewBag.MensagemErro = "Erro: " + ex.Message;
                return View("Excluir", model);
            }
        }

        // ----------------------------------------
        // ADICIONAR USUÁRIO A GRUPO
        // ----------------------------------------

        [HttpGet]
        public IActionResult AdicionarGrupo()
        {
            return View(new UserViewModel { Dominio = _domainDNS });
        }

        [HttpPost]
        public IActionResult AdicionarGrupo([FromForm] UserViewModel model)
        {
            model.Dominio = _domainDNS;
            try
            {
                using (var cn = GetLdapConnection())
                {
                    string? userDN = FindDnBySamAccountName(cn, model.Login); // <-- string?
                    string? grupoDN = FindDnBySamAccountName(cn, model.Grupo); // <-- string?

                    if (userDN == null || grupoDN == null)
                    {
                        ViewBag.MensagemErro = "Utilizador ou grupo não encontrado.";
                        return View("AdicionarGrupo", model);
                    }

                    var mod = new LdapModification(LdapModification.Add, new LdapAttribute("member", userDN));
                    cn.Modify(grupoDN, mod);

                    ViewBag.MensagemSucesso = "Utilizador adicionado ao grupo.";
                    return View("AdicionarGrupo", new UserViewModel { Dominio = _domainDNS });
                }
            }
            catch (LdapException ldapEx)
            {
                if (ldapEx.ResultCode == LdapException.ATTRIBUTE_OR_VALUE_EXISTS)
                {
                    ViewBag.MensagemErro = "O utilizador já é membro desse grupo.";
                }
                else
                {
                    ViewBag.MensagemErro = $"Erro de LDAP: ({ldapEx.ResultCode}) {ldapEx.Message}";
                }
                return View("AdicionarGrupo", model);
            }
            catch (Exception ex)
            {
                ViewBag.MensagemErro = "Erro: " + ex.Message;
                return View("AdicionarGrupo", model);
            }
        }

        // ----------------------------------------
        // REMOVER USUÁRIO DE GRUPO
        // ----------------------------------------

        [HttpGet]
        public IActionResult RemoverGrupo()
        {
            return View(new UserViewModel { Dominio = _domainDNS });
        }

        [HttpPost]
        public IActionResult RemoverGrupo([FromForm] UserViewModel model)
        {
            model.Dominio = _domainDNS;
            try
            {
                using (var cn = GetLdapConnection())
                {
                    string? userDN = FindDnBySamAccountName(cn, model.Login); // <-- string?
                    string? grupoDN = FindDnBySamAccountName(cn, model.Grupo); // <-- string?

                    if (userDN == null || grupoDN == null)
                    {
                        ViewBag.MensagemErro = "Utilizador ou grupo não encontrado.";
                        return View("RemoverGrupo", model);
                    }
                    
                    var mod = new LdapModification(LdapModification.Delete, new LdapAttribute("member", userDN));
                    cn.Modify(grupoDN, mod);

                    ViewBag.MensagemSucesso = "Utilizador removido do grupo.";
                    return View("RemoverGrupo", new UserViewModel { Dominio = _domainDNS });
                }
            }
            catch (LdapException ldapEx)
            {
                if (ldapEx.ResultCode == LdapException.NO_SUCH_ATTRIBUTE)
                {
                    ViewBag.MensagemErro = "O utilizador não era membro desse grupo.";
                }
                else
                {
                    ViewBag.MensagemErro = $"Erro de LDAP: ({ldapEx.ResultCode}) {ldapEx.Message}";
                }
                return View("RemoverGrupo", model);
            }
            catch (Exception ex)
            {
                ViewBag.MensagemErro = "Erro: " + ex.Message;
                return View("RemoverGrupo", model);
            }
        }
    }
}