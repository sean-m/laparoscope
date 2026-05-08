using System;
using System.Collections.Generic;

namespace LaparoscopeShared.Models.RunHistory
{
    /// <summary>
    /// Represents the result of a sync run profile execution.
    /// Maps to Microsoft.IdentityManagement.PowerShell.ObjectModel.RunProfileResult
    /// </summary>
    public class RunProfileResult
    {
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

    /// <summary>
    /// Represents the result of a single step in a sync run profile.
    /// Maps to Microsoft.IdentityManagement.PowerShell.ObjectModel.RunStepResult
    /// </summary>
    public class RunStepResult
    {
        public string RunHistoryId { get; set; }
        public string StepHistoryId { get; set; }
        public int StepNumber { get; set; }
        public string StepResult { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        // Stage counters
        public int StageNoChange { get; set; }
        public int StageAdd { get; set; }
        public int StageUpdate { get; set; }
        public int StageRename { get; set; }
        public int StageDelete { get; set; }
        public int StageDeleteAdd { get; set; }
        public int StageFailure { get; set; }

        // Disconnector counters
        public int DisconnectorFiltered { get; set; }
        public int DisconnectorJoinedNoFlow { get; set; }
        public int DisconnectorJoinedFlow { get; set; }
        public int DisconnectorJoinedRemoveMv { get; set; }
        public int DisconnectorProjectedNoFlow { get; set; }
        public int DisconnectorProjectedFlow { get; set; }
        public int DisconnectorProjectedRemoveMv { get; set; }
        public int DisconnectorRemains { get; set; }

        // Connector counters
        public int ConnectorFilteredRemoveMv { get; set; }
        public int ConnectorFilteredLeaveMv { get; set; }
        public int ConnectorFlow { get; set; }
        public int ConnectorFlowRemoveMv { get; set; }
        public int ConnectorNoFlow { get; set; }
        public int ConnectorDeleteRemoveMv { get; set; }
        public int ConnectorDeleteLeaveMv { get; set; }
        public int ConnectorDeleteAddProcessed { get; set; }

        // Flow counters
        public int FlowFailure { get; set; }

        // Export counters
        public int ExportAdd { get; set; }
        public int ExportUpdate { get; set; }
        public int ExportRename { get; set; }
        public int ExportDelete { get; set; }
        public int ExportDeleteAdd { get; set; }
        public int ExportFailure { get; set; }
        public int CurrentExportBatchNumber { get; set; }
        public int LastSuccessfulExportBatchNumber { get; set; }

        // Additional information
        public string StepFileName { get; set; }
        public string ConnectorConnectionInformationXml { get; set; }
        public RunStepResultConnectorDiscoveryErrors ConnectorDiscoveryErrors { get; set; }
        public string ConnectorCountersXml { get; set; }
        public RunStepResultSyncErrors SyncErrors { get; set; }
        public string StepXml { get; set; }
        public RunStepResultMvRetryErrors MvRetryErrors { get; set; }
        public string FlowCountersXml { get; set; }
    }

    /// <summary>
    /// Represents connector discovery errors for a run step.
    /// Maps to Microsoft.IdentityManagement.PowerShell.ObjectModel.RunStepResultConnectorDiscoveryErrors
    /// </summary>
    public class RunStepResultConnectorDiscoveryErrors
    {
        public string ConnectorDiscoveryErrorsXml { get; set; }
        public IEnumerable<RunStepErrorObject> ConnectorDiscoveryErrorsList { get; set; }
        public string ConnectorDiscoveryErrorsSummaryXml { get; set; }
    }

    /// <summary>
    /// Represents sync errors for a run step.
    /// Maps to Microsoft.IdentityManagement.PowerShell.ObjectModel.RunStepResultSyncErrors
    /// </summary>
    public class RunStepResultSyncErrors
    {
        public byte[] SyncErrorsCompressed { get; set; }
        public string SyncErrorsXml { get; set; }
        public IEnumerable<RunStepErrorObject> SyncErrorsList { get; set; }
        public string SyncErrorsSummaryXml { get; set; }
    }

    /// <summary>
    /// Represents metaverse retry errors for a run step.
    /// Maps to Microsoft.IdentityManagement.PowerShell.ObjectModel.RunStepResultMvRetryErrors
    /// </summary>
    public class RunStepResultMvRetryErrors
    {
        public string MvRetryErrorsXml { get; set; }
        public IEnumerable<RunStepErrorObject> MvRetryErrorsList { get; set; }
        public string MvRetryErrorsSummaryXml { get; set; }
    }

    /// <summary>
    /// Represents a sync error object.
    /// Maps to Microsoft.IdentityManagement.PowerShell.ObjectModel.RunStepErrorObject
    /// </summary>
    public class RunStepErrorObject
    {
        public string ObjectDisplayName { get; set; }
        public string ObjectDn { get; set; }
        public Guid? CsGuid { get; set; }
        public Guid? MvGuid { get; set; }
        public DateTime? FirstOccurred { get; set; }
        public int? RetryCount { get; set; }
        public DateTime? DateOccurred { get; set; }
        public string ErrorType { get; set; }
        public string AlgorithmStep { get; set; }
        public int? LineNumber { get; set; }
        public int? ColumnNumber { get; set; }
        public int? EntryNumber { get; set; }
        public string ExtensionErrorInfo { get; set; }
        public string CdError { get; set; }
    }
}
