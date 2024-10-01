using McAuthorization;
using McAuthorization.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace Laparoscope.Pages.Admin
{
    public class IndexModel : PageModel
    {

        //IEnumerable<object> GlobalProps = typeof(aadcapi.Utils.Globals).GetProperties().OrderBy(x => x.Name);

        //ViewBag.AppSettings = ConfigurationManager.AppSettings;
        //ViewBag.SettingKeys = ConfigurationManager.AppSettings.AllKeys.OrderBy(x => x);
        internal IEnumerable<RoleFilterModel> AuthorizationRules { get ; set; }

        public void OnGet()
        {
            AuthorizationRules = RegisteredRoleControllerRules.GetRoleControllerModels();
        }
    }
}
