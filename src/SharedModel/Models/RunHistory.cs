using System;
using System.Collections.Generic;
using System.Text;

namespace LaparoscopeShared.Models
{
    //public class RunHistory
    //{
    //    public Guid RunHistoryId { get; set; }
    //    public Guid ConnectorId { get; set; }
    //    public string ConnectorName { get; set; }
    //    public Guid RunProfileId { get; set; }
    //    public string RunProfileName { get; set; }
    //    public int RunNumber { get; set; }
    //    public string Username { get; set; }
    //    public bool IsRunComplete { get; set; }
    //    public string Result { get; set; }
    //    public int CurrentStepNumber { get; set; }
    //    public int TotalSteps { get; set; }
    //    public DateTime StartDate { get; set; }
    //    public DateTime EndDate { get; set; }
    //    public List<RunStepResult> RunStepResults { get; set; }

    //    public RunHistory()
    //    {
    //        RunStepResults = new List<RunStepResult>();
    //    }
    //}

    //public class RunStepResult
    //{
    //    public Guid RunHistoryId { get; set; }
    //    public Guid StepHistoryId { get; set; }
    //    public int StepNumber { get; set; }
    //    public string StepResult { get; set; }
    //    public DateTime StartDate { get; set; }
    //    public DateTime EndDate { get; set; }
    //    public int StageNoChange { get; set; }
    //    public int StageAdd { get; set; }
    //    public int StageUpdate { get; set; }
    //    public int StageRename { get; set; }
    //    public int StageDelete { get; set; }
    //    public int StageDeleteAdd { get; set; }
    //    public int StageFailure { get; set; }
    //    public int DisconnectorFiltered { get; set; }
    //    public int DisconnectorJoinedNoFlow { get; set; }
    //    public int DisconnectorJoinedFlow { get; set; }
    //    public int DisconnectorJoinedRemoveMv { get; set; }
    //    public int DisconnectorProjectedNoFlow { get; set; }
    //    public int DisconnectorProjectedFlow { get; set; }
    //    public int DisconnectorProjectedRemoveMv { get; set; }
    //    public int DisconnectorRemains { get; set; }
    //    public int ConnectorFilteredRemoveMv { get; set; }
    //    public int ConnectorFilteredLeaveMv { get; set; }
    //    public int ConnectorFlow { get; set; }
    //    public int ConnectorFlowRemoveMv { get; set; }
    //    public int ConnectorNoFlow { get; set; }
    //    public int ConnectorDeleteRemoveMv { get; set; }
    //    public int ConnectorDeleteLeaveMv { get; set; }
    //    public int ConnectorDeleteAddProcessed { get; set; }
    //    public int FlowFailure { get; set; }
    //    public int ExportAdd { get; set; }
    //    public int ExportUpdate { get; set; }
    //    public int ExportRename { get; set; }
    //    public int ExportDelete { get; set; }
    //    public int ExportDeleteAdd { get; set; }
    //    public int ExportFailure { get; set; }
    //    public int CurrentExportBatchNumber { get; set; }
    //    public int LastSuccessfulExportBatchNumber { get; set; }
    //    public string StepFileName { get; set; }
    //    public string ConnectorConnectionInformationXml { get; set; }
    //    public ConnectorDiscoveryErrors ConnectorDiscoveryErrors { get; set; }

    //    public RunStepResult()
    //    {
    //        ConnectorDiscoveryErrors = new ConnectorDiscoveryErrors();
    //    }
    //}

    //public class ConnectorDiscoveryErrors
    //{
    //    public string ConnectorDiscoveryErrorsXml { get; set; }
    //    public List<string> ConnectorDiscoveryErrorsList { get; set; }
    //    public string ConnectorDiscoveryErrorsSummaryXml { get; set; }

    //    public ConnectorDiscoveryErrors()
    //    {
    //        ConnectorDiscoveryErrorsList = new List<string>();
    //    }
    //}

    using System;
    using System.Collections.Generic;

    public class RunStepResult
    {
        public Guid RunHistoryId { get; set; }
        public Guid StepHistoryId { get; set; }
        public int StepNumber { get; set; }
        public string StepResult { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int StageNoChange { get; set; }
        public int StageAdd { get; set; }
        public int StageUpdate { get; set; }
        public int StageRename { get; set; }
        public int StageDelete { get; set; }
        public int StageDeleteAdd { get; set; }
        public int StageFailure { get; set; }
        public int DisconnectorFiltered { get; set; }
        public int DisconnectorJoinedNoFlow { get; set; }
        public int DisconnectorJoinedFlow { get; set; }
        public int DisconnectorJoinedRemoveMv { get; set; }
        public int DisconnectorProjectedNoFlow { get; set; }
        public int DisconnectorProjectedFlow { get; set; }
        public int DisconnectorProjectedRemoveMv { get; set; }
        public int DisconnectorRemains { get; set; }
        public int ConnectorFilteredRemoveMv { get; set; }
        public int ConnectorFilteredLeaveMv { get; set; }
        public int ConnectorFlow { get; set; }
        public int ConnectorFlowRemoveMv { get; set; }
        public int ConnectorNoFlow { get; set; }
        public int ConnectorDeleteRemoveMv { get; set; }
        public int ConnectorDeleteLeaveMv { get; set; }
        public int ConnectorDeleteAddProcessed { get; set; }
        public int FlowFailure { get; set; }
        public int ExportAdd { get; set; }
        public int ExportUpdate { get; set; }
        public int ExportRename { get; set; }
        public int ExportDelete { get; set; }
        public int ExportDeleteAdd { get; set; }
        public int ExportFailure { get; set; }
        public int CurrentExportBatchNumber { get; set; }
        public int LastSuccessfulExportBatchNumber { get; set; }
        public string StepFileName { get; set; }
        public string ConnectorConnectionInformationXml { get; set; }
        public string ConnectorCountersXml { get; set; }
        public string StepXml { get; set; }
        //public ConnectorDiscoveryErrors ConnectorDiscoveryErrors { get; set; }
        //public MvRetryErrors MvRetryErrors { get; set; }
        //public SyncErrors SyncErrors { get; set; }
        public string FlowCountersXml { get; set; }

        public RunStepResult()
        {
            
        }
    }

    public class ConnectorDiscoveryErrors
    {
        public string ConnectorDiscoveryErrorsXml { get; set; }
        public List<object> ConnectorDiscoveryErrorsList { get; set; }
        public string ConnectorDiscoveryErrorsSummaryXml { get; set; }
    }

    public class SyncErrors
    {
        public List<object> SyncErrorsCompressed { get; set; }
        public string SyncErrorsXml { get; set; }
        public List<object> SyncErrorsList { get; set; }
        public string SyncErrorsSummaryXml { get; set; }

        public SyncErrors()
        {
            
        }
    }

    public class MvRetryErrors
    {
        public string MvRetryErrorsXml { get; set; }
        public List<object> MvRetryErrorsList { get; set; }
        public string MvRetryErrorsSummaryXml { get; set; }

        public MvRetryErrors()
        {
                
        }
    }

    public class RunHistory
    {
        public Guid RunHistoryId { get; set; }
        public Guid ConnectorId { get; set; }
        public string ConnectorName { get; set; }
        public Guid RunProfileId { get; set; }
        public string RunProfileName { get; set; }
        public int RunNumber { get; set; }
        public string Username { get; set; }
        public bool IsRunComplete { get; set; }
        public string Result { get; set; }
        public int CurrentStepNumber { get; set; }
        public int TotalSteps { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<RunStepResult> RunStepResults { get; set; }

        public RunHistory()
        {
                
        }
    }

}
