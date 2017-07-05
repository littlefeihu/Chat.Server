
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;

namespace Chat.Service.Authentication
{
    public class DQPrincipal : IPrincipal
    {
        private IIdentity _identity;
        private SimpleUser _userData;

        public DQPrincipal(FormsAuthenticationTicket ticket, SimpleUser userData)
        {
            if (ticket == null)
                throw new ArgumentNullException("ticket");
            if (userData == null)
                throw new ArgumentNullException("userData");

            _identity = new FormsIdentity(ticket);
            _userData = userData;
        }

        public SimpleUser UserData
        {
            get { return _userData; }
        }

        public IIdentity Identity
        {
            get { return _identity; }
        }

        public bool IsInRole(string role)
        {
            return _userData.Roles.Any(o => o.Name == role);
        }

    }
}
