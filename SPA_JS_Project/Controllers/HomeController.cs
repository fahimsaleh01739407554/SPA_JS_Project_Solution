using Microsoft.AspNetCore.Mvc;
using SPA_JS_Project.Models;

namespace SPA_JS_Project.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
