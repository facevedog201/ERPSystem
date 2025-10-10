//using ERPSystem.Data;
//using ERPSystem.Models;
//using ERPSystem.Services;
//using Microsoft.AspNetCore.Mvc;
//using System.Linq;

//namespace ERPSystem.Controllers
//{
//    public class ProvidersController : Controller
//    {
//        private readonly AppDbContext _context;
//        private readonly AuditService _auditService;


//        public ProvidersController(AppDbContext context, AuditService auditService)
//        {
//            _context = context;
//            _auditService = auditService;
//        }

//        // Listar proveedores
//        public IActionResult Index()
//        {
//            var providers = _context.Providers.ToList();
//            return View(providers);
//        }

//        // Crear proveedor (GET)
//        public IActionResult Create()
//        {
//            return View();
//        }

//        // Crear proveedor (POST)
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public IActionResult Create(Provider provider)
//        {
//            if (ModelState.IsValid)
//            {
//                _context.Providers.Add(provider);
//                _context.SaveChanges();
//                _auditService.Log("Create", "Provider", provider.ProviderId, $"Se creó el Proveedor {provider.ProviderId}");
//                return RedirectToAction(nameof(Index));
//            }
//            return View(provider);
//        }

//        // Editar proveedor (GET)
//        public IActionResult Edit(int id)
//        {
//            var provider = _context.Providers.Find(id);
//            if (provider == null) return NotFound();
//            return View(provider);
//        }

//        // Editar proveedor (POST)
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public IActionResult Edit(Provider provider)
//        {
//            if (ModelState.IsValid)
//            {
//                _context.Providers.Update(provider);
//                _context.SaveChanges();
//                _auditService.Log("Edit", "Provider", provider.ProviderId, $"Se creó el Proveedor {provider.ProviderId}");

//                return RedirectToAction(nameof(Index));
//            }
//            return View(provider);
//        }

//        // Eliminar proveedor (GET)
//        public IActionResult Delete(int id)
//        {
//            var provider = _context.Providers.Find(id);
//            if (provider == null) return NotFound();
//            return View(provider);
//        }

//        // Eliminar proveedor (POST)
//        [HttpPost, ActionName("Delete")]
//        [ValidateAntiForgeryToken]
//        public IActionResult DeleteConfirmed(int id)
//        {
//            var provider = _context.Providers.Find(id);
//            if (provider == null) return NotFound();

//            _context.Providers.Remove(provider);
//            _context.SaveChanges();
//            _auditService.Log("Delete", "Provider", provider.ProviderId, $"Se creó el Proveedor {provider.ProviderId}");
//            return RedirectToAction(nameof(Index));
//        }
//    }
//}


using ERPSystem.Data;
using ERPSystem.Models;
using ERPSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERPSystem.Controllers
{
    [Authorize] // Solo usuarios autenticados
    public class ProvidersController : Controller
    {
        private readonly AppDbContext _context;
        private readonly AuditService _auditService;

        public ProvidersController(AppDbContext context, AuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        // LISTAR PROVEEDORES ACTIVOS
        public async Task<IActionResult> Index()
        {
            var providers = await _context.Providers
                                          .Where(p => p.IsActive)
                                          .ToListAsync();
            return View(providers);
        }

        // LISTAR PROVEEDORES INACTIVOS – solo Admin
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> InactiveProviders()
        {
            var providers = await _context.Providers
                                          .Where(p => !p.IsActive)
                                          .ToListAsync();
            return View("InactiveProviders", providers);
        }

        // DETALLES
        public async Task<IActionResult> Details(int id)
        {
            var provider = await _context.Providers.FindAsync(id);
            if (provider == null) return NotFound();
            return View(provider);
        }

        // CREAR PROVEEDOR (GET)
        public IActionResult Create() => View();

        // CREAR PROVEEDOR (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Provider provider)
        {
            if (ModelState.IsValid)
            {
                provider.IsActive = true;
                provider.CreatedAt = DateTime.Now;
                _context.Add(provider);
                await _context.SaveChangesAsync();
                _auditService.Log("Create", "Provider", provider.ProviderId, $"Se creó el proveedor {provider.Name}");
                return RedirectToAction(nameof(Index));
            }
            return View(provider);
        }

        // EDITAR PROVEEDOR (GET)
        public async Task<IActionResult> Edit(int id)
        {
            var provider = await _context.Providers.FindAsync(id);
            if (provider == null || !provider.IsActive) return NotFound();
            return View(provider);
        }

        // EDITAR PROVEEDOR (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Provider provider)
        {
            if (id != provider.ProviderId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(provider);
                    await _context.SaveChangesAsync();
                    _auditService.Log("Edit", "Provider", provider.ProviderId, $"Se editó el proveedor {provider.Name}");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Providers.Any(p => p.ProviderId == id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(provider);
        }

        // ELIMINAR PROVEEDOR (GET) – solo Admin
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var provider = await _context.Providers.FindAsync(id);
            if (provider == null || !provider.IsActive) return NotFound();
            return View(provider);
        }

        // SOFT DELETE (POST) – solo Admin
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var provider = await _context.Providers.FindAsync(id);
            if (provider != null && provider.IsActive)
            {
                provider.IsActive = false;
                _context.Update(provider);
                await _context.SaveChangesAsync();
                _auditService.Log("Delete", "Provider", provider.ProviderId, $"Se eliminó (soft) el proveedor {provider.Name}");
            }
            return RedirectToAction(nameof(Index));
        }

        // ACTIVAR PROVEEDOR INACTIVO – solo Admin
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Activate(int id)
        {
            var provider = await _context.Providers.FindAsync(id);
            if (provider != null && !provider.IsActive)
            {
                provider.IsActive = true;
                _context.Update(provider);
                await _context.SaveChangesAsync();
                _auditService.Log("Activate", "Provider", provider.ProviderId, $"Se reactivó el proveedor {provider.Name}");
            }
            return RedirectToAction(nameof(InactiveProviders));
        }
    }
}
