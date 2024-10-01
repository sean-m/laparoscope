using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace aadcapi.Models
{
    public class AadcConnector
    {
        public dynamic Name { get; set; }
        public dynamic Identifier { get; set; }
        public dynamic Description { get; set; }
        public dynamic CreationTime { get; set; }
        public dynamic LastModificationTime { get; set; }
        public List<string> ObjectInclusionList { get; set; }
        public List<string> AttributeInclusionList { get; set; }
        public Dictionary<string,object> ConnectivityParameters { get; set; }
        public dynamic Partitions { get; set; }
        public dynamic AnchorConstructionSettings { get; set; }
        public dynamic CompanyName { get; set; }
        public dynamic Type { get; set; }
        public dynamic Subtype { get; set; }

        public class ConnectorPartitionScope
        {
            public List<string> ObjectClasses { get; set; }
            public List<string> ContainerInclusionList { get; set; }
            public List<string> ContainerExclusionList { get; set; }

            public ConnectorPartitionScope()
            {
                ObjectClasses = new List<string>();
                ContainerInclusionList = new List<string>();
                ContainerExclusionList = new List<string>();
            }
        }

        public class Partition
        {
            public Guid Identifier { get; set; }
            public string DN { get; set; }
            public int Version { get; set; }
            public DateTime CreationTime { get; set; }
            public DateTime LastModificationTime { get; set; }
            public bool Selected { get; set; }
            public ConnectorPartitionScope ConnectorPartitionScope { get; set; }
            public string Name { get; set; }
            public object Parameters { get; set; }
            public List<string> PreferredDCs { get; set; }
            public bool IsDomain { get; set; }

            public Partition()
            {
                Identifier = Guid.NewGuid();
                DN = string.Empty;
                Version = 0;
                CreationTime = DateTime.MinValue;
                LastModificationTime = DateTime.MinValue;
                Selected = false;
                ConnectorPartitionScope = new ConnectorPartitionScope();
                Name = string.Empty;
                Parameters = null;
                PreferredDCs = new List<string>();
                IsDomain = false;
            }
        }
    }
}