using aadcapi.Utils.Authorization;
using McAuthorization;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace aadcapi.Controllers
{
    [Authorize]
    public class ServerController : Controller
    {
        // GET: Admin
        public ActionResult Index()
        {
            ViewBag.CanPause = McAuthorization.Filter.IsAuthorized<dynamic>
                (new { Name = "Scheduler", Setting = "SchedulerSuspended" }, "Scheduler", (ClaimsPrincipal)User);
            return View();
        }
    }
}