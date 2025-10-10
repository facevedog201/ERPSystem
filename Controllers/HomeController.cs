using DocumentFormat.OpenXml.InkML;
using ERPSystem.Data;
using ERPSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ERPSystem.Controllers
{
    [Authorize] // Solo usuarios autenticados pueden acceder a este controlador
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger,AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }
        public IActionResult Index()
        {
            // Datos dinámicos para cada módulo
            ViewBag.ActiveUsers = _context.Users.Count(u => u.IsActive);
            ViewBag.ActiveClients = _context.Clients.Count(c => c.IsActive);
            ViewBag.ActiveServices = _context.Services.Count(s => s.IsActive);
            ViewBag.ActiveProviders = _context.Providers.Count(p => p.IsActive);

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
