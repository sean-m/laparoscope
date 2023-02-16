using SMM.Automation;
using SMM.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;

namespace aadcapi.Controllers.Server
{
    [Authorize]
    public class MVObjectController : ApiController
    {
        /// <summary>
        /// Controller for the Get-ADSyncMVObject command. Records are only retrieved by ObjectID
        /// which can only be discovered from an unbounded metaverse search (not allowed by this
        /// api) or by cross walking from the connector space.
        /// 
        /// Note: if validating a merged
        /// identity (one that exists in multiple source connector spaces), inspect the 'Lineage'
        /// property for all connectors joined to the metaverse record.
        /// </summary>
        /// <param name="Id">Guid identifying a single metaverse record.</param>
        /// <returns>
        /// https://docs.microsoft.com/en-us/azure/active-directory/hybrid/reference-connect-adsync#get-adsyncmvobject
        /// </returns>
        // GET api/<controller>
        [ResponseType(typeof(Dictionary<string, object>))]
        public dynamic Get(string Id)
        {
            // TODO require an MVObject role
            using (var runner = new SimpleScriptRunner(aadcapi.Properties.Resources.Get_ADSyncMVObjectStrict))
            {
                runner.Parameters.Add("Identifier", Id);
                runner.Run();

                if (runner.HadErrors)
                {
                    var err = runner.LastError ?? new Exception("Encountered an error in PowerShell but could not capture the exception.");
                    return InternalServerError(err);
                }

                var resultValues = runner.Results.ToDict();

                return Ok(resultValues);
            }
        }
    }
}