using aadcapi.Models;
using aadcapi.Utils;
using SMM.Automation;
using SMM.Helper;
using System.Collections.Generic;
using System.Web.Http;
using System.Linq;
using System.Security.Claims;
using aadcapi.Utils.Authorization;
using System.Web.Http.Description;

namespace aadcapi.Controllers.Server
{
    [Authorize]
    public class SyncRuleController : ApiController
    {
        // GET: Connectors
        /// <summary>
        /// Executes Get-ADSyncRule and returns sync rules the requestor is authorized to see.
        /// Can optionally take the identifier of a specific sync rule. Note: if an identifier 
        /// is specified and no matching authorization rule exists this does not result in a 403
        /// as authorization rules act as filters. The result is an empty set.
        /// </summary>
        /// <param name="Identifier">Guid identifier of a specific sync rule.</param>
        /// <returns></returns>
        [ResponseType(typeof(IEnumerable<object>))]
        public dynamic Get(string Identifier=null)
        {
            // Run PowerShell command to get AADC sync rules
            using (var runner = new SimpleScriptRunner(@"
[CmdletBinding()]
param([Guid]$Identifier)

Get-ADSyncRule @PSBoundParameters
"))
            {
                if (Identifier != null) {
                    runner.Parameters.Add("Identifier", Identifier);
                }
                runner.Run();
                var resultValues = runner.Results.CapturePSResult<SyncRule>()
                    .Where(x => x is SyncRule)
                    .Cast<SyncRule>();

                resultValues = this.WhereAuthorized<SyncRule>(resultValues);

                if (resultValues != null)
                {
                    var result = Ok(resultValues);
                    return result;
                }
            }

            return Ok();
        }
    }
}