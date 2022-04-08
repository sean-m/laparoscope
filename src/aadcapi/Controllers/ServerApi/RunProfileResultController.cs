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
    [Authorize]
    public class RunProfileResultController : ApiController
    {
        [ResponseType(typeof(IEnumerable<Dictionary<string,object>>))]
        public IEnumerable<Dictionary<string, object>> Get(Guid? RunHistoryId=null, Guid? ConnectorId=null, int NumberRequested=0, bool RunStepDetails=false)
        {
            // Run PowerShell command to get AADC connector configurations
            var runner = new SimpleScriptRunner(aadcapi.Properties.Resources.Get_ADSyncRunProfileLastHour);
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