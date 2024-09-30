using aadcapi.Utils.Authorization;
using McAuthorization;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace aadcapi.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        // GET: Admin
        public ActionResult Index()
        {
            // TODO fix all this
            //ViewBag.GlobalProps = typeof(aadcapi.Utils.Globals).GetProperties().OrderBy(x => x.Name);

            //ViewBag.AppSettings = ConfigurationManager.AppSettings;
            //ViewBag.SettingKeys = ConfigurationManager.AppSettings.AllKeys.OrderBy(x => x);
            ViewBag.AuthorizationRules = RegisteredRoleControllerRules.GetRoleControllerModels();
            return View();
        }
    }
}