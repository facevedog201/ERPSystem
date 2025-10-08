using Microsoft.AspNetCore.Mvc;

namespace ERPSystem.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
