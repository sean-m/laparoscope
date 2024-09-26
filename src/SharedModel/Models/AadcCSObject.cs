using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace aadcapi.Models
{
    public class AadcCSObject
    {
        public dynamic ObjectId { get; set; }
        public dynamic ConnectorId { get; set; }
        public dynamic ConnectorName { get; set; }
        public dynamic ConnectorType { get; set; }
        public dynamic PartitionId { get; set; }
        public dynamic DistinguishedName { get; set; }
        public dynamic AnchorValue { get; set; }
        public dynamic ObjectType { get; set; }
        public dynamic IsTransient { get; set; }
        public dynamic IsPlaceHolder { get; set; }
        public dynamic IsConnector { get; set; }
        public dynamic HasSyncError { get; set; }
        public dynamic HasExportError { get; set; }
        public dynamic ExportError { get; set; }
        public dynamic SynchronizationError { get; set; }
        public dynamic ConnectedMVObjectId { get; set; }
        public dynamic Lineage { get; set; }
        public dynamic Attributes { get; set; }
    }
}