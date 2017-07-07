using Chat.Data;
using Chat.Service.Authentication;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
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

        private static object connectedLock = new object();
        public SimpleUser CurrentUser
        {
            get
            {
                return (Context.User as DQPrincipal).UserData;
            }
        }

        /// <summary>
        /// 自动消息处理
        /// </summary>
        /// <param name="tousername"></param>
        /// <param name="message"></param>
        public void replyMessage(string tousername, string touserid, string message)
        {

            using (var db = new UserContext())
            {
                var targetUser = db.Users.FirstOrDefault(o => o.UserName == CurrentUser.Name);
                if (targetUser == null)
                {
                    Clients.Caller.showErrorMessage("用户不存在");
                }
                else
                {
                    var onlineClients = targetUser.Connections.Where(c => c.Connected == true).ToList();
                    var isOnline = true;
                    if (onlineClients == null || onlineClients.Count == 0)
                    {
                        Clients.Caller.notOnline("对方不在线");
                        isOnline = false;
                    }
                    var dt = string.Format("{0:t}", DateTime.Now);
                    foreach (var connection in onlineClients)
                    {
                        Clients.Client(connection.ConnectionID).reply(touserid, tousername, message, dt, "new");
                    }
                    db.MsgRecords.Add(new MsgRecord
                    {
                        Sended = isOnline,
                        LastUpdatedOn = DateTime.Now,
                        Content = message,
                        FromUserID = touserid,
                        ToUserID = CurrentUser.Name
                    });
                    db.SaveChanges();
                }
            }
        }


        /// <summary>
        /// 给指定的用户发送消息
        /// </summary>
        /// <param name="tousername"></param>
        /// <param name="message"></param>
        public void sendMessage(string tousername, string touserid, string message)
        {
            using (var db = new UserContext())
            {
                var targetUser = db.Users.FirstOrDefault(o => o.UserName == touserid);
                if (targetUser == null)
                {
                    Clients.Caller.showErrorMessage("用户不存在");
                }
                else
                {
                    var onlineClients = targetUser.Connections.Where(c => c.Connected == true).ToList();
                    var isOnline = true;
                    if (onlineClients == null || onlineClients.Count == 0)
                    {
                        Clients.Caller.notOnline("对方不在线");
                        isOnline = false;
                    }

                    if (string.IsNullOrEmpty(message))
                    {
                        ///如果消息为空，则发送自动消息
                        replyMessage(tousername, touserid, "您好");
                    }
                    else
                    {
                        var dt = string.Format("{0:t}", DateTime.Now);
                        foreach (var connection in onlineClients)
                        {
                            Clients.Client(connection.ConnectionID).reply(CurrentUser.Name, CurrentUser.Name, message, dt, "new");
                        }
                        db.MsgRecords.Add(new MsgRecord
                        {
                            Sended = isOnline,
                            LastUpdatedOn = DateTime.Now,
                            Content = message,
                            FromUserID = CurrentUser.Name,
                            ToUserID = targetUser.UserName
                        });
                        db.SaveChanges();
                    }
                }
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
                            Clients.Client(connection.ConnectionID).reply(CurrentUser.ID, CurrentUser.Name, message, "new");
                        }
                        db.MsgRecords.Add(new MsgRecord
                        {
                            Sended = false,
                            LastUpdatedOn = DateTime.Now,
                            Content = message,
                            FromUserID = CurrentUser.Name,
                            ToUserID = targetUser.UserName
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

            SetConnected();

            var records = GetRecentContacts();

            foreach (var record in records)
            {
                var dt = string.Format("{0:t}", record.LastUpdatedOn);
                Clients.Caller.reply(record.FromUserID, record.FromUserID, record.Content, dt, "history");
            }

            return base.OnConnected();
        }


        private void SetConnected()
        {
            lock (connectedLock)
            {
                using (var db = new UserContext())
                {
                    var user = db.Users.SingleOrDefault(u => u.UserName == CurrentUser.Name);

                    if (user == null)
                    {
                        user = new User
                        {
                            UserName = CurrentUser.Name,
                            Alias = CurrentUser.Alias,
                            Id = CurrentUser.ID == Guid.Empty ? Guid.NewGuid() : CurrentUser.ID,
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
            }
        }
        public override Task OnDisconnected(bool stopCalled)
        {
            SetDisConnected();
            return base.OnDisconnected(stopCalled);
        }
        public override Task OnReconnected()
        {
            SetDisConnected();
            return base.OnReconnected();
        }


        private void SetDisConnected()
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

        private List<MsgRecord> GetRecentContacts()
        {
            using (var db = new UserContext())
            {

                var records = db.Database.SqlQuery<MsgRecord>(@"select * from (
SELECT  [Id]
      ,[FromUserID]
      ,[ToUserID]
      ,[Content]
      ,[Sended]
      ,[LastUpdatedOn],ROW_NUMBER() over(partition by fromuserid order by LastUpdatedOn desc) as rows 
  FROM [dbo].[MsgRecord] where  ToUserID=" + CurrentUser.Name + ") as a where a.rows=1").ToList();

                return records;

            }
        }
    }
}
