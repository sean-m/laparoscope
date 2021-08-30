using SMM.Automation;
using SMM.Helper;
using System.Web.Http;

namespace aadcapi.Controllers.Server
{
    /// <summary>
    /// Unqualified GET returns a subset of information for connectors. With multiple connectors
    /// the result can be 100MB+ so some properties are left out.
    /// </summary>
    [Authorize]
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
            var runner = new SimpleScriptRunner(aadcapi.Properties.Resources.Get_ADSyncConnectorsBasic);
            runner.Parameters.Add( "Name", Name );
            runner.Run();
            var result = Ok(runner.Results.ToDict());
            return result;
        }
    }
}