using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PoseidonLogic.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PoseidonLogic
{
    public class ChangeProcessor
    {
        private readonly PoseidonManager _manager;
        private readonly ILogger _logger;

        public CancellationTokenSource QueueTokenSource = new CancellationTokenSource();

        private readonly BlockingCollection<PoseidonNotification> changeQueue = new BlockingCollection<PoseidonNotification>();

        private bool _running = true;
        public ChangeProcessor(PoseidonManager manager)
        {
            this._manager = manager;
            this._logger = ApplicationLogging.CreateLogger<ChangeProcessor>();

            this._running = true;
            Task.Run(this.ProcessQueue);
        }

        public void AddNotification(PoseidonNotification notification)
        {
            this.changeQueue.Add(notification);
        }

        private void ProcessQueue()
        {
            while (this._running)
            {
                foreach (PoseidonNotification notification in this.changeQueue.GetConsumingEnumerable(this.QueueTokenSource.Token))
                {
                    try
                    {
                        List<ContentChange> contentChanges = new List<ContentChange>();
                        IEnumerable<JObject> jobjectChanges = JsonConvert.DeserializeObject<IEnumerable<JObject>>(notification.Data.ToString());
                        foreach (JObject jObject in jobjectChanges)
                        {
                            if (jObject.ContainsKey("changeType"))
                                contentChanges.Add(jObject.ToObject<ContentChange>());
                            else
                                this._logger.LogWarning("No change type received");
                        }

                        if (contentChanges.Any())
                        {
                            this.ProcessChanges(contentChanges);
                        }
                    }
                    catch (Exception ex)
                    {
                        this._logger.LogWarning(ex.ToString());
                    }
                }
            }
        }

        internal void Clear()
        {
            while (this.changeQueue.TryTake(out _)) { };
        }

        private void ProcessChanges(IEnumerable<ContentChange> changes)
        {
            if (changes == null) return;

            foreach (ContentChange change in changes)
            {
                int? id;
                string type;
                this.GetIdOfChange(change, out id, out type);

                if (id == null)
                {

                    continue;
                }

                if (change.contentId.type == "marketGroup" || change.contentId.type == "eventGroup" || change.contentId.type == "event" || change.contentId.type == "market" || change.contentId.type == "selection")
                {
                    switch (change.changeType)
                    {
                        case "updated":
                            // process object update.
                            break;
                        case "removed":
                            // process object removal
                            break;
                        case "added":
                            // Process object additions, if event, subscribe
                            if (change.path.Contains("idfoevent"))
                            {
                                this._manager.PoseidonAPI.SubscribeToEvent(id.GetValueOrDefault());
                            }
                            break;
                        case "refreshed":
                            this.ProcessRefresh(change);
                            break;
                        default:
                            this._logger.LogWarning($"Unprocessed Message of changeType: '{change.changeType}'.");
                            break;
                    }
                }
                else
                {
                    this._logger.LogWarning($"Unprocessed Message of contentId: '{change.contentId.type}'");
                }
            }
        }

        private void ProcessRefresh(ContentChange change)
        {
            if(change.change == null)
            {
                if (change.contentId.type == "eventGroup" && change.contentId.id == "-1")
                {
                    this._manager.PingHandler.EventGroupResponsePong();
                 
                }
                else
                {
                    this._logger.LogWarning("Received refresh with no content");
                }

                return;
            }            

            switch (change.contentId.type)
            {
                // Subscribe to all markets received to ensure we get the data.
                case "marketGroup":
                case "eventGroup":
                    JObject jEventGroup = change.change as JObject;
                    foreach (var newEvent in jEventGroup["events"])
                    {
                        JObject jObj = newEvent as JObject;
                        int eventID = jObj["idfoevent"].ToString().ConvertToInt();
                        this._manager.PoseidonAPI.SubscribeToEvent(eventID);
                    }
                    break;
                // Process object refreshes similarly to updates.
                case "event":
                    break;
                case "market":
                    break;
                case "selection":
                    break;
            }
        }

        private void GetIdOfChange(ContentChange change, out int? id, out string type)
        {
            id = null;
            type = null;

            if (change.path == null)
            {
                if (change.contentId == null)
                {
                    this._logger.LogWarning("Refresh with no contentChange.change. No contentId");
                    return;
                }
                else
                    id = change.contentId.id.ConvertToInt();
            }
            else
            {
                var splitPath = change.path.Split('|');
                if (!splitPath.Last().Contains("idfo"))
                {
                    if (splitPath.Count() > 1)
                    {
                        string target = splitPath[splitPath.Length - 2].Replace("]", "");
                        target = target.Split('[').Last();
                        var targetSplit = target.Split("=");
                        string tempStringID = targetSplit[1];
                        type = targetSplit[0];
                        tempStringID = tempStringID.Remove(tempStringID.IndexOf('.'));
                        id = Convert.ToInt32(tempStringID);
                    }
                    else
                        id = change.contentId.id.ConvertToInt();
                }
                else
                {
                    string idString = splitPath.Last();
                    idString = idString.Substring(idString.IndexOf('=') + 1);
                    idString = idString.Remove(idString.IndexOf('.'));
                    id = Convert.ToInt32(idString);
                }
            }
        }

        public void Dispose()
        {
            this._running = false;
            this.Clear();
        }
    }
}
