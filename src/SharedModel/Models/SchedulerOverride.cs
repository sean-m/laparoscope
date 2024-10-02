using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laparoscope.Models
{
    public class SchedulerOverride
    {
        public string ConnectorName { get; set; }
        public bool FullSyncRequired { get; set; } = false;
        public bool FullImportRequired { get; set; } = false;
        public string Description = "Populate the connector name and desired sync actions to override on the next scheduled sync. POST that object back to this endpoint.";
    }

    public class SchedulerOverrideResult
    {
        public string ConnectorIdentifier { get; set; }
        public bool FullSyncRequired { get; set; } = false;
        public bool FullImportRequired { get; set; } = false;
    }
}