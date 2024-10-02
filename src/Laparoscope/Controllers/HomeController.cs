using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Laparoscope.Controllers
{   
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "AADC API";

            return View();
        }
    }
}
