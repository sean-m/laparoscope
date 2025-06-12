using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Laparoscope.Controllers.Server
{
    [Route("api/[controller]")]
    [ApiController]
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    [Authorize(AuthenticationSchemes = Global.AuthSchemes)]
    public class MeController : Controller
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        /// <summary>
        /// Returns the ClaimsIdentity used by the server for authorization.
        /// Provided for debugging purposes.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public dynamic Get()
        {
            var json = JsonConvert.SerializeObject(
                HttpContext.User,
                new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }
                );
            if (json == null) { return "{}"; }

            return JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
        }
    }
}