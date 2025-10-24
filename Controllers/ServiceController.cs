using ERPSystem.Data;
using ERPSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERPSystem.Controllers
{
    [Authorize] // Solo usuarios autenticados
    public class ServicesController : Controller
    {
        private readonly AppDbContext _context;

        public ServicesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Search(string query)
        {
            var results = _context.Services
                .Where(s => s.IsActive && (string.IsNullOrEmpty(query) || s.Name.Contains(query)))
                .Select(s => new
                {
                    id = s.ServiceId,
                    text = s.Name + (s.HasIVA ? " (con IVA)" : " (sin IVA)"),
                    price = s.Price,
                    hasiva = s.HasIVA
                })
                .Take(20)
                .ToList();

            return Json(results);
        }

        // LISTAR SERVICIOS ACTIVOS con filtro
        public async Task<IActionResult> Index(string? name, bool? hasIVA, decimal? price)
        {
            var services = _context.Services
                                   .Where(s => s.IsActive)
                                   .AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
                services = services.Where(s => EF.Functions.Like(s.Name, $"%{name}%"));

            if (hasIVA.HasValue)
                services = services.Where(s => s.HasIVA == hasIVA.Value);

            if (price.HasValue)
                services = services.Where(s => s.Price == price.Value);

            var list = await services.OrderBy(s => s.Name).ToListAsync();

            // Mantener valores seleccionados en la vista
            ViewBag.Name = name;
            ViewBag.HasIVA = hasIVA;
            ViewBag.Price = price;

            return View(list);
        }

        [HttpGet]
        public async Task<IActionResult> FilterServices(string search)
        {
            var services = _context.Services
                                   .Where(s => s.IsActive)
                                   .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                services = services.Where(s =>
                    s.Name.Contains(search) ||
                    (s.Description != null && s.Description.Contains(search)) ||
                    s.Price.ToString().Contains(search) ||
                    (s.HasIVA ? "IVA" : "No IVA").Contains(search));
            }

            var list = await services.OrderBy(s => s.Name).ToListAsync();
            return PartialView("_ServicesTable", list);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> FilterInactiveServices(string search)
        {
            var services = _context.Services
                                   .Where(s => !s.IsActive)
                                   .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                services = services.Where(s =>
                    s.Name.Contains(search) ||
                    (s.Description != null && s.Description.Contains(search)) ||
                    s.Price.ToString().Contains(search) ||
                    (s.HasIVA ? "IVA" : "No IVA").Contains(search));
            }

            var list = await services.OrderBy(s => s.Name).ToListAsync();
            return PartialView("_ServicesTable", list);
        }



        // LISTAR SERVICIOS INACTIVOS (Solo Admin)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> InactiveServices()
        {
            var services = await _context.Services
                                         .Where(s => !s.IsActive)
                                         .ToListAsync();
            return View("InactiveServices", services);
        }

        // DETALLES
        public async Task<IActionResult> Details(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null) return NotFound();
            return View(service);
        }

        // CREAR SERVICIO (GET)
        public IActionResult Create() => View();

        // CREAR SERVICIO (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Service service)
        {
            if (ModelState.IsValid)
            {
                service.IsActive = true;
                service.CreatedAt = DateTime.Now;
                _context.Add(service);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(service);
        }

        // EDITAR SERVICIO (GET)
        public async Task<IActionResult> Edit(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null || !service.IsActive) return NotFound();
            return View(service);
        }

        // EDITAR SERVICIO (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Service service)
        {
            if (id != service.ServiceId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(service);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Services.Any(e => e.ServiceId == id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(service);
        }

        // ELIMINAR SERVICIO (GET) – solo Admin
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null || !service.IsActive) return NotFound();
            return View(service);
        }

        // SOFT DELETE (POST) – solo Admin
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service != null && service.IsActive)
            {
                service.IsActive = false;
                _context.Update(service);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // ACTIVAR SERVICIO INACTIVO – solo Admin
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Activate(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service != null && !service.IsActive)
            {
                service.IsActive = true;
                _context.Update(service);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(InactiveServices));
        }
    }
}
