using System;
using System.Collections.Generic;
using System.Text;

namespace PoseidonLogic.Toolbox
{
    public static class GroupCache
    {
        private static object cacheLock = new object();

        private static List<string> eventGroupIds = new List<string>();

        // Check whether we have already subscribed.
        internal static bool AddEventGroupSubscription(string eventGroupId)
        {
            lock (cacheLock)
            {
                if (eventGroupIds.Contains(eventGroupId))
                    return false;

                eventGroupIds.Add(eventGroupId);
                return true;
            }
        }

        internal static bool HasGroup(string idfwmarketgroup)
        {
            lock (cacheLock)
            {
                return eventGroupIds.Contains(idfwmarketgroup);
            }
        }
    }
}
