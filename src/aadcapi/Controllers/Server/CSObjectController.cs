using aadcapi.Models;
using aadcapi.Utils.Authorization;
using SMM.Automation;
using SMM.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace aadcapi.Controllers.Server
{
    [Authorize]
    public class CSObjectController : ApiController
    {
        // TODO (Sean) Document
        // GET: CSObject
        // Get-ADSyncCSObject *
        public dynamic Get(string ConnectorName, string DistinguishedName)
        {
            var runner = new SimpleScriptRunner(aadcapi.Properties.Resources.Get_ADSyncCSObjectStrict);
            runner.Parameters.Add("ConnectorName", ConnectorName);
            runner.Parameters.Add("DistinguishedName", DistinguishedName);
            runner.Run();

            var resultValues = runner.Results.CapturePSResult<AadcCSObject>()
                .Where(x => x is AadcCSObject)
                .Select(x => x as AadcCSObject);

            resultValues = this.WhereAuthorized<AadcCSObject>(resultValues);

            return Ok(resultValues);
        }
    }
}