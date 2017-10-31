using Novell.Directory.Ldap;
using System;

namespace DemoDotnetCoreNovellLdap
{
    class Program
    {
        static void Main(string[] args)
        {
            var User = new { username = "tesla", password = "password" };

            var simpleAuthResult = SimpleLdapAuth(User.username, User.password);
            Console.WriteLine($"LDAP Auth Result: {simpleAuthResult}");
            var selfAuthResult = SelfLdapAuth(User.username, User.password);
            Console.WriteLine($"LDAP Auth Result: {selfAuthResult}");
        }

        /// <summary>
        /// 簡單驗證 LDAP 帳戶
        /// </summary>
        /// <remarks>需要管理者帳號的寫法</remarks>
        /// <param name="username">帳號</param>
        /// <param name="password">密碼</param>
        /// <returns>驗證是否成功</returns>
        public static bool SimpleLdapAuth(string username, string password)
        {
            var Host = "ldap.forumsys.com";
            var BindDN = "cn=read-only-admin,dc=example,dc=com";
            var BindPassword = "password";
            var BaseDC = "dc=example,dc=com";

            try
            {
                using (var connection = new LdapConnection())
                {
                    connection.Connect(Host, LdapConnection.DEFAULT_PORT);
                    connection.Bind(BindDN, BindPassword);

                    var searchFilter = $"(uid={username})";
                    var entities = connection.Search(BaseDC, LdapConnection.SCOPE_SUB, searchFilter, null, false);

                    string userDn = null;
                    while (entities.hasMore())
                    {
                        var entity = entities.next();
                        var account = entity.getAttribute("uid");
                        if (account != null && account.StringValue == username)
                        {
                            userDn = entity.DN;
                            break;
                        }
                    }
                    if (string.IsNullOrWhiteSpace(userDn)) return false;

                    connection.Bind(userDn, password);
                    return connection.Bound;
                }
            }
            catch (LdapException e)
            {
                throw e;
            }
        }

        /// <summary>
        /// 簡單驗證 LDAP 帳戶
        /// </summary>
        /// <remarks>不需要管理者帳號的寫法</remarks>
        /// <param name="username">帳號</param>
        /// <param name="password">密碼</param>
        /// <returns>驗證是否成功</returns>
        public static bool SelfLdapAuth(string username, string password)
        {
            var Host = "ldap.forumsys.com";
            var BaseDC = "dc=example,dc=com";

            try
            {
                using (var connection = new LdapConnection())
                {
                    connection.Connect(Host, LdapConnection.DEFAULT_PORT);
                    connection.Bind($"uid={username},{BaseDC}", password);
                    return connection.Bound;
                }
            }
            catch (LdapException e)
            {
                throw e;
            }
        }
    }
}
