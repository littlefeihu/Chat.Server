using Chat.Data;
using Chat.Service.Authentication;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chat.WebServer
{
    [Authorize]
    public class ChatHub : Hub
    {

        public SimpleUser CurrentUser
        {
            get
            {
                return (Context.User as DQPrincipal).UserData;
            }
        }
        /// <summary>
        /// 给指定的用户发送消息
        /// </summary>
        /// <param name="tousername"></param>
        /// <param name="message"></param>
        public void SendChatMessage(string tousername, string message)
        {
            using (var db = new UserContext())
            {
                var targetUser = db.Users.FirstOrDefault(o => o.UserName == tousername);
                if (targetUser == null)
                {
                    Clients.Caller.showErrorMessage("用户不存在");
                }
                else
                {
                    targetUser.Connections.Where(c => c.Connected == true).ToList();

                    if (targetUser.Connections == null)
                    {
                        Clients.Caller.showErrorMessage("用户不在线");
                    }
                    else
                    {
                        foreach (var connection in targetUser.Connections)
                        {
                            Clients.Client(connection.ConnectionID).reply(CurrentUser.ID, CurrentUser.Name, message);
                        }
                        db.MsgRecords.Add(new MsgRecord
                        {
                            Sended = false,
                            LastUpdatedOn = DateTime.Now,
                            Content = message,
                            FromUserID = CurrentUser.ID,
                            ToUserID = targetUser.Id
                        });
                        db.SaveChanges();
                    }
                }
            }
        }

        /// <summary>
        /// 设置消息的状态为已发送
        /// </summary>
        /// <param name="recordid"></param>
        public void Sended(string recordid)
        {
            using (var db = new UserContext())
            {
                db.Database.ExecuteSqlCommand("update MsgRecords set Sended=1 where id=" + recordid);
            }
        }

        /// <summary>
        /// 给所有客户端发送消息
        /// </summary>
        /// <param name="name"></param>
        /// <param name="message"></param>
        public void Send(string name, string message)
        {
            Clients.All.addMessage(name, message);
        }

        public override Task OnConnected()
        {
            using (var db = new UserContext())
            {
                var user = db.Users.SingleOrDefault(u => u.Id == CurrentUser.ID);

                if (user == null)
                {
                    user = new User
                    {
                        UserName = CurrentUser.Name,
                        Alias = CurrentUser.Alias,
                        Id = CurrentUser.ID,
                        Connections = new List<Connection>()
                    };
                    db.Users.Add(user);
                }

                user.Connections.Add(new Connection
                {
                    ConnectionID = Context.ConnectionId,
                    UserAgent = Context.Request.Headers["User-Agent"],
                    Connected = true,
                    LastUpdatedOn = DateTime.Now
                });
                db.SaveChanges();
            }

            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            SetConnected();
            return base.OnDisconnected(stopCalled);
        }
        public override Task OnReconnected()
        {
            SetConnected();
            return base.OnReconnected();
        }


        private void SetConnected()
        {
            using (var db = new UserContext())
            {
                var connection = db.Connections.FirstOrDefault(o => o.ConnectionID == Context.ConnectionId);
                if (connection != null)
                {
                    connection.Connected = false;
                    connection.LastUpdatedOn = DateTime.Now;

                    db.SaveChanges();
                }

            }
        }

        private void GetRecentContacts()
        {
            using (var db = new UserContext())
            {
                db.MsgRecords.Where(o => o.LastUpdatedOn > DateTime.Now.AddHours(-24));


            }
        }
    }
}
