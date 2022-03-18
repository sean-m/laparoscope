using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;

namespace aadcapi.Controllers.Server
{
    [Authorize]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class MeController : ApiController
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        /// <summary>
        /// Returns the ClaimsIdentity used by the server for authorization.
        /// Provided for debugging purposes.
        /// </summary>
        /// <returns></returns>
        [ResponseType(typeof(ClaimsIdentity))]
        public dynamic Get()
        {
            var json = JsonConvert.SerializeObject(
                (ClaimsIdentity)RequestContext.Principal.Identity,
                new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }
                );
            return Ok( JsonConvert.DeserializeObject<Dictionary<string, object>>(json));
        }
    }
}