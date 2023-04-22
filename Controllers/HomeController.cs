using Microsoft.AspNetCore.Mvc;
using BHP.Helpers;

namespace BHP.Controllers
{
   
    public class HomeController : Controller
    {
        GeneralClass generalClass = new GeneralClass();

        public IActionResult Index()
        {
            
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error(string message)
        {
            var msg = generalClass.Decrypt(message);

            ViewData["Message"] = msg;
            return View();
        }

        public IActionResult Errorr(string message)
        {
            var msg = generalClass.Decrypt(message);

            ViewData["Message"] = msg;
            return View();
        }

        public IActionResult Login()
        {
            return RedirectToAction("AccessDenied", "Account");
        }
    }
}
