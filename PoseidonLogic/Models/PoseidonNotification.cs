using System;
using System.Collections.Generic;
using System.Text;

namespace PoseidonLogic.Models
{
    public class PoseidonNotification
    {
        public object Data { get; set; }

        public string NotificationType { get; set; }

        public string errorType { get; set; }

        public int Version { get; set; }
    }
}
