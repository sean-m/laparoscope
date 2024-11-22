using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laparoscope.Models
{
    public class PasswordSyncState
    {
        public dynamic ConnectorId { get; set; }
        public string DN { get; set; }
        public dynamic PasswordSyncLastSuccessfulCycleStartTimestamp { get; set; }
        public dynamic PasswordSyncLastSuccessfulCycleEndTimestamp { get; set; }
        public dynamic PasswordSyncLastCycleStartTimestamp { get; set; }
        public dynamic PasswordSyncLastCycleEndTimestamp { get; set; }
        public dynamic PasswordSyncLastCycleStatus { get; set; }
    }
}