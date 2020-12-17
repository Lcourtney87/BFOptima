using Microsoft.Extensions.Logging;
using PoseidonLogic.Connections;
using PoseidonLogic.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PoseidonLogic
{
    public class PoseidonManager
    {
        public readonly string POSEIDON_API = "https://velsv.betfredsports.com/services";
        public readonly string POSEIDON_SOCKET = "wss://velnt.betfredsports.com/notification/listen/websocket";

        public string SubscriberId { get; set; }

        public readonly PoseidonSocket PoseidonSocket;

        public readonly PoseidonAPI PoseidonAPI;

        public readonly PingHandler PingHandler;

        public readonly ChangeProcessor ChangeProcessor;

        public readonly Toolbox.NavList NavList;

        private readonly ILogger _logger;


        public PoseidonManager()
        {
            this._logger = ApplicationLogging.CreateLogger<PoseidonManager>();

            this.PoseidonSocket = new PoseidonSocket(this);
            this.PoseidonAPI = new PoseidonAPI(this);
            this.PingHandler = new PingHandler(this);
            this.ChangeProcessor = new ChangeProcessor(this);
            this.NavList = new Toolbox.NavList(this);
        }        

        public void Begin()
        {
            this.PoseidonSocket.Connect();
        }


        private void SubscriptionDead()
        {
            if (string.IsNullOrEmpty(this.SubscriberId))
                return;

            this.ChangeProcessor.Clear();
            this.PoseidonSocket.RestartSocket();
        }

        internal void NewNotification(PoseidonNotification notification)
        {
            if (!string.IsNullOrEmpty(notification.errorType))
                this.ProcessError(notification.errorType);
            else
                this.ProcessMessage(notification);
        }

        private void ProcessError(string error)
        {
            switch (error)
            {
                case "SUBSCRIBER_NOT_FOUND":
                    //needs a new subscription, this is more than 120 secs dead.
                    this._logger.LogError("'SUBSCRIBER_NOT_FOUND' message received.");
                    SubscriptionDead();
                    break;
                case "INTERNAL_ERROR":
                    this._logger.LogError("Internal error from websocket.");
                    SubscriptionDead();
                    break;
                default:
                    this._logger.LogWarning($"Unknown Error:{error}");
                    break;
            }
        }

        private void ProcessMessage(PoseidonNotification notification)
        {
            switch (notification.NotificationType)
            {
                case "CONTENT_CHANGES":
                    // Process content changes.
                    this.ChangeProcessor.AddNotification(notification);
                    break;
                case "LISTENING_STARTED":
                    string newId = notification.Data.ToString();
                    bool isNewId = this.SubscriberId != newId;
                    this.SubscriberId = newId;
                    this.NavList.ReadyToMine = true;

                    if (isNewId)
                    {
                        this._logger.LogInformation($"Listening Started {this.SubscriberId}");
                        // Subscribe to all event groups.
                    }
                    else
                    {
                        PingHandler.SubscriberResponsePong();
                    }

                    PingHandler.Start();

                    break;
            }
        }

        public void Dispose()
        {
            this.NavList.Dispose();
            this.PoseidonSocket.Dispose();
            this.PingHandler.Dispose();
            this.ChangeProcessor.Dispose();

        }
    }
}
