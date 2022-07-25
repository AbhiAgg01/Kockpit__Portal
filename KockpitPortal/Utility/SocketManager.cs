using KockpitUtility.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KockpitPortal.Utility
{
    public class SocketManager
    {
        public string SOCKETID { get; set; }
        public SocketManager(string socketid)
        {
            SOCKETID = socketid;
        }
        public SocketResponse SocketResponse(eQueryType queryType, string queryToExecute)
        {
            bool lretval = false;
            string strMsg = string.Empty;
            string jsonResponse = string.Empty;
            try
            {
                bool lSocketFound = false;
                foreach (var socket in WebSocketClientCollection.GetAll().Where(c => c.Session == this.SOCKETID))
                {
                    lSocketFound = true;

                    var requestId = Guid.NewGuid().ToString();
                    try
                    {
                        if (socket != null && socket.SocketConnection.State == WebSocketState.Open)
                        {
                            socket.ResultQueue.Add(requestId, string.Empty);

                            var queryInfo = QueryManager.Queries[queryType];

                            var request = JsonConvert.SerializeObject(new SendingQuery
                            {
                                RequestId = requestId,
                                Mode = queryInfo.Mode,
                                Query = queryToExecute
                            });

                            Task t = Send(socket.SocketConnection, request);
                            t.Wait();

                            while (string.IsNullOrEmpty(socket.ResultQueue[requestId]))
                            {
                                Thread.Sleep(2000);
                            }

                            jsonResponse = socket.ResultQueue[requestId];
                            if (jsonResponse.StartsWith("Error:"))
                            {
                                lretval = false;
                                strMsg = jsonResponse;
                            }
                            else
                            {
                                lretval = true;
                            }

                            socket.ResultQueue.Remove(requestId);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (socket.ResultQueue.ContainsKey(requestId))
                            socket.ResultQueue.Remove(requestId);
                        strMsg = "Error: " + ex.Message;
                    }
                }

                if (!lSocketFound)
                {
                    strMsg = "Error while establishing a connection with gateway";
                }
            }
            catch (Exception ex)
            {
                strMsg = "Error: " + ex.Message;
            }

            return new SocketResponse { success = lretval, msg = strMsg, jsonresponse = jsonResponse };
        }

        private async Task Send(WebSocket webSocket, string message)
        {
            //code to encrypt the message
            message = Secure.Encryptdata(message);
            if (webSocket != null && webSocket.State == WebSocketState.Open)
            {
                var value = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message));
                await webSocket.SendAsync(value, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }
}
