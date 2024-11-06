using Laparoscope.Controllers.Server;
using LaparoscopeShared.Models;
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
        public IEnumerable<RoleFilterModel> AuthorizationRules { get ; set; } = Array.Empty<RoleFilterModel>();

        public IEnumerable<WindowsTask> Processes { get; set; }

        public async void OnGet()
        {
            AuthorizationRules = RegisteredRoleControllerRules.GetRoleControllerModels();
            Processes = await ProcessesController.GetProcesses();
        }
    }
}
