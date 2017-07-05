using Chat.Service.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Chat.WebClient
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (HttpContext.Current.User != null)
            {
                Label1.Text = HttpContext.Current.User.Identity.Name;
            }
        }

        protected void loginbtn_Click(object sender, EventArgs e)
        {
            FormsAuthenticationService.Signin(new SimpleUser { Alias = nickName.Text, Name = nickName.Text, ID = Guid.NewGuid() });


        }
    }
}