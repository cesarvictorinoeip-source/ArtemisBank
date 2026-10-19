using Microsoft.AspNetCore.Mvc;

namespace ArtemisBank.WebApp.Controllers
{
    public class AccessDeniedController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}