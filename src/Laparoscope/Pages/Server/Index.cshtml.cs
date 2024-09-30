using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Client;
using System.Security.Claims;

namespace Laparoscope.Pages.Server
{
    public class IndexModel : PageModel
    {
        public bool CanPause { get; set; } = false;
        public void OnGet()
        {
            CanPause = McAuthorization.Filter.IsAuthorized<dynamic>
                 (new { Name = "Scheduler", Setting = "SchedulerSuspended" }, "Scheduler", (ClaimsPrincipal)User);
        }
    }
}
