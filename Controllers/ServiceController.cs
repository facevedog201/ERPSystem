////using ERPSystem.Data;
////using ERPSystem.Models;
////using Microsoft.AspNetCore.Authorization;
////using Microsoft.AspNetCore.Mvc;
////using Microsoft.EntityFrameworkCore;

////namespace ERPSystem.Controllers
////{
////    [Authorize] // Solo usuarios autenticados pueden acceder
////    public class ServicesController : Controller
////    {
////        private readonly AppDbContext _context;

////        public ServicesController(AppDbContext context)
////        {
////            _context = context;
////        }

////        // LISTAR SERVICIOS
////        public async Task<IActionResult> Index()
////        {
////            var services = await _context.Services.ToListAsync();
////            return View(services);
////        }

////        // DETALLES
////        public async Task<IActionResult> Details(int id)
////        {
////            var service = await _context.Services.FindAsync(id);
////            if (service == null) return NotFound();
////            return View(service);
////        }

////        // CREAR SERVICIO (GET)
////        public IActionResult Create()
////        {
////            return View();
////        }

////        // CREAR SERVICIO (POST)
////        [HttpPost]
////        [ValidateAntiForgeryToken]
////        public async Task<IActionResult> Create(Service service)
////        {
////            if (ModelState.IsValid)
////            {
////                _context.Add(service);
////                await _context.SaveChangesAsync();
////                return RedirectToAction(nameof(Index));
////            }
////            return View(service);
////        }

////        // EDITAR SERVICIO (GET)
////        public async Task<IActionResult> Edit(int id)
////        {
////            var service = await _context.Services.FindAsync(id);
////            if (service == null) return NotFound();
////            return View(service);
////        }

////        // EDITAR SERVICIO (POST)
////        [HttpPost]
////        [ValidateAntiForgeryToken]
////        public async Task<IActionResult> Edit(int id, Service service)
////        {
////            if (id != service.ServiceId) return NotFound();

////            if (ModelState.IsValid)
////            {
////                try
////                {
////                    _context.Update(service);
////                    await _context.SaveChangesAsync();
////                }
////                catch (DbUpdateConcurrencyException)
////                {
////                    if (!_context.Services.Any(e => e.ServiceId == id))
////                        return NotFound();
////                    else
////                        throw;
////                }
////                return RedirectToAction(nameof(Index));
////            }
////            return View(service);
////        }

////        // ELIMINAR SERVICIO (GET)
////        public async Task<IActionResult> Delete(int id)
////        {
////            var service = await _context.Services.FindAsync(id);
////            if (service == null) return NotFound();
////            return View(service);
////        }

////        // ELIMINAR SERVICIO (POST)
////        [HttpPost, ActionName("Delete")]
////        [ValidateAntiForgeryToken]
////        public async Task<IActionResult> DeleteConfirmed(int id)
////        {
////            var service = await _context.Services.FindAsync(id);
////            if (service != null)
////            {
////                _context.Services.Remove(service);
////                await _context.SaveChangesAsync();
////            }
////            return RedirectToAction(nameof(Index));
////        }
////    }
////}


//using ERPSystem.Data;
//using ERPSystem.Models;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;

//namespace ERPSystem.Controllers
//{
//    [Authorize] // Solo usuarios autenticados
//    public class ServicesController : Controller
//    {
//        private readonly AppDbContext _context;

//        public ServicesController(AppDbContext context)
//        {
//            _context = context;
//        }

//        // LISTAR SERVICIOS ACTIVOS
//        public async Task<IActionResult> Index()
//        {
//            var services = await _context.Services
//                                         .Where(s => s.IsActive)
//                                         .ToListAsync();
//            return View(services);
//        }

//        // LISTAR SERVICIOS INACTIVOS (Solo Admin)
//        [Authorize(Roles = "Admin")]
//        public async Task<IActionResult> InactiveServices()
//        {
//            var services = await _context.Services
//                                         .Where(s => !s.IsActive)
//                                         .ToListAsync();
//            return View("Index", services); // Reutiliza la misma vista
//        }

//        // DETALLES
//        public async Task<IActionResult> Details(int id)
//        {
//            var service = await _context.Services.FindAsync(id);
//            if (service == null) return NotFound();
//            return View(service);
//        }

//        // CREAR SERVICIO (GET)
//        public IActionResult Create() => View();

//        // CREAR SERVICIO (POST)
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Create(Service service)
//        {
//            if (ModelState.IsValid)
//            {
//                service.IsActive = true; // aseguramos activo
//                service.CreatedAt = DateTime.Now;
//                _context.Add(service);
//                await _context.SaveChangesAsync();
//                return RedirectToAction(nameof(Index));
//            }
//            return View(service);
//        }

//        // EDITAR SERVICIO (GET)
//        public async Task<IActionResult> Edit(int id)
//        {
//            var service = await _context.Services.FindAsync(id);
//            if (service == null || !service.IsActive) return NotFound();
//            return View(service);
//        }

//        // EDITAR SERVICIO (POST)
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Edit(int id, Service service)
//        {
//            if (id != service.ServiceId) return NotFound();

//            if (ModelState.IsValid)
//            {
//                try
//                {
//                    _context.Update(service);
//                    await _context.SaveChangesAsync();
//                }
//                catch (DbUpdateConcurrencyException)
//                {
//                    if (!_context.Services.Any(e => e.ServiceId == id))
//                        return NotFound();
//                    else
//                        throw;
//                }
//                return RedirectToAction(nameof(Index));
//            }
//            return View(service);
//        }

//        // ELIMINAR SERVICIO (GET) – solo Admin
//        [Authorize(Roles = "Admin")]
//        public async Task<IActionResult> Delete(int id)
//        {
//            var service = await _context.Services.FindAsync(id);
//            if (service == null || !service.IsActive) return NotFound();
//            return View(service);
//        }

//        // SOFT DELETE (POST) – solo Admin
//        [HttpPost, ActionName("Delete")]
//        [ValidateAntiForgeryToken]
//        [Authorize(Roles = "Admin")]
//        public async Task<IActionResult> DeleteConfirmed(int id)
//        {
//            var service = await _context.Services.FindAsync(id);
//            if (service != null && service.IsActive)
//            {
//                service.IsActive = false;
//                _context.Update(service);
//                await _context.SaveChangesAsync();
//            }
//            return RedirectToAction(nameof(Index));
//        }

//        // ACTIVAR SERVICIO INACTIVO – solo Admin
//        [Authorize(Roles = "Admin")]
//        public async Task<IActionResult> Activate(int id)
//        {
//            var service = await _context.Services.FindAsync(id);
//            if (service != null && !service.IsActive)
//            {
//                service.IsActive = true;
//                _context.Update(service);
//                await _context.SaveChangesAsync();
//            }
//            return RedirectToAction(nameof(Index));
//        }
//    }
//}

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

        // LISTAR SERVICIOS ACTIVOS
        public async Task<IActionResult> Index()
        {
            var services = await _context.Services
                                         .Where(s => s.IsActive)
                                         .ToListAsync();
            return View(services);
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
