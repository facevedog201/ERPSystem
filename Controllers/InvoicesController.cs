using ERPSystem.Data;
using ERPSystem.Helpers;
using ERPSystem.Models;
using ERPSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERPSystem.Controllers
{
    [Authorize]
    public class InvoicesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly AuditService _auditService;

        public InvoicesController(AppDbContext context, AuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        // LISTAR FACTURAS
        public IActionResult Index(string search)
        {
            ViewBag.Search = search;

            var invoices = _context.Invoices
                .Include(i => i.Client)
                .Include(i => i.InvoiceDetails)
                    .ThenInclude(d => d.Service)
                .Include(i => i.Payments)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                invoices = invoices.Where(i =>
                    i.InvoiceId.ToString().Contains(search) ||
                    i.Client.Name.Contains(search));
            }

            return View(invoices.ToList());
        }


        // DETALLES
        public IActionResult Details(int id)
        {
            var invoice = _context.Invoices
                .Include(i => i.Client)
                .Include(i => i.InvoiceDetails)
                    .ThenInclude(d => d.Service)
                .Include(i => i.Payments)
                .FirstOrDefault(i => i.InvoiceId == id);

            if (invoice == null) return NotFound();

            invoice.UpdateStatus();
            _context.SaveChanges();

            return View(invoice);
        }

        // CREAR (GET)
        [RoleAuthorize("Admin", "Contador","Recepcion")]
        public IActionResult Create()
        {
            ViewBag.Clients = _context.Clients.ToList();
            ViewBag.Services = _context.Services.Where(s => s.IsActive).ToList();
            return View(new Invoice { InvoiceDetails = new List<InvoiceDetail>() });
        }

        // CREAR (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Admin", "Contador","Recepcion")]
        public IActionResult Create(int clientId, int[] selectedServices, int[] quantities)
        {
            if (clientId == 0) ModelState.AddModelError("", "Debe seleccionar un cliente.");
            if (selectedServices == null || selectedServices.Length == 0) ModelState.AddModelError("", "Debe seleccionar al menos un servicio.");

            if (!ModelState.IsValid)
            {
                ViewBag.Clients = _context.Clients.ToList();
                ViewBag.Services = _context.Services.ToList();
                return View();
            }

            var invoice = new Invoice
            {
                ClientId = clientId,
                InvoiceDate = DateTime.Now,
                InvoiceDetails = new List<InvoiceDetail>()
            };

            for (int i = 0; i < selectedServices.Length; i++)
            {
                int serviceId = selectedServices[i];
                int qty = quantities[i];

                if (qty <= 0) continue;

                var service = _context.Services.Find(serviceId);
                if (service != null)
                {
                    invoice.InvoiceDetails.Add(new InvoiceDetail
                    {
                        ServiceId = service.ServiceId,
                        Quantity = qty,
                        Price = service.Price
                    });
                }
            }

            invoice.Total = invoice.InvoiceDetails.Sum(d => d.Quantity * d.Price);
            _context.Invoices.Add(invoice);
            _context.SaveChanges();

            _auditService.Log("Create", "Invoice", invoice.InvoiceId,
                $"Se creó la factura #{invoice.InvoiceId} para el cliente {invoice.ClientId}");

            return RedirectToAction("Index");
        }

        // ELIMINAR (GET)
        [RoleAuthorize("Admin", "Contador")]
        public IActionResult Delete(int id)
        {
            var invoice = _context.Invoices
                .Include(i => i.Client)
                .FirstOrDefault(i => i.InvoiceId == id);

            if (invoice == null) return NotFound();
            return View(invoice);
        }

        // ELIMINAR (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [RoleAuthorize("Admin", "Contador")]
        public IActionResult DeleteConfirmed(int id)
        {
            var invoice = _context.Invoices
                .Include(i => i.InvoiceDetails)
                .Include(i => i.Payments)
                .FirstOrDefault(i => i.InvoiceId == id);

            if (invoice != null)
            {
                _context.InvoiceDetails.RemoveRange(invoice.InvoiceDetails);
                _context.Payments.RemoveRange(invoice.Payments);
                _context.Invoices.Remove(invoice);
                _context.SaveChanges();

                _auditService.Log("Delete", "Invoice", invoice.InvoiceId,
                    $"Se eliminó la factura #{invoice.InvoiceId} para el cliente {invoice.ClientId}");
            }

            return RedirectToAction(nameof(Index));
        }

        // AGREGAR PAGO
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddPayment(int invoiceId, decimal amount, string notes)
        {
            var invoice = _context.Invoices
                .Include(i => i.Payments)
                .FirstOrDefault(i => i.InvoiceId == invoiceId);

            if (invoice == null) return NotFound();

            invoice.UpdateStatus();

            if (invoice.Status == InvoiceStatus.Paid)
            {
                TempData["Error"] = "Factura ya pagada, no se pueden registrar más pagos.";
                return RedirectToAction("Details", new { id = invoiceId });
            }

            var remaining = invoice.Total - invoice.PaidAmount;
            if (amount <= 0)
            {
                TempData["Error"] = "El monto debe ser mayor a 0.";
                return RedirectToAction("Details", new { id = invoiceId });
            }

            if (amount > remaining)
            {
                TempData["Error"] = $"El monto excede el saldo pendiente ({remaining:C}).";
                return RedirectToAction("Details", new { id = invoiceId });
            }

            var payment = new Payment
            {
                InvoiceId = invoiceId,
                Amount = amount,
                Notes = notes
            };

            _context.Payments.Add(payment);
            invoice.PaidAmount += amount;
            invoice.UpdateStatus();

            _context.SaveChanges();
            TempData["Success"] = $"Pago de {amount:C} registrado correctamente.";

            return RedirectToAction("Details", new { id = invoiceId });
        }
    }
}
