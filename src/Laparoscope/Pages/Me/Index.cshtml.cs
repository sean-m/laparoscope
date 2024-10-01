using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace Laparoscope.Pages.Me
{
    public class IndexModel : PageModel
    {
        ClaimsPrincipal User { get; set; }

        public void OnGet()
        {
            User = PageContext.HttpContext.User;
        }
    }
}
