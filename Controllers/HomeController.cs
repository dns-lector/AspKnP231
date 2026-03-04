using System.Diagnostics;
using AspKnP231.Models;
using Microsoft.AspNetCore.Mvc;

namespace AspKnP231.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Intro()
        {
            return View();
        }

        public IActionResult UnLayout()
        {
            return View();
        }
        
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
