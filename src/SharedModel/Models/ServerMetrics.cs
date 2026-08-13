using System;
using System.Collections.Generic;
using System.Text;

namespace Laparoscope.Models
{
    public class ServerMetrics
    {
        public IEnumerable<HashSyncMetric> HashSync { get; set; }

    }
}
