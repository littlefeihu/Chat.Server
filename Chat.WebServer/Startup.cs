using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Security.Cookies;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.Identity;

[assembly: OwinStartup(typeof(Chat.WebServer.Startup))]

namespace Chat.WebServer
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCors(CorsOptions.AllowAll);
            app.MapSignalR();
        }
    }
}
