using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laparoscope.Models
{
    public class SyncRule
    {
        public dynamic Identifier { get; set; }
        public dynamic InternalId { get; set; }
        public dynamic Name { get; set; }
        public dynamic Version { get; set; }
        public dynamic Description { get; set; }
        public dynamic ImmutableTag { get; set; }
        public dynamic Connector { get; set; }
        public dynamic Direction { get; set; }
        public dynamic Disabled { get; set; }
        public dynamic SourceObjectType { get; set; }
        public dynamic TargetObjectType { get; set; }
        public dynamic Precedence { get; set; }
        public dynamic PrecedenceAfter { get; set; }
        public dynamic PrecedenceBefore { get; set; }
        public dynamic LinkType { get; set; }
        public dynamic EnablePasswordSync { get; set; }
        public dynamic JoinFilter { get; set; }
        public dynamic ScopeFilter { get; set; }
        public dynamic AttributeFlowMappings { get; set; }
        public dynamic SoftDeleteExpiryInterval { get; set; }
        public dynamic SourceNamespaceId { get; set; }
        public dynamic TargetNamespaceId { get; set; }
        public dynamic VersionAgnosticTag { get; set; }
        public dynamic TagVersion { get; set; }
        public dynamic IsStandardRule { get; set; }
        public dynamic IsLegacyCustomRule { get; set; }
        public dynamic JoinHash { get; set; }
    }
}