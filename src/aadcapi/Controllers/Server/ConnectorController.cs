using aadcapi.Models;
using aadcapi.Utils;
using SMM.Automation;
using SMM.Helper;
using System.Collections.Generic;
using System.Web.Http;
using System.Linq;
using System.Security.Claims;
using aadcapi.Utils.Authorization;

namespace aadcapi.Controllers.Server
{
    /// <summary>
    /// Unqualified GET returns a subset of information for connectors. With multiple connectors
    /// the result can be 100MB+ so some properties are left out.
    /// </summary>
    //[Authorize]
    public class ConnectorController : ApiController
    {
        // GET: Connectors
        /// <summary>
        /// Executes Get-ADSyncConnector and returns a subset of the properties. This will help
        /// to map a connector name to identifier for other cmdlets that only take identifier.
        /// </summary>
        /// <param name="Name">Name of a specific connector to return.</param>
        /// <returns></returns>
        public dynamic Get(string Name=null)
        {   
            // Run PowerShell command to get AADC connector configurations
            var runner = new SimpleScriptRunner(aadcapi.Properties.Resources.Get_ADSyncConnectorsBasic);
            runner.Parameters.Add( "Name", Name );
            runner.Run();

            // Map PowerShell objects to models, C# classes with matching property names.
            // All results should be AadcConnector but CapturePSResult can return as
            // type: dynamic if the PowerShell object doesn't successfully map to the
            // desired model type. For those that are the correct model, we pass them
            // to the IsAuthorized method which loads and runs any rules for this connector
            // who match the requestors roles.
            var resultValues = runner.Results.CapturePSResult<AadcConnector>().Where(x => x is AadcConnector);
            resultValues = this.WhereAuthorized(resultValues);
            
            if (resultValues != null)
            {
                var result = Ok(resultValues);
                return result;
            }

            return Ok();
        }
    }
}