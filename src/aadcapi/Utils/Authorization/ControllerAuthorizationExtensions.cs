using SMM.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;
using System.Web.Http;

namespace aadcapi.Utils.Authorization
{
    /// <summary>
    /// Methods to assist in processing authorization rules from within a controller.
    /// </summary>
    public static class ControllerAuthorizationExtensions
    {

        /// <summary>
        /// Helper for returning the name of a given controller. Helpful when calling
        /// an authorization routine that loads rules based on controller name.
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        public static string ControllerName(this ApiController conn)
        {
            return conn.ControllerContext.RouteData.Values["controller"].ToString();
        }

        /// <summary>
        /// Yields a collection of role values for a given ClaimsPrincipal.
        /// </summary>
        /// <param name="Principal"></param>
        /// <returns></returns>
        public static IEnumerable<string> RoleClaims(this ClaimsPrincipal Principal)
        {
            return Principal.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value);
        }

        /// <summary>
        /// Given a controller, filter a collection of objects that the principal can access in that context.
        /// Simply a helper function so filtering a collection from within a controller can be simplified.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Controller"></param>
        /// <param name="Collection"></param>
        /// <returns></returns>
        public static IEnumerable<T> WhereAuthorized<T>(this ApiController Controller, IEnumerable<T> Collection)
        {
            var context = Controller.ControllerName();
            var roles = ((ClaimsPrincipal)Controller?.RequestContext?.Principal)?.RoleClaims();
            if (roles != null)
            {
                // TODO LOGME (Sean) log null role set
            }

            return Collection.Where(x => IsAuthorized<T>(x, context, roles));
        }

        public static bool IsAuthorized<T>(T Model, string Context, IEnumerable<string> Roles)
        {
            var rules = RegisteredRoleControllerRules.GetRoleControllerModelsByContext(Context);
            var connType = typeof(T);

            return rules.Where(
                    rule => {
                        bool result = (bool)Roles?.Any(
                            role => String.Equals(role, rule.Role, StringComparison.CurrentCultureIgnoreCase)); // short circuits when a matching rule is found.
                                                                                                                // Returns false if no roles match those defined
                                                                                                                // in rules for the specified context.
                        if (!result)
                        {
                            // TODO LOGME (Sean) $"Searching for rules for the given context: {Context} and roles: {String.Join(', ', Roles)} yields no results"
                        }
                        return result;
                    }).Any(rule => {
                        var testValue = connType.GetProperty(rule.ModelProperty)?.GetValue(Model).ToString();
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
                        if (rule.ModelValues != null && rule.ModelValues.Count() > 0)
                        {
                            return rule.ModelValues.Any(pattern => testValue.Like(pattern));   // Uses Like extension method that supports wildcards
                                                                                               // so you could say role: GarageAdmins could see all
                                                                                               // connectors that match: garage.* .
                        }
                        else
                        {
                            var pattern = rule.ModelValue;
                            return testValue.Like(pattern);
                        }
                    });
        }
    }
}