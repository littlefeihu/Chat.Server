using Chat.Data;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chat.Server
{
    public class ChatHub : Hub
    {
        public void SendChatMessage(string who, string message)
        {
            var name = Context.User.Identity.Name;
            using (var db = new UserContext())
            {
                var user = db.Users.Find(who);
                if (user == null)
                {
                    Clients.Caller.showErrorMessage("用户不存在");
                }
                else
                {
                    user.Connections.Where(c => c.Connected == true).ToList();

                    if (user.Connections == null)
                    {
                        Clients.Caller.showErrorMessage("用户不在线");
                    }
                    else
                    {
                        foreach (var connection in user.Connections)
                        {
                            Clients.Client(connection.ConnectionID).addChatMessage(name + ": " + message);
                        }
                    }
                }
            }
        }


        public void Send(string name, string message)
        {
            if (Context.User != null)
            {
                Console.WriteLine(Context.User.Identity.Name);
            }
            Clients.All.addMessage(name, message);
        }

        public override Task OnConnected()
        {
            if (Context.User != null)
                Console.WriteLine("Client connected: " + Context.ConnectionId);
            //var name = Context.User.Identity.Name;
            //using (var db = new UserContext())
            //{
            //    var user = db.Users.SingleOrDefault(u => u.UserName == name);

            //    if (user == null)
            //    {
            //        user = new User
            //        {
            //            UserName = name,
            //            Connections = new List<Connection>()
            //        };
            //        db.Users.Add(user);
            //    }

            //    user.Connections.Add(new Connection
            //    {
            //        ConnectionID = Context.ConnectionId,
            //        UserAgent = Context.Request.Headers["User-Agent"],
            //        Connected = true
            //    });
            //    db.SaveChanges();
            //}
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            Console.WriteLine("Client disconnected: " + Context.ConnectionId);
            using (var db = new UserContext())
            {
                var connection = db.Connections.Find(Context.ConnectionId);
                connection.Connected = false;
                db.SaveChanges();
            }
            return base.OnDisconnected(stopCalled);
        }

    }
}
