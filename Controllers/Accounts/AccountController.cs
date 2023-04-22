using Microsoft.AspNetCore.Mvc;

namespace BHP.Controllers.Accounts
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AccessDenied()
        {
            return View();
        }


        public IActionResult Login(string id)
        {
            return RedirectToAction("AccessDenied", "Account");
        }


        public IActionResult ExpiredSession()
        {
            return View();
        }
    }
}