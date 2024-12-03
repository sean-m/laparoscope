using System;
using System.Collections.Generic;
using System.Text;

namespace LaparoscopeShared.Models
{
    public class ConnectorStatistics
    {
        public string Connector { get; set; }
        public int ExportAdds {get; set; }
        public int ExportUpdates {get; set; }
        public int ExportDeletes {get; set; }
        public int ImportAdds {get; set; }
        public int ImportUpdates {get; set; }
        public int ImportDeletes {get; set; }
        public int ImportNoChange {get; set; }
        public int TotalConnectors { get; set; }
    }
}
