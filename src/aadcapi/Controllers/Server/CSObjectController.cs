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
        /// <summary>
        /// Controller for the Get-ADSyncCSObject command. This is used to retreive a single record
        /// from a named connector space. Records are queried by DN only and must specify a valid connector
        /// name. The returned record contains a ConnectedMVObjectId property that can be used to inspect
        /// the corresponding metaverse record.
        /// 
        /// Note: records filtered on import may not resolve or may only exist in the connector space and
        /// will not have a ConnectedMVObjectId property.
        /// </summary>
        /// <param name="ConnectorName">
        /// Name of the connector space to query. Can be retreived from /api/Connector.
        /// </param>
        /// <param name="DistinguishedName">
        /// The full distinguished name for the desired connector space record. If querying an
        /// Active Directory connector it may look like this: CN=Super Great Person,CN=Cool Stuff,DC=my,DC=garage.
        /// </param>
        /// <returns>
        /// https://docs.microsoft.com/en-us/azure/active-directory/hybrid/reference-connect-adsync#get-adsynccsobject
        /// </returns>
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