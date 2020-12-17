using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PoseidonLogic.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace PoseidonLogic.Connections
{
    public class PoseidonAPI
    {
        private PoseidonManager _manager { get; set; }

        private readonly ILogger _logger;
        public PoseidonAPI(PoseidonManager manager)
        {
            this._logger = ApplicationLogging.CreateLogger<PoseidonAPI>();

            this._manager = manager;
        }

        internal async Task<PoseidonResponse> MakeRequest(OptimaRequest request, string target)
        {
            PoseidonResponse result = new PoseidonResponse();

            using (HttpClient client = new HttpClient())
            {
                string json = JsonConvert.SerializeObject(request);
                client.BaseAddress = new Uri($"{this._manager.POSEIDON_API}/content/{target}");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                StringContent message = new StringContent(json, Encoding.UTF8, "application/json");

                using (HttpResponseMessage response = await client.PostAsync($"{this._manager.POSEIDON_API}/content/{target}", message))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string data = await response.Content.ReadAsStringAsync();
                        if (!string.IsNullOrEmpty(data))
                        {
                            PoseidonNotification notification = JsonConvert.DeserializeObject<PoseidonNotification>(data);
                            result.error = notification.errorType;
                            if (notification == null)
                                throw new Exception($"Posiedon {target} failed to return notification");
                            else if (notification.errorType == "SUBSCRIBER_NOT_FOUND")
                                throw new Exception($"Poseidon SUBSCRIBER_NOT_FOUND");
                            else if (notification.errorType == "INTERNAL_ERROR")
                                throw new Exception("Poseidon INTERNAL_ERROR");
                            else if (notification.errorType == "CONTENT_NOT_FOUND")
                                Console.WriteLine($"{target} - {request.contentId.type} - {request.contentId.id} Content not found");
                            else if (notification.errorType != null)
                                throw new Exception($"Poseidon {notification.errorType}");
                            else
                                result.responseData = data;
                        }
                        else
                            this._logger.LogError($"Posiedon {target} failed to return response");
                    }
                    else
                    {
                        throw new Exception($"Access to {client.BaseAddress.AbsoluteUri} Errored with status: {response.StatusCode}");
                    }
                }
            }

            return result;
        }

        internal async void SubscribeToEvent(int eventId)
        {
            SubscriptionRequest request = new SubscriptionRequest
            {
                contentId = new Content("event", $"{eventId}.1"),
                subscriberId = this._manager.SubscriberId
            };

            await this.MakeRequest(request, "subscribe");
        }

        private object _eventGroupLock = new object();
        public void SubscribeToEventGroup(IEnumerable<string> groupIds, bool force = false)
        {
            lock (_eventGroupLock)
            {
                foreach (string groupIdString in groupIds)
                {
                    try
                    {
                        decimal eventGroupId = Convert.ToDecimal(groupIdString);

                        if (!Toolbox.GroupCache.AddEventGroupSubscription(eventGroupId.ToString()))
                            continue;

                        SubscriptionRequest request = new SubscriptionRequest
                        {
                            contentId = new Content("eventGroup", $"{eventGroupId}"),
                            subscriberId = this._manager.SubscriberId
                        };

                        this._logger.LogInformation($"Subscribing to EventGroup {groupIdString}");
                        var response = MakeRequest(request, "subscribe").Result;
                    }
                    catch (Exception ex)
                    {
                        this._logger.LogError(ex.ToString());
                    }
                }
            }
        }
    }
}
