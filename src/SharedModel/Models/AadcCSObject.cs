using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laparoscope.Models
{
    public class SyncRuleIdentifier
    {
        public string SyncRuleInternalId { get; set; }
        public string SyncRuleName { get; set; }
        public string Operation { get; set; }

        public SyncRuleIdentifier() { }
    }

    public class Attribute
    {
        public string Name { get; set; }
        public int Type { get; set; }
        public bool IsMultiValued { get; set; }
        public string SyncRuleName { get; set; }
        public string ConnectorName { get; set; }
        public string LineageId { get; set; }
        public string LastModificationTime { get; set; }
        public List<string> Values { get; set; }

        public Attribute() { }
    }

    public class AadcCSObject
    {
        public string ObjectId { get; set; }
        public string ConnectorId { get; set; }
        public string ConnectorName { get; set; }
        public string ConnectorType { get; set; }
        public string PartitionId { get; set; }
        public string DistinguishedName { get; set; }
        public string ObjectType { get; set; }
        public bool IsTransient { get; set; }
        public bool IsPlaceHolder { get; set; }
        public bool IsConnector { get; set; }
        public bool HasSyncError { get; set; }
        public bool HasExportError { get; set; }
        public object ExportError { get; set; }
        public object SynchronizationError { get; set; }
        public string ConnectedMVObjectId { get; set; }
        public List<SyncRuleIdentifier> Lineage { get; set; }
        public List<Attribute> Attributes { get; set; }

        public AadcCSObject()
        {
            Lineage = new List<SyncRuleIdentifier>();
            Attributes = new List<Attribute>();
        }
    }
}