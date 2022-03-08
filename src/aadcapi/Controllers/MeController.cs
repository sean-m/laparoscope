using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace aadcapi.Controllers
{
    [Authorize]
    public class MeController : Controller
    {
        // GET: Me
        public ActionResult Index()
        {
            ViewBag.UserId = (ClaimsIdentity) HttpContext.User.Identity;
            return View();
        }
    }

    [System.Web.Http.Authorize]
    public class MeApiController : System.Web.Http.ApiController
    {
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