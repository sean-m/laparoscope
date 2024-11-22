using Laparoscope.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LaparoscopeShared.Models
{
    public class HashSyncMetric
    {
        public string Name { get; set; }
        public long LastCycle { get; set; }
        public long LastSuccess { get; set; }
        public float LastSuccessDuration { get; set; }
        public string LastStatus { get; set; }

        public HashSyncMetric() { }        
    }
}
