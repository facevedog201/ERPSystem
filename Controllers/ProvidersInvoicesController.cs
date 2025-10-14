using ERPSystem.Data;
using ERPSystem.Models;
using ERPSystem.Services;
using ERPSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

[Authorize]
public class ProviderInvoicesController : Controller
{
    private readonly AppDbContext _context;
    private readonly AuditService _audit;
    public ProviderInvoicesController(AppDbContext context, AuditService audit)
    {
        _context = context;
        _audit = audit;
    }

    // LISTAR FACTURAS POR PROVEEDOR
    public async Task<IActionResult> Index(int providerId)
    {
        var provider = await _context.Providers.FindAsync(providerId);
        if (provider == null) return NotFound();

        var invoices = await _context.ProviderInvoices
            .Where(i => i.ProviderId == providerId)
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync();

        ViewBag.ProviderName = provider.Name;
        ViewBag.ProviderId = providerId;
        return View(invoices); // /Views/ProviderInvoices/Index.cshtml
    }

    // CREAR FACTURA (GET)
    [HttpGet]
    public IActionResult Create(int providerId)
    {
        var provider = _context.Providers.Find(providerId);
        if (provider == null) return NotFound();

        var model = new ProviderInvoice
        {
            ProviderId = providerId,
            InvoiceDate = DateTime.Today,
            DueDate = DateTime.Today.AddDays(30)
        };
        ViewBag.ProviderName = provider.Name;
        return View(model); // /Views/ProviderInvoices/Create.cshtml
    }

    // CREAR FACTURA (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProviderInvoice model)
    {
        // Validación adicional de negocio
        if (model.InvoiceDate > model.DueDate)
            ModelState.AddModelError(nameof(model.DueDate), "La fecha de vencimiento debe ser posterior a la fecha de la factura.");
        if (!ModelState.IsValid)
        {
            ViewBag.ProviderName = (await _context.Providers.FindAsync(model.ProviderId))?.Name;
            // Log de errores para depurar
            foreach (var kv in ModelState)
            {
                foreach (var err in kv.Value.Errors)
                    Console.WriteLine($"ModelState error in {kv.Key}: {err.ErrorMessage}");
            }
            return View(model);
        }
        // Guardado
        model.Status = "Pendiente";
        model.CreatedAt = DateTime.UtcNow;
        _context.Add(model);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Factura creada correctamente.";
        return RedirectToAction(nameof(Index), new { providerId = model.ProviderId });
    }


    // REGISTRAR PAGO (GET)
    [HttpGet]
    public async Task<IActionResult>  RegisterPayment(int id)
    {
        var inv = await _context.ProviderInvoices.Include(i => i.Provider)
            .FirstOrDefaultAsync(i => i.ProviderInvoiceId == id);
        if (inv == null) return NotFound();

        var vm = new ProviderPaymentViewModel
        {
            ProviderInvoiceId = inv.ProviderInvoiceId,
            ProviderId = inv.ProviderId,
            ProviderName = inv.Provider?.Name ?? string.Empty,
            InvoiceNumber = inv.InvoiceNumber,
            Outstanding = inv.Amount - inv.PaidAmount,
            PaymentDate = DateTime.Today
        };
        return View(vm); // /Views/ProviderInvoices/RegisterPayment.cshtml
    }

    // REGISTRAR PAGO (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegisterPayment(ProviderPaymentViewModel vm)
    {
        var inv = await _context.ProviderInvoices.FindAsync(vm.ProviderInvoiceId);
        if (inv == null) return NotFound();

        if (vm.Amount <= 0)
            ModelState.AddModelError(nameof(vm.Amount), "El monto debe ser mayor que 0.");

        var outstanding = inv.Amount - inv.PaidAmount;
        if (vm.Amount > outstanding)
            ModelState.AddModelError(nameof(vm.Amount), $"El monto excede el saldo pendiente ({outstanding:C}).");

        if (!ModelState.IsValid) return View(vm);

        inv.PaidAmount += vm.Amount;
        inv.Status = inv.PaidAmount >= inv.Amount ? "Pagada" : "Parcial";

        await _context.SaveChangesAsync();
        _audit.Log("RegisterPayment", "ProviderInvoice", inv.ProviderInvoiceId,
            $"Pago de {vm.Amount:C} aplicado a factura {inv.InvoiceNumber}");

        return RedirectToAction(nameof(Index), new { providerId = inv.ProviderId });
    }
}