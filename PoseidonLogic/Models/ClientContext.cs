using System;
using System.Collections.Generic;
using System.Text;

namespace PoseidonLogic.Models
{
    public class ClientContextWrapper
    {
        public ClientContext clientContext { get; set; }

        public string subscriberId { get; set; }

        public ReconnectionRequest[] versionList { get; set; }

        public ClientContextWrapper(ClientContext context, string subscriberID, ReconnectionRequest[] marketList)
        {
            this.clientContext = context;
            this.subscriberId = subscriberID;
            this.versionList = marketList;
        }
    }

    public class ClientContext
    {
        public string ipAddress { get; set; }

        public string language { get; set; }

        public ClientContext()
        {
            this.language = "UK";
        }

        public ClientContext(string ipAddress)
        {
            this.ipAddress = ipAddress;
            this.language = "UK";
        }
    }

    public class ReconnectionRequest
    {
        public Content contentId { get; set; }

        public int version { get; set; }

        public ReconnectionRequest(string id, string type, int versionPassed)
        {
            contentId = new Content(type, id);
            version = versionPassed;
        }
    }

    public class Content
    {
        public string id { get; set; }

        public string type { get; set; }

        public Content()
        {

        }

        public Content(string type, string id)
        {
            this.type = type;
            this.id = id;
        }
    }

    public class OptimaRequest
    {
        public ClientContext clientContext { get; set; }

        public Content contentId { get; set; }

        public OptimaRequest()
        {
            this.clientContext = new ClientContext("127.0.0.1");//ConfigurationManager.AppSettings["IPAddress"]);

        }
    }

    public class SubscriptionRequest : OptimaRequest
    {
        public string subscriberId { get; internal set; }
    }

    public class PoseidonResponse
    {
        public string responseData;

        public string error;

        public PoseidonResponse()
        {
            responseData = string.Empty;
            error = string.Empty;
        }
    }

    public class ContentChange
    {
        public string changeType { get; set; }

        public Content contentId { get; set; }

        public string path { get; set; }

        public object change { get; set; }

        public int version { get; set; }
    }
}
