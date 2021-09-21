using SMM.Automation;
using SMM.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;

namespace aadcapi.Controllers.Server
{
    [Authorize]
    public class MVObjectController : ApiController
    {
        // TODO (Sean) Document
        // GET api/<controller>
        // Get-ADSyncMVObject
        public dynamic Get(string Id)
        {
            var runner = new SimpleScriptRunner(aadcapi.Properties.Resources.Get_ADSyncMVObjectStrict);
            runner.Parameters.Add("Identifier", Id);
            runner.Run();

            var resultValues = runner.Results.ToDict();

            return Ok(resultValues);
        }
    }
}