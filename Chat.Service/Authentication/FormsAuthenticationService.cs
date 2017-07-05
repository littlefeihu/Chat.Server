
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;

namespace Chat.Service.Authentication
{
    public class FormsAuthenticationService
    {
        public static void Signin(SimpleUser user)
        {
            var now = DateTime.Now;
            var ticket = new FormsAuthenticationTicket(
            1,
            user.Name,
            now,
            now.Add(new TimeSpan(0, 30, 0)),
            false,
            Newtonsoft.Json.JsonConvert.SerializeObject(user),
            FormsAuthentication.FormsCookiePath);

            var encryptedTicket = FormsAuthentication.Encrypt(ticket);

            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
            cookie.HttpOnly = true;
            if (ticket.IsPersistent)
            {
                cookie.Expires = ticket.Expiration;
            }

            cookie.Secure = FormsAuthentication.RequireSSL;
            cookie.Path = FormsAuthentication.FormsCookiePath;
            if (FormsAuthentication.CookieDomain != null)
            {
                cookie.Domain = FormsAuthentication.CookieDomain;
            }
            HttpContext.Current.Response.Cookies.Add(cookie);
        }
        public static void SignOut()
        {
            FormsAuthentication.SignOut();
        }

        public static SimpleUser GetAuthenticatedCustomer()
        {

            if (HttpContext.Current == null ||
                     HttpContext.Current.Request == null ||
                !HttpContext.Current.Request.IsAuthenticated ||
                !(HttpContext.Current.User.Identity is FormsIdentity))
            {
                return null;
            }

            var formsIdentity = (FormsIdentity)HttpContext.Current.User.Identity;
            var customer = GetAuthenticatedCustomerFromTicket(formsIdentity.Ticket);

            return customer;
        }

        protected static SimpleUser GetAuthenticatedCustomerFromTicket(FormsAuthenticationTicket ticket)
        {
            if (ticket == null)
                throw new ArgumentNullException("ticket");

            return Newtonsoft.Json.JsonConvert.DeserializeObject<SimpleUser>(ticket.UserData);
        }

        public static void SetCurrentUser()
        {
            if (HttpContext.Current == null ||
                 HttpContext.Current.Request == null ||
            !HttpContext.Current.Request.IsAuthenticated ||
            !(HttpContext.Current.User.Identity is FormsIdentity))
            {
                return;
            }
            var formsIdentity = (FormsIdentity)HttpContext.Current.User.Identity;
            HttpContext.Current.User = new DQPrincipal(formsIdentity.Ticket, GetAuthenticatedCustomerFromTicket(formsIdentity.Ticket));
        }
    }
}
