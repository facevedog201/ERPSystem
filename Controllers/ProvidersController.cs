using ERPSystem.Data;
using ERPSystem.Models;
using ERPSystem.Services;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace ERPSystem.Controllers
{
    public class ProvidersController : Controller
    {
        private readonly AppDbContext _context;
        private readonly AuditService _auditService;


        public ProvidersController(AppDbContext context)
        {
            _context = context;
        }

        // Listar proveedores
        public IActionResult Index()
        {
            var providers = _context.Providers.ToList();
            return View(providers);
        }

        // Crear proveedor (GET)
        public IActionResult Create()
        {
            return View();
        }

        // Crear proveedor (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Provider provider)
        {
            if (ModelState.IsValid)
            {
                _context.Providers.Add(provider);
                _context.SaveChanges();
                _auditService.Log("Create", "Provider", provider.ProviderId, $"Se creó el Proveedor {provider.ProviderId}");
                return RedirectToAction(nameof(Index));
            }
            return View(provider);
        }

        // Editar proveedor (GET)
        public IActionResult Edit(int id)
        {
            var provider = _context.Providers.Find(id);
            if (provider == null) return NotFound();
            return View(provider);
        }

        // Editar proveedor (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Provider provider)
        {
            if (ModelState.IsValid)
            {
                _context.Providers.Update(provider);
                _context.SaveChanges();
                _auditService.Log("Edit", "Provider", provider.ProviderId, $"Se creó el Proveedor {provider.ProviderId}");

                return RedirectToAction(nameof(Index));
            }
            return View(provider);
        }

        // Eliminar proveedor (GET)
        public IActionResult Delete(int id)
        {
            var provider = _context.Providers.Find(id);
            if (provider == null) return NotFound();
            return View(provider);
        }

        // Eliminar proveedor (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var provider = _context.Providers.Find(id);
            if (provider == null) return NotFound();

            _context.Providers.Remove(provider);
            _context.SaveChanges();
            _auditService.Log("Delete", "Provider", provider.ProviderId, $"Se creó el Proveedor {provider.ProviderId}");
            return RedirectToAction(nameof(Index));
        }
    }
}
