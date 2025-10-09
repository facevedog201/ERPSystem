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


        public InvoicesController(AppDbContext context)
        {
            _context = context;
        }

        // LISTAR FACTURAS
        public IActionResult Index()
        {
            var invoices = _context.Invoices
                .Include(i => i.Client)
                .Include(i => i.InvoiceDetails)
                .ThenInclude(d => d.Service)
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
            {
                ModelState.AddModelError("", "Debe seleccionar un cliente.");
            }

            if (selectedServices == null || selectedServices.Length == 0)
            {
                ModelState.AddModelError("", "Debe seleccionar al menos un servicio.");
            }

            if (!ModelState.IsValid)
            {
                // Recargar listas para la vista si hay error
                ViewBag.Clients = _context.Clients.ToList();
                ViewBag.Services = _context.Services.ToList();
                return View();
            }

            // Crear la factura
            var invoice = new Invoice
            {
                ClientId = clientId,
                InvoiceDate = DateTime.Now,
                InvoiceDetails = new List<InvoiceDetail>()
            };

            // Agregar los servicios seleccionados
            for (int i = 0; i < selectedServices.Length; i++)
            {
                int serviceId = selectedServices[i];
                int qty = quantities[i];

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

            // Calcular total
            invoice.Total = invoice.InvoiceDetails.Sum(d => d.Quantity * d.Price);

            // Guardar en la base de datos
            _context.Invoices.Add(invoice);
            _context.SaveChanges();
            _auditService.Log("Create", "Invoice", invoice.InvoiceId,
                $"Se creó la factura #{invoice.InvoiceId} para el cliente {invoice.ClientId}");
            return RedirectToAction("Index");
        }


        // EDITAR (GET)
        public IActionResult Edit(int id)
        {
            var invoice = _context.Invoices
                .Include(i => i.InvoiceDetails)
                .FirstOrDefault(i => i.InvoiceId == id);

            if (invoice == null) return NotFound();

            ViewBag.Clients = _context.Clients.ToList();
            ViewBag.Services = _context.Services.Where(s => s.IsActive).ToList();
            return View(invoice);
        }

        // EDITAR (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Invoice invoice, int[] serviceIds, int[] quantities)
        {
            var existingInvoice = _context.Invoices
                .Include(i => i.InvoiceDetails)
                .FirstOrDefault(i => i.InvoiceId == id);

            if (existingInvoice == null) return NotFound();

            existingInvoice.ClientId = invoice.ClientId;
            existingInvoice.InvoiceDate = invoice.InvoiceDate;

            // Actualizar detalles
            _context.InvoiceDetails.RemoveRange(existingInvoice.InvoiceDetails);
            existingInvoice.InvoiceDetails = new List<InvoiceDetail>();
            decimal total = 0;

            for (int i = 0; i < serviceIds.Length; i++)
            {
                var service = _context.Services.Find(serviceIds[i]);
                if (service != null)
                {
                    var detail = new InvoiceDetail
                    {
                        ServiceId = service.ServiceId,
                        Quantity = quantities[i],
                        Price = service.Price
                    };
                    existingInvoice.InvoiceDetails.Add(detail);
                    total += detail.Price * detail.Quantity;
                }
            }

            existingInvoice.Total = total;
            _context.SaveChanges();
            _auditService.Log("Edit", "Invoice", invoice.InvoiceId,
                $"Se edito la factura #{invoice.InvoiceId} para el cliente {invoice.ClientId}"); return RedirectToAction(nameof(Index));
        }

        // ELIMINAR (GET)
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
        public IActionResult DeleteConfirmed(int id)
        {
            var invoice = _context.Invoices
                .Include(i => i.InvoiceDetails)
                .FirstOrDefault(i => i.InvoiceId == id);

            if (invoice != null)
            {
                _context.InvoiceDetails.RemoveRange(invoice.InvoiceDetails);
                _context.Invoices.Remove(invoice);
                _context.SaveChanges();
                _auditService.Log("Delete", "Invoice", invoice.InvoiceId,
                    $"Se elimino la factura #{invoice.InvoiceId} para el cliente {invoice.ClientId}");
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
