using DataAccessLayer.Interfaces;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KE03_INTDEV_SE_1_Base.Pages
{
    public class IndexModel : PageModel
    {
        

        public IActionResult OnGet()
        {
            HttpContext.Session.SetString("isLoggedIn", "False");
            return RedirectToPage("/LoginPage");
        }
    }
}
