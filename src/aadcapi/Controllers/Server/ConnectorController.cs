﻿using aadcapi.Models;
using aadcapi.Utils;
using SMM.Automation;
using SMM.Helper;
using System.Collections.Generic;
using System.Web.Http;
using System.Linq;

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

            // Map PowerShell objects to models, C# classes with matching property names
            var resultValues = runner.Results.CapturePSResult<AadcConnector>().Where(
                   x => { 
                       // All results should be AadcConnector but CapturePSResult can return as
                       // type: dynamic if the PowerShell object doesn't successfully map to the
                       // desired model type. For those that are the correct model, we pass them
                       // to the IsAuthorized method which loads and runs any rules for this connector
                       // who match the requestors roles.
                       if (x is AadcConnector connector) {
                            return IsAuthorized<AadcConnector>(connector, this.ControllerName());
                       }
                       return false;
                   });  // The Where() will filter out any object 'x' where the => {} function
                        // returns false. That would include any that couldn't be mapped to the
                        // model and any whose IsAuthorized call returns false.

            if (resultValues != null)
            {
                var result = Ok(resultValues);
                return result;
            }

            return Ok();
        }

        private bool IsAuthorized<T>(T Model, string Controller)
        {
            var rules = RegisteredRoleControllerRules.GetRoleControllerModelsByController(Controller);
            var connType = typeof(T);

            return rules.Any(r =>
            {
                var roleMatch = RequestContext.Principal.IsInRole(r.Role);
                if (roleMatch)
                {
                    var testValue = connType.GetProperty(r.ModelProperty)?.GetValue(Model).ToString();
                    // bail out if there's no property with that name on the value
                    if (testValue == null) return false;

                    /*
                    * Check if there are any model values that match the specified property on the model
                    * matches one of the specified values. For AADC controllers it would be obvious
                    * to check if there is a rule that specifies a role that should be allowd to access
                    * connectors with a given name. The rule would for admins accessing my lab connector
                    * would look like this:
                    *   Role: "Admin"
                    *   ModelProperty: "Name"
                    *   ModelValue: "garage.mcardletech.com"  // actual example used in my lab not shilling my website, 
                    *                                         // it's truly not worth your time.
                    */
                    if (r.ModelValues != null && r.ModelValues.Count() > 0)
                    {
                        return r.ModelValues.Any(pattern => testValue.Like(pattern));   // Uses Like extension method that supports wildcards
                                                                                        // so you could say role: GarageAdmins could see all
                                                                                        // connectors that match: garage.* .
                    }
                    else
                    {
                        var pattern = r.ModelValue;
                        return testValue.Like(pattern);
                    }
                }
                return false;  // If there's no matching rule, assume they shouldn't see it.
            });
        }
    }
}