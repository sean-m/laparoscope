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
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        // GET: Admin
        public ActionResult Index()
        {
            ViewBag.GlobalProps = typeof(aadcapi.Utils.Globals).GetProperties().OrderBy(x => x.Name);

            ViewBag.AppSettings = ConfigurationManager.AppSettings;
            ViewBag.SettingKeys = ConfigurationManager.AppSettings.AllKeys.OrderBy(x => x);
            ViewBag.AuthorizationRules = RegisteredRoleControllerRules.GetRoleControllerModels();
            return View();
        }
    }
}