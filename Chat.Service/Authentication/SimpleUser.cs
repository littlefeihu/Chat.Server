using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chat.Service.Authentication
{
    public class SimpleUser
    {
        public string Name { get; set; }

        public string Alias { get; set; }

        public Guid ID { get; set; }
        public Guid? SupplierId { get; set; }
        public List<SimpleRole> Roles { get; set; }

        public static SimpleUser CreateUser(Guid userid, string username, string alias, List<SimpleRole> roles)
        {
            return new SimpleUser { ID = userid, Name = username, Roles = roles, Alias = alias };
        }
    }
}
