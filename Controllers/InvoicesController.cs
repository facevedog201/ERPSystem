using ERPSystem.Data;
using ERPSystem.Models;
using ERPSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERPSystem.Controllers
{
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
        public IActionResult Index()
        {
            var invoices = _context.Invoices
                .Include(i => i.Client)
                .Include(i => i.InvoiceDetails)
                    .ThenInclude(d => d.Service)
                .Include(i => i.Payments)
                .ToList();

            return View(invoices);
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

            return View(invoice);
        }

        // CREAR (GET)
        public IActionResult Create()
        {
            ViewBag.Clients = _context.Clients.ToList();
            ViewBag.Services = _context.Services.Where(s => s.IsActive).ToList();
            return View(new Invoice { InvoiceDetails = new List<InvoiceDetail>() });
        }

        // CREAR (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(int clientId, int[] selectedServices, int[] quantities)
        {
            // Validaciones básicas
            if (clientId == 0)
                ModelState.AddModelError("", "Debe seleccionar un cliente.");

            if (selectedServices == null || selectedServices.Length == 0)
                ModelState.AddModelError("", "Debe seleccionar al menos un servicio.");

            if (!ModelState.IsValid)
            {
                ViewBag.Clients = _context.Clients.ToList();
                ViewBag.Services = _context.Services.Where(s => s.IsActive).ToList();
                return View();
            }

            var invoice = new Invoice
            {
                ClientId = clientId,
                InvoiceDate = DateTime.Now,
                InvoiceDetails = new List<InvoiceDetail>()
            };

            // Solo agregar servicios con cantidad > 0
            for (int i = 0; i < selectedServices.Length; i++)
            {
                int serviceId = selectedServices[i];
                int qty = quantities[i];

                if (qty <= 0) continue; // Ignorar si la cantidad es 0 o negativa

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

            if (!invoice.InvoiceDetails.Any())
            {
                ModelState.AddModelError("", "Debe agregar al menos un servicio con cantidad válida.");
                ViewBag.Clients = _context.Clients.ToList();
                ViewBag.Services = _context.Services.Where(s => s.IsActive).ToList();
                return View();
            }

            // Calcular total
            invoice.Total = invoice.InvoiceDetails.Sum(d => d.Quantity * d.Price);

            _context.Invoices.Add(invoice);
            _context.SaveChanges();

            _auditService?.Log("Create", "Invoice", invoice.InvoiceId,
                $"Se creó la factura #{invoice.InvoiceId} para el cliente {invoice.ClientId}");

            return RedirectToAction("Index");
        }


        // AGREGAR PAGO
        public IActionResult AddPayment(int id)
        {
            var invoice = _context.Invoices
                .Include(i => i.Payments)
                .Include(i => i.InvoiceDetails)
                .FirstOrDefault(i => i.InvoiceId == id);

            if (invoice == null) return NotFound();

            return View(invoice);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddPayment(int invoiceId, decimal amount, string notes)
        {
            var invoice = _context.Invoices
                .Include(i => i.Payments)
                .FirstOrDefault(i => i.InvoiceId == invoiceId);

            if (invoice == null) return NotFound();
            if (amount <= 0) return BadRequest("Monto inválido.");

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

            _auditService.Log("AddPayment", "Invoice", invoiceId,
                $"Se registró un pago de {amount:C} para la factura #{invoiceId}");

            return RedirectToAction("Details", new { id = invoiceId });
        }
    }
}
