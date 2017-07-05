using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chat.Service.Authentication
{
    public class SimpleRole
    {
        public string Name { get; set; }
        public Guid ID { get; set; }
        public static SimpleRole CreateRole(Guid roleid, string rolename)
        {
            return new SimpleRole { ID = roleid, Name = rolename };
        }
    }
}
