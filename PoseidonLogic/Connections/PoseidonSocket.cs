using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PoseidonLogic.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PoseidonLogic
{
    public class PoseidonSocket
    {
        public bool Running { get; set; }
        private static CancellationTokenSource tokenSource = new CancellationTokenSource();
        private ClientWebSocket webSocket { get; set; }

        private PoseidonManager _manager { get; set; }

        private readonly ILogger _logger;

        public PoseidonSocket(PoseidonManager manager)
        {
            this._manager = manager;
            this._logger = ApplicationLogging.CreateLogger<PoseidonSocket>();
        }

        public void Connect()
        {
            this.Running = true;

            Thread thread = new Thread(() => { this.Run(); });
            thread.Priority = ThreadPriority.Highest;
            thread.Start();
        }

        public async void Run()
        {
            try
            {
                this._logger.LogInformation("Opening Socket");

                this.webSocket = new ClientWebSocket();
                
                try
                {
                    webSocket.Options.KeepAliveInterval = TimeSpan.FromSeconds(5);

                    await webSocket.ConnectAsync(new Uri(this._manager.POSEIDON_SOCKET), CancellationToken.None);

                    this.SendListenRequest();

                    while (this.Running)
                    {
                        if (webSocket.State == WebSocketState.Open)
                        {
                            // Read the bytes from the web socket and accumulate all into a list.
                            var buffer = new ArraySegment<byte>(new byte[4 * 1024]);
                            WebSocketReceiveResult result = null;
                            var allBytes = new List<byte>();

                            do
                            {
                                result = await webSocket.ReceiveAsync(buffer, tokenSource.Token);
                                for (int i = 0; i < result.Count; i++)
                                {
                                    allBytes.Add(buffer.Array[i]);
                                }
                            } while (!result.EndOfMessage && result.CloseStatus == null && result.MessageType != WebSocketMessageType.Close);

                            if (result.CloseStatus != null || result.MessageType == WebSocketMessageType.Close)
                            {
                                this._logger.LogError($"Error {result.CloseStatusDescription}");
                                this.RestartSocket();
                            }
                            else
                            {
                                if (allBytes.Count > 0)
                                {
                                    string data = Encoding.UTF8.GetString(allBytes.ToArray(), 0, allBytes.Count);
                                    Models.PoseidonNotification notification = JsonConvert.DeserializeObject<PoseidonNotification>(data);

                                    if (notification != null)
                                    {
                                        // Process the new notification.
                                        this._manager.NewNotification(notification);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    this._logger.LogError(ex.ToString());
                    if(this.Running)
                        this.RestartSocket();
                }
            }
            finally
            {
                this.Connect();
            }
        }

        internal async void Dispose()
        {
            this.Running = false;
            await this.webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", tokenSource.Token);
        }

        public bool SendListenRequest()
        {
            if (this.webSocket == null)
                return false;

            if (this.webSocket.State != WebSocketState.Open)
                return false;
            try
            {
                IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
                string ipAddress = "0.0.0.0";
                if (ipHost.AddressList.Length > 0)
                    ipAddress = ipHost.AddressList[0].ToString();

                ClientContextWrapper context = new ClientContextWrapper(new ClientContext(ipAddress), this._manager.SubscriberId, null);
                byte[] messageBytes = Encoding.Default.GetBytes(JsonConvert.SerializeObject(context));
                ArraySegment<byte> bytes = new ArraySegment<byte>(messageBytes, 0, messageBytes.Length);

                Task task = this.webSocket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
                task.Wait();
                return task.IsCompletedSuccessfully;
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex.ToString());                
                return false;
            }
        }

        public async void RestartSocket()
        {
            this.Running = false;

            if (this.webSocket == null)
                return;

            if (this.webSocket.State == WebSocketState.Open
                || this.webSocket.State == WebSocketState.CloseReceived
                || this.webSocket.State == WebSocketState.CloseSent)
                await this.webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, "Closing", CancellationToken.None);
        }
    }
}
