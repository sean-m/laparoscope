using SMM.Automation;
using SMM.Helper;
using aadcapi.Utils.Authorization;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;

namespace aadcapi.Controllers.ServerApi
{
    /// <summary>
    /// Unqualified GET returns sync history for the last hour. Results are filtered
    /// to connectors you have rights to view in this context.
    /// </summary>
    [Authorize]
    public class RunProfileResultController : ApiController
    {
        /// <summary>
        /// Executes Get-ADSyncRunProfileResult and filters out entries older than an hour.
        /// </summary>
        /// <param name="RunHistoryId">For retreiving a specific run profile instance.</param>
        /// <param name="ConnectorId">The guid of a specific container. Can be retreived from: /api/Connector.</param>
        /// <param name="NumberRequested">Positive integer limiting the results. Note this limits results
        /// before authorization filtering. The returned set may be less than you specify if your query
        /// was not scoped to a connector you have rights to. Default: 0, no limit.</param>
        /// <param name="RunStepDetails">Indicates whether you'd like detailed error information included. Default: false.</param>
        /// <returns>
        /// Example:
        ///   RunHistoryId      : 8a654dc1-cf23-40ed-86f4-b745bd553c22
        ///   ConnectorId       : fffb4a69-4ed6-444d-8b89-73bc16f373dd
        ///   ConnectorName     : garage.mcardletech.com
        ///   RunProfileId      : e73df090-c632-4a61-961a-87ae047570c8
        ///   RunProfileName    : Export
        ///   RunNumber         : 26285
        ///   Username          : NT SERVICE\ADSync
        ///   IsRunComplete     : True
        ///   Result            : no-start-connection
        ///   CurrentStepNumber : 1
        ///   TotalSteps        : 1
        ///   StartDate         : 2022-04-08T20:13:31.163
        ///   EndDate           : 2022-04-08T20:13:52.187
        ///   RunStepResults    : 
        /// </returns>
        [ResponseType(typeof(IEnumerable<Dictionary<string,object>>))]
        public IEnumerable<Dictionary<string, object>> Get(Guid? RunHistoryId=null, Guid? ConnectorId=null, int NumberRequested=0, bool RunStepDetails=false)
        {
            // Run PowerShell command to get AADC connector configurations
            using (var runner = new SimpleScriptRunner(aadcapi.Properties.Resources.Get_ADSyncRunProfileLastHour))
            {
                if (RunHistoryId != null)
                    runner.Parameters.Add("Id", RunHistoryId);

                if (ConnectorId != null)
                    runner.Parameters.Add("ConnectorId", ConnectorId);

                if (NumberRequested != 0)
                    runner.Parameters.Add("NumberRequested", NumberRequested);

                runner.Parameters.Add("RunStepDetails", RunStepDetails);
                runner.Run();

                // Map PowerShell objects to Dictionary<string,object> as a generic
                // box type for PSObject. There is no real type safety but it serializes
                // to JSON rather well which is the goal for a web api and allows
                // for PowerShell models to change without requiring changes to this application.
                var resultValues = runner.Results.ToDict();
                var resultList = this.WhereAuthorized(resultValues);
                return resultList;
            }
        }
    }
}