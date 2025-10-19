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

        public IActionResult Index(DateTime? from, DateTime? to, int? providerId)
        {
            // Rangos de fecha por defecto (últimos 12 meses)
            var end = to?.Date.AddDays(1).AddTicks(-1) ?? DateTime.Now;
            var start = from?.Date ?? end.AddMonths(-11).Date;

            // Query base de facturas de proveedores
            var invoicesQuery = _context.ProviderInvoices
                .Include(pi => pi.Provider)
                .Where(pi => pi.InvoiceDate >= start && pi.InvoiceDate <= end)
                .Where(pi => pi.Provider.IsActive); // Solo proveedores activos

            // Filtro por proveedor
            if (providerId.HasValue && providerId.Value > 0)
            {
                invoicesQuery = invoicesQuery.Where(pi => pi.ProviderId == providerId.Value);
            }

            var invoices = invoicesQuery.ToList();

            // KPIs
            var totalFacturado = invoices.Sum(i => i.Amount);
            var totalCobrado = invoices.Sum(i => i.PaidAmount);
            var totalPendiente = totalFacturado - totalCobrado;
            var countInvoices = invoices.Count;

            // Facturación por mes (últimos 12 meses)
            var salesByMonth = invoices
                .GroupBy(i => new { Year = i.InvoiceDate.Year, Month = i.InvoiceDate.Month })
                .Select(g => new { Year = g.Key.Year, Month = g.Key.Month, Total = g.Sum(x => x.Amount) })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToList();

            // Estado de facturas
            var statusSummary = invoices
                .GroupBy(i => i.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToList();

            // Top proveedores
            var topProviders = invoices
                .GroupBy(i => i.Provider.Name)
                .Select(g => new { Provider = g.Key, Total = g.Sum(x => x.Amount) })
                .OrderByDescending(x => x.Total)
                .Take(10)
                .ToList();

            // Lista de proveedores activos para el filtro
            var providers = _context.Providers
                .Where(p => p.IsActive)
                .OrderBy(p => p.Name)
                .ToList();

            // Pasar datos a la vista
            ViewBag.From = start;
            ViewBag.To = end;
            ViewBag.SelectedProviderId = providerId ?? 0;
            ViewBag.TotalFacturado = totalFacturado;
            ViewBag.TotalCobrado = totalCobrado;
            ViewBag.TotalPendiente = totalPendiente;
            ViewBag.CountInvoices = countInvoices;
            ViewBag.SalesByMonth = salesByMonth;
            ViewBag.StatusSummary = statusSummary;
            ViewBag.TopProviders = topProviders;
            ViewBag.Providers = providers;

            return View();
        }

        // Acción para el listado completo de proveedores
        public IActionResult ProvidersList()
        {
            var providers = _context.Providers
                .Where(p => p.IsActive)
                .OrderBy(p => p.Name)
                .ToList();
            return View(providers);
        }



        //// LISTAR PROVEEDORES ACTIVOS
        //public async Task<IActionResult> Index()
        //{
        //    var providers = await _context.Providers
        //                                  .Where(p => p.IsActive)
        //                                  .ToListAsync();
        //    return View(providers);
        //}

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
                // Validar si el proveedor tiene facturas pendientes
                int countInvoices = await _context.ProviderInvoices
                    .CountAsync(pi => pi.ProviderId == id && pi.InvoiceNumber != null);
                // Validar si el proveedor tiene facturas pendientes
                bool hasPendingInvoices = await _context.ProviderInvoices
                    .AnyAsync(pi => pi.ProviderId == id && pi.Status !="Pagada");
                if(countInvoices > 0 && hasPendingInvoices)
                {
                    TempData["ErrorMessage"] = "No se puede dar de baja este proveedor porque tiene facturas asociadas.";
                    return RedirectToAction(nameof(ProvidersList));
                }
                //if (hasPendingInvoices)
                //{
                //    TempData["ErrorMessage"] = "No se puede dar de baja este proveedor porque tiene facturas pendientes de pago.";
                //    return RedirectToAction(nameof(ProvidersList));
                //}

                provider.IsActive = false;
                _context.Update(provider);
                await _context.SaveChangesAsync();
                _auditService.Log("Delete", "Provider", provider.ProviderId, $"Se eliminó (soft) el proveedor {provider.Name}");
                TempData["SuccessMessage"] = "Proveedor dado de baja correctamente.";
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
