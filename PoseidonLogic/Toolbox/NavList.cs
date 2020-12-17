using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PoseidonLogic.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PoseidonLogic.Toolbox
{
    public class NavList
    {
        private readonly int repeat_Interval = 10;

        public bool ReadyToMine = false;

        private Task MiningTask { get; set; }
        private readonly PoseidonManager _manager;
        private readonly ILogger _logger;

        public NavList(PoseidonManager manager)
        {
            this._manager = manager;
            this._logger = ApplicationLogging.CreateLogger<NavList>();
            this.MiningTask = Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        if (!ReadyToMine)
                        {
                            Thread.Sleep(3000);
                        }
                        else
                        {
                            this.BuildNavigationList();

                            DateTime now = DateTime.Now;
                            // how many INTERVALS passed add one minute or max intervals per hour multiplied by Interval add 1 safety minute
                            int nextTrigger = (Math.Min((now.Minute == 0 ? 1 : now.Minute / this.repeat_Interval) + 1, (60 / this.repeat_Interval)) * this.repeat_Interval);

                            DateTime then = DateTime.Now.AddMinutes(-now.Minute).AddSeconds(-now.Second).AddMinutes(nextTrigger);
                            TimeSpan difference = (then - now);
                            this._logger.LogInformation($"Next rebuild in {(int)difference.TotalMinutes} minutes");

                            Thread.Sleep(difference);
                        }
                    }
                    catch (Exception ex)
                    {
                        this._logger.LogError(ex.ToString());
                    }
                }
            });
        }

        public async void BuildNavigationList()
        {
            try
            {
                this._logger.LogInformation("Getting Nav list...");
                Dictionary<string, EventGroupModel> eventDictionary = new Dictionary<string, EventGroupModel>();

                OptimaRequest request = new OptimaRequest
                {
                    contentId = new Content("boNavigationList", "1355/top")
                };

                string data = this._manager.PoseidonAPI.MakeRequest(request, "get").GetAwaiter().GetResult().responseData;

                if (!string.IsNullOrEmpty(data))
                {
                    PoseidonNotification notification = JsonConvert.DeserializeObject<PoseidonNotification>(data);
                    NavigationNode topnode = JsonConvert.DeserializeObject<NavigationNode>(notification.Data.ToString());
                    topnode.idfwbonavigation = "1";

                    foreach (NavigationNode node in topnode.bonavigationnodes)
                    {
                        if (node.nummarkets == 0)
                        {
                            continue;
                        }
                        try
                        {
                            request = new OptimaRequest
                            {
                                contentId = new Content("boNavigationList", $"1355/{node.idfwbonavigation}")
                            };

                            string sportsData = this._manager.PoseidonAPI.MakeRequest(request, "get").GetAwaiter().GetResult().responseData;
                            if (!string.IsNullOrEmpty(sportsData))
                            {
                                PoseidonNotification sportNotification = JsonConvert.DeserializeObject<PoseidonNotification>(sportsData);
                                if (sportNotification != null)
                                {
                                    NavigationNode newnode = JsonConvert.DeserializeObject<NavigationNode>(sportNotification.Data.ToString());
                                    if (newnode != null)
                                    {
                                        Dictionary<string, EventGroupModel> sportDic = IterateNodeTree_M(newnode, new Dictionary<string, EventGroupModel>(), node.name);
                                        sportDic.ToList().ForEach(newEvent =>
                                        {
                                            if (!eventDictionary.ContainsKey(newEvent.Key))
                                            {
                                                eventDictionary.Add(newEvent.Key, newEvent.Value);
                                            }
                                        });
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            this._logger.LogError(ex.ToString());
                        }
                    }
                }

                List<EventGroupModel> result = eventDictionary.Select(p => p.Value).ToList();

                this._manager.PoseidonAPI.SubscribeToEventGroup(result.Select(p => p.GroupId));
                this._logger.LogInformation($"Found {result.Count()} EventGroupModels");
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex.ToString());
            }
        }

        private Dictionary<string, EventGroupModel> IterateNodeTree_M(NavigationNode topnode, Dictionary<string, EventGroupModel> eventGroupsModels, string sportName)
        {
            try
            {
                int id = Convert.ToInt32(Convert.ToDecimal(topnode.idfwbonavigation));

                foreach (NavigationNode node in topnode.bonavigationnodes)
                {
                    if (node.nummarkets == 0) continue;

                    OptimaRequest request = new OptimaRequest
                    {
                        contentId = new Content("boNavigationList", $"1355/{node.idfwbonavigation}")
                    };

                    string data = this._manager.PoseidonAPI.MakeRequest(request, "get").GetAwaiter().GetResult().responseData;

                    if (!string.IsNullOrEmpty(data))
                    {
                        PoseidonNotification notification = JsonConvert.DeserializeObject<PoseidonNotification>(data);
                        if (notification == null) continue;

                        NavigationNode newnode = JsonConvert.DeserializeObject<NavigationNode>(notification.Data.ToString());
                        if (newnode == null || newnode.nummarkets == 0) continue;

                        eventGroupsModels = IterateNodeTree_M(newnode, eventGroupsModels, sportName);
                    }
                }

                foreach (MarketGroup marketGroup in topnode.marketgroups)
                {
                    if (!eventGroupsModels.ContainsKey(marketGroup.idfwmarketgroup) && !Toolbox.GroupCache.HasGroup(marketGroup.idfwmarketgroup))
                    {
                        EventGroupModel newEventGroup = new EventGroupModel()
                        {
                            Sport = sportName,
                            Tournament = topnode.name,
                            GroupName = marketGroup.name,
                            GroupId = marketGroup.idfwmarketgroup,
                            eventIds = new HashSet<decimal>(),
                            IsSubscribed = true
                        };
                        eventGroupsModels.Add(marketGroup.idfwmarketgroup, newEventGroup);
                    }
                }
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex.ToString());
            }

            return eventGroupsModels;
        }

        public void Dispose()
        {
        }
    }
}
