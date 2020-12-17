using Microsoft.Extensions.Logging;
using PoseidonLogic.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace PoseidonLogic.Connections
{
    public class PingHandler
    {
        private System.Timers.Timer PingTimer { get; set; }

        private int failed_Socket_Pings = 0;

        private int failed_Subscribe_Pings = 0;

        private int missed_Subscribe_Pongs = 0;

        private int missed_EventGroup_Pongs = 0;

        private readonly PoseidonManager _manager;

        private readonly ILogger _logger;

        public PingHandler(PoseidonManager manager)
        {
            this._manager = manager;
            this._logger = ApplicationLogging.CreateLogger<PingHandler>();
        }

        public void Start()
        {
            if (PingTimer != null)
                PingTimer.Dispose();

            PingTimer = new System.Timers.Timer(TimeSpan.FromSeconds(10).TotalMilliseconds);
            PingTimer.Elapsed += Ping;
            PingTimer.AutoReset = false;
            PingTimer.Enabled = true;
        }

        public async void Ping(object source, ElapsedEventArgs e)
        {
            try
            {
                if (this._manager.PoseidonSocket.SendListenRequest())
                {
                    failed_Socket_Pings = 0;

                    SubscriptionRequest request = new SubscriptionRequest()
                    {
                        contentId = new Content("eventGroup", "-1"),
                        subscriberId = this._manager.SubscriberId
                    };

                    PoseidonResponse response = await this._manager.PoseidonAPI.MakeRequest(request, "subscribe");
                    if (string.IsNullOrEmpty(response.error))
                        failed_Subscribe_Pings = 0;
                    else
                        failed_Subscribe_Pings++;
                }
            }
            catch (Exception ex)
            {
                this._logger.LogError($"Ping failed: {ex}");
                failed_Socket_Pings++;
            }
        }

        public static void Stop()
        {

        }

        internal void SubscriberResponsePong()
        {
            this.missed_Subscribe_Pongs = 0;
            this._logger.LogInformation("Received subscriber pong");
        }

        internal void EventGroupResponsePong()
        {
            this.missed_EventGroup_Pongs = 0;
            this._logger.LogInformation("Received eventgroup pong");
        }

        internal void Dispose()
        {
            this.PingTimer.Dispose();
        }
    }
}
