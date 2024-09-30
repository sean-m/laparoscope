using System;
using System.Collections.Generic;
using System.Text;

namespace LaparoscopeShared
{
    public class SyncScheduleSettings
    {
        public bool? SyncCycleEnabled { get; set; }
        public bool? SchedulerSuspended { get; set; }
        public bool? MaintenanceEnabled { get; set; }
        public string NextSyncCyclePolicyType { get; set; }
    }
}
