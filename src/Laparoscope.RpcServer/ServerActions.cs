using Laparoscope.Models;
using LaparoscopeShared;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SMM.Automation;
using SMM.Helper;
using StreamJsonRpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Laparoscope.RpcServer
{
    public class ServerActions
    {
        ILogger logger;
        public ServerActions() { }

        public ServerActions(ILogger logger)
        {
            this.logger = logger;
        }

        public string Hello() => "Hi there!";

        public SyncResult StartADSync()
        {
            using (var runner = new SimpleScriptRunner(Properties.Resources.Start_ADSyncDelta))
            {
                runner.Run();
                if (runner.HadErrors)
                {
                    var err = runner.LastError ?? new Exception("Encountered an error in PowerShell but could not capture the exception.");
                    throw err;
                }
                var result = runner.Results.CapturePSResult<SyncResult>().FirstOrDefault();
                return result;
            }
        }

        public AadcConnector[] GetADSyncConnector(string Name = null)
        {
            // Run PowerShell command to get AADC connector configurations
            using (var runner = new SimpleScriptRunner(Properties.Resources.Get_ADSyncConnectorsBasic))
            {
                runner.Parameters.Add("Name", Name);
                runner.Run();
                if (runner.HadErrors)
                {
                    var err = runner.LastError ?? new Exception("Encountered an error in PowerShell but could not capture the exception.");
                    throw err;
                }
                // Map PowerShell objects to models, C# classes with matching property names.
                // All results should be AadcConnector but CapturePSResult can return as
                // type: dynamic if the PowerShell object doesn't successfully map to the
                // desired model type. For those that are the correct model, we pass them
                // to the IsAuthorized method which loads and runs any rules for this connector
                // who match the requestors roles.
                var dynResults = runner.Results.CapturePSResult<AadcConnector>();
                var resultValues = dynResults
                    .Where(x => x is AadcConnector)     // Filter out results that couldn't be captured as AadcConnector.
                    .Select(x => x as AadcConnector)    // Take as AadcConnector so typed call to WhereAuthorized avoids GetType() call.
                    .ToArray();   

                return resultValues;
            }
        }

        public dynamic GetADSyncConnectorStatistics(string Name)
        {
            // Run PowerShell command to get AADC connector configurations
            using (var runner = new SimpleScriptRunner("param($ConnectorName) Get-ADSyncConnectorStatistics -ConnectorName $ConnectorName"))
            {
                runner.Parameters.Add("ConnectorName", Name);
                runner.Run();
                if (runner.HadErrors)
                {
                    var err = runner.LastError ?? new Exception("Encountered an error in PowerShell but could not capture the exception.");
                    throw err;
                }
                var result = runner.Results.ToDict().FirstOrDefault();
                return result;
            }
        }

        public Dictionary<string, object> GetADSyncAADCompanyFeature()
        {
            using (var runner = new SimpleScriptRunner("Get-ADSyncAADCompanyFeature"))
            {
                runner.Run();
                if (runner.HadErrors)
                {
                    var err = runner.LastError ?? new Exception("Encountered an error in PowerShell but could not capture the exception.");
                    throw err;
                }
                var result = runner.Results.ToDict().FirstOrDefault();
                return result;
            }
        }

        public Dictionary<string, object> GetADSyncAADPasswordResetConfiguration()
        {
            using (var runner = new SimpleScriptRunner(Properties.Resources.Get_ADSyncAADPasswordResetConfiguration))
            {
                runner.Run();
                if (runner.HadErrors)
                {
                    var err = runner.LastError ?? new Exception("Encountered an error in PowerShell but could not capture the exception.");
                    throw err;
                }
                var result = runner.Results.ToDict().FirstOrDefault();
                return result;
            }
        }

        public Dictionary<string, object> GetADSyncAADPasswordSyncConfiguration(string SourceConnector)
        {
            using (var runner = new SimpleScriptRunner(Properties.Resources.Get_ADSyncAADPasswordSyncConfiguration))
            {
                runner.Parameters.Add("SourceConnector", SourceConnector.Trim());
                runner.Run();
                if (runner.HadErrors)
                {
                    var err = runner.LastError ?? new Exception("Encountered an error in PowerShell but could not capture the exception.");
                    throw err;
                }
                var result = runner.Results.ToDict().FirstOrDefault();
                return result;
            }
        }

        public Dictionary<string, object> GetADSyncAutoUpgrade()
        {
            using (var runner = new SimpleScriptRunner(Properties.Resources.Get_ADSyncAutoUpgrade))
            {
                runner.Run();
                if (runner.HadErrors)
                {
                    var err = runner.LastError ?? new Exception("Encountered an error in PowerShell but could not capture the exception.");
                    throw err;
                }
                var result = runner.Results.ToDict().FirstOrDefault();
                return result;
            }
        }

        public AadcCSObject GetADSyncCSObjectStrict(string ConnectorName, string DistinguishedName)
        {
            using (var runner = new SimpleScriptRunner(Properties.Resources.Get_ADSyncCSObjectStrict))
            {
                runner.Parameters.Add("ConnectorName", ConnectorName);
                runner.Parameters.Add("DistinguishedName", DistinguishedName);
                runner.Run();
                if (runner.HadErrors)
                {
                    var err = runner.LastError ?? new Exception("Encountered an error in PowerShell but could not capture the exception.");
                    throw err;
                }
                var resultValues = runner.Results.CapturePSResult<AadcCSObject>()
                    .Where(x => x is AadcCSObject)
                    .Select(x => x as AadcCSObject);

                var result = resultValues.FirstOrDefault();
                return result;
            }
        }

        public DomainReachabilityStatus GetADSyncDomainReachabilityStatus(string ConnectorName)
        {
            
            // Run PowerShell command to get AADC connector configurations
            using (var runner = new SimpleScriptRunner(Properties.Resources.Get_ADSyncDomainReachabilityStatus))
            {
                runner.Parameters.Add("ConnectorName", ConnectorName);
                runner.Run();
                if (runner.HadErrors)
                {
                    var err = runner.LastError ?? new Exception("Encountered an error in PowerShell but could not capture the exception.");
                    throw err;
                }
                // Map PowerShell objects to models, C# classes with matching property names.
                // All results should be DomainReachabilityStatus but CapturePSResult can return as
                // type: dynamic if the PowerShell object doesn't successfully map to the
                // desired model type. For those that are the correct model, we pass them
                // to the IsAuthorized method which loads and runs any rules for this connector
                // who match the requestors roles.
                var resultValues = runner.Results.CapturePSResult<DomainReachabilityStatus>()
                    .Where(x => x is DomainReachabilityStatus)     // Filter out results that couldn't be captured as DomainReachabilityStatus.
                    .Select(x => x as DomainReachabilityStatus);   // Take as DomainReachabilityStatus so typed call to WhereAuthorized avoids GetType() call.

                if (resultValues != null)
                {
                    var result = resultValues.FirstOrDefault();
                    return result;
                }
            }
            return null;
        }

        public Dictionary<string, object> GetADSyncExportDeletionThreshold()
        {
            using (var runner = new SimpleScriptRunner("Get-ADSyncExportDeletionThreshold"))
            {
                runner.Run();
                if (runner.HadErrors)
                {
                    var err = runner.LastError ?? new Exception("Encountered an error in PowerShell but could not capture the exception.");
                    throw err;
                }
                var result = runner.Results.ToDict().FirstOrDefault();
                return result;
            }
        }

        public Dictionary<string, object> GetAdSyncGlobalSettingsStrict()
        {
            using (var runner = new SimpleScriptRunner(Properties.Resources.Get_AdSyncGlobalSettingsStrict))
            {
                runner.Run();
                if (runner.HadErrors)
                {
                    var err = runner.LastError ?? new Exception("Encountered an error in PowerShell but could not capture the exception.");
                    throw err;
                }
                var result = runner.Results.ToDict().FirstOrDefault();
                return result;
            }
        }

        public Dictionary<string, object> GetADSyncMVObjectStrict(string Id)
        {
            using (var runner = new SimpleScriptRunner(Properties.Resources.Get_ADSyncMVObjectStrict))
            {
                runner.Parameters.Add("Identifier", Id);
                runner.Run();
                if (runner.HadErrors)
                {
                    var err = runner.LastError ?? new Exception("Encountered an error in PowerShell but could not capture the exception.");
                    throw err;
                }
                var resultValues = runner.Results.ToDict().FirstOrDefault();
                return resultValues;
            }
        }

        public IEnumerable<PasswordSyncState> GetADSyncPartitionPasswordSyncState()
        {
            // Filter out the 'default' connector before yielding any results.
            using (var runner = new SimpleScriptRunner("Import-Module ADSync; Get-ADSyncPartitionPasswordSyncState | ? DN -notlike 'default'"))
            {
                runner.Run();
                if (runner.HadErrors)
                {
                    var err = runner.LastError ?? new Exception("Encountered an error in PowerShell but could not capture the exception.");
                    throw err;
                }
                // See the ConnectorController for more details on how the filtering is suppsoed to work.
                var resultValues = runner.Results.CapturePSResult<PasswordSyncState>()
                    .Where(x => x is PasswordSyncState)
                    .Select(x => x as PasswordSyncState);

                // Without this, the enum value is stored as an integer. It's a hack
                // but still worth the flexibility of using dynamic types on the model.
                foreach (var r in resultValues)
                    r.PasswordSyncLastCycleStatus = r.PasswordSyncLastCycleStatus?.ToString() ?? "Unknown";

                return resultValues;
            }
        }

        public IEnumerable<Dictionary<string, object>> GetADSyncRunProfileLastHour(Guid? RunHistoryId = null, Guid? ConnectorId = null, int NumberRequested = 0, bool RunStepDetails = false)
        {
            // Run PowerShell command to get AADC connector configurations
            using (var runner = new SimpleScriptRunner(Properties.Resources.Get_ADSyncRunProfileLastHour))
            {
                if (RunHistoryId != null)
                    runner.Parameters.Add("Id", RunHistoryId);

                if (ConnectorId != null)
                    runner.Parameters.Add("ConnectorId", ConnectorId);

                if (NumberRequested != 0)
                    runner.Parameters.Add("NumberRequested", NumberRequested);

                runner.Parameters.Add("RunStepDetails", RunStepDetails);
                runner.Run();
                if (runner.HadErrors)
                {
                    var err = runner.LastError ?? new Exception("Encountered an error in PowerShell but could not capture the exception.");
                    throw err;
                }
                // Map PowerShell objects to Dictionary<string,object> as a generic
                // box type for PSObject. There is no real type safety but it serializes
                // to JSON rather well which is the goal for a web api and allows
                // for PowerShell models to change without requiring changes to this application.
                var resultValues = runner.Results.ToDict()?.ToList();
                return resultValues;
            }
        }

        public dynamic GetADSyncSchedulerConnectorOverride(string ConnectorName = null)
        {
            if (string.IsNullOrEmpty(ConnectorName))
            {
                return new SchedulerOverride();
            }

            using (var runner = new SimpleScriptRunner(Properties.Resources.Get_ADSyncSchedulerConnectorOverride))
            {
                runner.Parameters.Add("ConnectorName", ConnectorName);
                runner.Run();
                if (runner.HadErrors)
                {
                    var err = runner.LastError ?? new Exception("Encountered an error in PowerShell but could not capture the exception.");
                    throw err;
                }
                var resultValues = runner.Results?.CapturePSResult<SchedulerOverrideResult>()?.FirstOrDefault();
                return resultValues;
            }
        }

        public dynamic SetADSyncSchedulerConnectorOverride(SchedulerOverride OverrideConfig)
        {
            string ConnectorName = OverrideConfig.ConnectorName;
            if (string.IsNullOrEmpty(ConnectorName))
            {
                throw new Exception("Bad request. Must specify ConnectorName");
            }

            using (var runner = new SimpleScriptRunner(Properties.Resources.Set_ADSyncSchedulerConnectorOverride))
            {
                runner.Parameters.Add("ConnectorName", OverrideConfig.ConnectorName);
                runner.Parameters.Add("FullSyncRequired", OverrideConfig.FullSyncRequired);
                runner.Parameters.Add("FullImportRequired", OverrideConfig.FullImportRequired);
                runner.Run();
                runner.Results.ToDict();

                var resultValues = runner.Results.CapturePSResult<SchedulerOverrideResult>();
                return resultValues;
            }
        }

        public Dictionary<string, object> GetADSyncScheduler()
        {
            using (var runner = new SimpleScriptRunner("Import-Module ADSync; Get-ADSyncScheduler"))
            {
                runner.Run();

                if (runner.HadErrors)
                {
                    var err = runner.LastError ?? new Exception("Encountered an error in PowerShell but could not capture the exception.");
                    throw err;
                }

                var result = runner.Results.ToDict()?.FirstOrDefault();
                return result;
            }
        }


        static Regex validSyncPolicyTypes = new Regex(@"(?i)(Unspecified|Delta|Initial)");
        public dynamic SetADSyncScheduler(SyncScheduleSettings settings)
        {
            var parameters = new Dictionary<string, object>();
            if (null != settings.SyncCycleEnabled) { parameters.Add(nameof(settings.SyncCycleEnabled), settings.SyncCycleEnabled); }
            if (null != settings.SchedulerSuspended) { parameters.Add(nameof(settings.SchedulerSuspended), settings.SchedulerSuspended); }
            if (null != settings.MaintenanceEnabled) { parameters.Add(nameof(settings.MaintenanceEnabled), settings.MaintenanceEnabled); }
            if (null != settings.NextSyncCyclePolicyType) { parameters.Add(nameof(settings.NextSyncCyclePolicyType), settings.NextSyncCyclePolicyType); }

            using (var runner = new SimpleScriptRunner(@"
[CmdletBinding()]
param(
    [bool]$SyncCycleEnabled,
    [bool]$SchedulerSuspended,
    [bool]$MaintenanceEnabled,
    [string]$NextSyncCyclePolicyType
)

Set-AdSyncScheduler @PSBoundParameters
"))
            {
                runner.Parameters = parameters;
                runner.Run();

                if (runner.HadErrors)
                {
                    var err = runner.LastError ?? new Exception("Encountered an error in PowerShell but could not capture the exception.");
                    throw err;
                }

                var result = runner.Results.ToDict().FirstOrDefault();
                if (null == result || result.Count == 0)
                {
                    return null;
                }
                return result;
            }
        }

        public IEnumerable<object> GetADSyncRule(string Identifier = null)
        {
            // Run PowerShell command to get AADC sync rules
            using (var runner = new SimpleScriptRunner(@"
[CmdletBinding()]
param([Guid]$Identifier)

Get-ADSyncRule @PSBoundParameters
"))
            {
                if (Identifier != null)
                {
                    runner.Parameters.Add("Identifier", Identifier);
                }
                runner.Run();
                if (runner.HadErrors)
                {
                    var err = runner.LastError ?? new Exception("Encountered an error in PowerShell but could not capture the exception.");
                    throw err;
                }
                var resultValues = runner.Results.CapturePSResult<SyncRule>()
                    .Where(x => x is SyncRule)
                    .Cast<SyncRule>();

                if (resultValues != null)
                {
                    return resultValues;
                }
            }

            return null;
        }
    }
}
