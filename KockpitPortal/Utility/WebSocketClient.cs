using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace KockpitPortal.Utility
{
    public class WebSocketClient
    {
        public string Session { get; set; }
        public WebSocket SocketConnection { get; set; }
        public Dictionary<string, string> ResultQueue { get; set; }
    }
    public class WebSocketClientCollection
    {
        static List<WebSocketClient> clients = new List<WebSocketClient>();

        public static WebSocketClient Add(string session, WebSocket webSocket, Dictionary<string, string> resultQueue)
        {
            if (clients.Exists(s => s.Session == session))
            {
                clients.Remove(new WebSocketClient { Session = session, SocketConnection = webSocket, ResultQueue = resultQueue });
            }
            var webSocketClient = new WebSocketClient { Session = session, SocketConnection = webSocket, ResultQueue = resultQueue };
            clients.Add(webSocketClient);
            return webSocketClient;
        }

        public static WebSocket Get(string session)
        {
            var clientSession = clients.FirstOrDefault(c => c.Session == session);
            if (clientSession == null)
            {
                throw new Exception("Invalid session");
            }
            else if (clientSession.SocketConnection.State == WebSocketState.Closed)
            {
                throw new Exception("Invalid session");
            }

            return clientSession.SocketConnection;
        }

        public static List<WebSocketClient> GetAll()
        {
            return clients.Where(c => c.SocketConnection.State == WebSocketState.Open).ToList();
        }
    }
}
