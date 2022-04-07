using McAuthorization;
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

            return Collection.Where(x => Filter.IsAuthorized<T>(x, context, roles));
        }

        /// <summary>
        /// IsAuthorized extension method for evaluating rules against a single object
        /// from within a controller method.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Controller"></param>
        /// <param name="Model"></param>
        /// <returns></returns>
        public static bool IsAuthorized<T>(this ApiController Controller, T Model)
        {
            var context = Controller.ControllerName();
            var roles = ((ClaimsPrincipal)Controller?.RequestContext?.Principal)?.RoleClaims();
            if (roles != null)
            {
                // TODO LOGME (Sean) log null role set
            }

            return Filter.IsAuthorized<T>(Model, context, roles);
        }

        /// <summary>
        /// Given a controller, filter a collection of objects that the principal can access in that context.
        /// Simply a helper function so filtering a collection from within a controller can be simplified.
        /// </summary>
        /// <param name="Controller"></param>
        /// <param name="Collection"></param>
        /// <returns></returns>
        public static IEnumerable<Dictionary<string, object>> WhereAuthorized(this ApiController Controller, IEnumerable<Dictionary<string, object>> Collection)
        {
            var context = Controller.ControllerName();
            var roles = ((ClaimsPrincipal)Controller?.RequestContext?.Principal)?.RoleClaims();
            if (roles != null)
            {
                // TODO LOGME (Sean) log null role set
            }

            return Collection?.Where(x => Filter.IsAuthorized(x, context, roles));
        }

        /// <summary>
        /// IsAuthorized extension method for evaluating rules against a single object
        /// from within a controller method.
        /// </summary>
        /// <param name="Controller"></param>
        /// <param name="Model"></param>
        /// <returns></returns>
        public static bool IsAuthorized(this ApiController Controller, Dictionary<string, object> Model)
        {
            var context = Controller.ControllerName();
            var roles = ((ClaimsPrincipal)Controller?.RequestContext?.Principal)?.RoleClaims();
            if (roles != null)
            {
                // TODO LOGME (Sean) log null role set
            }

            return Filter.IsAuthorized(Model, context, roles);
        }
    }
}