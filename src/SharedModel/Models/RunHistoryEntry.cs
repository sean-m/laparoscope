using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace LaparoscopeShared.Models {

    public class RunHistoryEntry {
        public string RunHistoryId { get; set; }
        public string ConnectorId { get; set; }
        public string ConnectorName { get; set; }
        public string RunProfileId { get; set; }
        public string RunProfileName { get; set; }
        public int RunNumber { get; set; }
        public string Username { get; set; }
        public bool IsRunComplete { get; set; }
        public string Result { get; set; }
        public int CurrentStepNumber { get; set; }
        public int TotalSteps { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public IEnumerable<RunStepResult> RunStepResults { get; set; }
    }

    public class RunStepResult {
        public RunStepResult() { }

        public string ConnectorTypeName { get; set; }
        public string Identifier { get; set; }
        public int Version { get; set; }
        public int InternalVersion { get; set; }
        public int FormatVersion { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastModificationTime { get; set; }
        public Partition[] Partitions { get; set; }
        public Runprofile[] RunProfiles { get; set; }
        public Componentprovisioningmappings ComponentProvisioningMappings { get; set; }
        public Schema Schema { get; set; }
        public Allparameterdefinition[] AllParameterDefinitions { get; set; }
        public ConfigurationParameter[] ConnectivityParameters { get; set; }
        public ConfigurationParameter[] GlobalParameters { get; set; }
        public ConfigurationParameter[] CapabilityParameters { get; set; }
        public ConfigurationParameter[] SchemaParameters { get; set; }
        public string[] ObjectInclusionList { get; set; }
        public string[] AttributeInclusionList { get; set; }
        public Anchorconstructionsetting[] AnchorConstructionSettings { get; set; }
        public string ListName { get; set; }
        public string CompanyName { get; set; }
        public string Type { get; set; }
        public string Subtype { get; set; }
        public bool IsUpgradeOrImportScenario { get; set; }
        public Extensionconfiguration ExtensionConfiguration { get; set; }
        public object PasswordHashConfiguration { get; set; }
        public string AADPasswordResetConfiguration { get; set; }
    }

    public class ConfigurationParameter {
        public string Name { get; set; }
        public int InputType { get; set; }
        public int Scope { get; set; }
        public string Description { get; set; }
        public string RegexValidationPattern { get; set; }
        public string DefaultValue { get; set; }
        public string Value { get; set; }
        public bool Extensible { get; set; }
        public int PageNumber { get; set; }
        public bool Intrinsic { get; set; }
        public int DataType { get; set; }
    }

    public class Componentprovisioningmappings {
    }

    public class Schema {
        public string Identifier { get; set; }
        public string[] ObjectTypes { get; set; }
        public string[] AttributeTypes { get; set; }
        public bool IsConnectorSchema { get; set; }
        public string[] IntrinsicAttributes { get; set; }
        public object[] AllDNComponents { get; set; }
    }

    public class Extensionconfiguration {
        public int ExportType { get; set; }
        public long CapabilityBits { get; set; }
        public bool IsImportFileBased { get; set; }
        public bool IsExportFileBased { get; set; }
        public bool IsImportEnabled { get; set; }
        public bool IsExportEnabled { get; set; }
        public string ExtensionFileName { get; set; }
        public int CachedImportDefaultPageSize { get; set; }
        public int CachedExportDefaultPageSize { get; set; }
        public int CachedImportMaxPageSize { get; set; }
        public int CachedExportMaxPageSize { get; set; }
        public bool PartitionDiscoveryEnabled { get; set; }
        public bool SchemaDiscoveryEnabled { get; set; }
        public bool HierarchyDiscoveryEnabled { get; set; }
        public bool PasswordManagementEnabled { get; set; }
        public Assemblyversion AssemblyVersion { get; set; }
    }

    public class Assemblyversion {
        public int Major { get; set; }
        public int Minor { get; set; }
        public int Build { get; set; }
        public int Revision { get; set; }
        public int MajorRevision { get; set; }
        public int MinorRevision { get; set; }
    }

    public class Partition {
        public string Identifier { get; set; }
        public string DN { get; set; }
        public int Version { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastModificationTime { get; set; }
        public bool Selected { get; set; }
        public string ConnectorPartitionScope { get; set; }
        public string Name { get; set; }
        public string Parameters { get; set; }
        public string PreferredDCs { get; set; }
        public bool IsDomain { get; set; }
    }

    public class Runprofile {
        public string Identifier { get; set; }
        public string Name { get; set; }
        public string RunSteps { get; set; }
        public string ConnectorIdentifier { get; set; }
        public int Version { get; set; }
    }

    public class Allparameterdefinition {
        public string Name { get; set; }
        public int InputType { get; set; }
        public int Scope { get; set; }
        public string Description { get; set; }
        public string RegexValidationPattern { get; set; }
        public string DefaultValue { get; set; }
        public object Value { get; set; }
        public bool Extensible { get; set; }
        public int PageNumber { get; set; }
        public bool Intrinsic { get; set; }
        public int DataType { get; set; }
    }

    public class Anchorconstructionsetting {
        public string ObjectType { get; set; }
        public string Attributes { get; set; }
        public bool Locked { get; set; }
    }

}
