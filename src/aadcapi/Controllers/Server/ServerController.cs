using aadcapi.Utils.Authorization;
using McAuthorization;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
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
            return View();
        }
    }
}