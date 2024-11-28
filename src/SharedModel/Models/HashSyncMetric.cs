using Laparoscope.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LaparoscopeShared.Models
{
    public class HashSyncMetric
    {
        public string Name { get; set; }
        public string LastCycle { get; set; }
        public string LastSuccess { get; set; }
        public string LastSuccessDuration { get; set; }
        public string LastStatus { get; set; }

        public HashSyncMetric() { }        
    }
}
