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
}