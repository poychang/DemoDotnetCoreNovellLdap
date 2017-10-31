using Novell.Directory.Ldap;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DemoDotnetCoreNovellLdap
{
    public class LdapUtility : IDisposable
    {
        private string Host { get; set; }
        private string BindDN { get; set; }
        private string BindPassword { get; set; }
        private int Port { get; set; }
        private string BaseDC { get; set; }
        private LdapConnection Connection { get; set; }

        public LdapUtility()
        {
            Host = "ldap.forumsys.com";
            BindDN = "cn=read-only-admin,dc=example,dc=com";
            BindPassword = "password";
            BaseDC = "dc=example,dc=com";

            Connection = new LdapConnection();
            Connection.Connect(Host, LdapConnection.DEFAULT_PORT);
            Connection.Bind(BindDN, BindPassword);
        }

        /// <summary>
        /// 搜尋群組
        /// </summary>
        /// <param name="groupName">群組名稱</param>
        /// <returns>成員清單</returns>
        public HashSet<string> SearchForGroup(string groupName)
        {
            var groups = new HashSet<string>();
            try
            {
                var searchFilter = $"(&(objectClass=group)(cn={groupName}))";
                var entities = Connection.Search(BaseDC, LdapConnection.SCOPE_SUB, searchFilter, null, false);
                while (entities.hasMore())
                {
                    var entity = entities.next();
                    groups.Add(entity.DN);
                    var childGroups = GetChildren(string.Empty, entity.DN);
                    foreach (var child in childGroups)
                    {
                        groups.Add(child);
                    }
                }
            }
            catch (LdapException e)
            {
                throw e;
            }
            return groups;
        }

        /// <summary>
        /// 取得子項目
        /// </summary>
        /// <param name="searchBase">搜尋的根目錄</param>
        /// <param name="groupDn">群組識別名稱</param>
        /// <param name="objectClass">物件類別</param>
        /// <returns></returns>
        public HashSet<string> GetChildren(string searchBase, string groupDn, string objectClass = "group")
        {
            var listNames = new HashSet<string>();
            try
            {
                var searchFilter = $"(&(objectClass={objectClass})(memberOf={groupDn}))";
                var entities = Connection.Search(BaseDC, LdapConnection.SCOPE_SUB, searchFilter, null, false);
                while (entities.hasMore())
                {
                    var entity = entities.next();
                    listNames.Add(entity.DN);
                    var children = GetChildren(string.Empty, entity.DN);
                    foreach (var child in children)
                    {
                        listNames.Add(child);
                    }
                }
            }
            catch (LdapException e)
            {
                throw e;
            }
            return listNames;
        }

        /// <summary>
        /// 搜尋使用者
        /// </summary>
        /// <param name="company"></param>
        /// <param name="groups"></param>
        public HashSet<string> SearchForUser(string username, List<string> groups = null)
        {
            var users = new HashSet<string>();
            try
            {
                string groupFilter = (groups?.Count ?? 0) > 0 ?
                    $"(|{string.Join("", groups.Select(x => $"(memberOf={x})").ToList())})" :
                    string.Empty;
                var searchBase = string.Empty;
                string filter = $"(&(objectClass=user)(uid={username}){groupFilter})";
                var entities = Connection.Search(searchBase, LdapConnection.SCOPE_SUB, filter, null, false);

                while (entities.hasMore())
                {
                    var entity = entities.next();
                    entity.getAttributeSet();
                    users.Add(entity.DN);
                }
            }
            catch (LdapException e)
            {
                throw e;
            }
            return users;
        }

        public void Dispose()
        {
            Connection.Disconnect();
            Connection.Dispose();
        }
    }
}
