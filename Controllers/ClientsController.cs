using DocumentFormat.OpenXml.Spreadsheet;
using ERPSystem.Data;
using ERPSystem.Helpers;
using ERPSystem.Models;
using ERPSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace ERPSystem.Controllers
{
    [RoleAuthorize("Admin", "Recepcion", "Contador","Vendedor","Asistente")]
    public class ClientsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly AuditService _auditService;

        public ClientsController(AppDbContext context, AuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        // LISTAR CLIENTES ACTIVOS
        public async Task<IActionResult> Index(string searchQuery)
        {
            var clients = _context.Clients.Where(c => c.IsActive == true);

            if (!string.IsNullOrEmpty(searchQuery))
            {
                clients = clients.Where(c =>
                    c.Name.Contains(searchQuery) ||
                    (c.RUC != null && c.RUC.Contains(searchQuery)) ||
                    (c.Phone != null && c.Phone.Contains(searchQuery)) ||
                    (c.Email != null && c.Email.Contains(searchQuery))
                );
            }

            return View(await clients.ToListAsync());
        }

        //// LISTAR CLIENTES ACTIVOS
        //public async Task<IActionResult> Index()
        //{
        //    var clients = await _context.Clients
        //                                .Where(c => c.IsActive==true)
        //                                .ToListAsync();
        //    return View(clients);
        //}

        // LISTAR CLIENTES INACTIVOS
        public async Task<IActionResult> InactiveClients()
        {
            var inactiveClients = await _context.Clients
                                                .Where(c => c.IsActive != true)
                                                .ToListAsync();
            return View(inactiveClients);
        }

        // DETALLES DEL CLIENTE
        public async Task<IActionResult> Details(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null || client.IsActive != true) return NotFound();
            return View(client);
        }

        // CREAR CLIENTE (GET)
        public IActionResult Create()
        {
            return View();
        }

        // CREAR CLIENTE (POST)
        [HttpPost]
        [Authorize(Roles = "Admin, Contador")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Client client)
        {
            if (ModelState.IsValid)
            {
                client.IsActive = true; // activo por defecto
                _context.Clients.Add(client);
                await _context.SaveChangesAsync();
                _auditService.Log("Create", "Client", client.ClientId, $"Se creó el Cliente {client.ClientId}");
                return RedirectToAction(nameof(Index));
            }
            return View(client);
        }

        // EDITAR CLIENTE (GET)
        public async Task<IActionResult> Edit(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null || !(client.IsActive ?? false)) return NotFound();
            return View(client);
        }

        // EDITAR CLIENTE (POST)
        [HttpPost]
        [Authorize(Roles = "Admin, Contador")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Client client)
        {
            var existingClient = await _context.Clients.FindAsync(client.ClientId);
            if (existingClient == null || !(existingClient.IsActive ?? false)) return NotFound();

            if (ModelState.IsValid)
            {
                existingClient.Name = client.Name;
                existingClient.Email = client.Email;
                existingClient.Phone = client.Phone;
                existingClient.Address = client.Address;

                await _context.SaveChangesAsync();
                _auditService.Log("Edit", "Client", existingClient.ClientId, $"Se editó el Cliente {existingClient.ClientId}");
                return RedirectToAction(nameof(Index));
            }
            return View(client);
        }

        // DAR DE BAJA CLIENTE (GET)
        public async Task<IActionResult> Delete(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null || (client.IsActive ?? false) == false) return NotFound();
            return View(client);
        }

        // DAR DE BAJA CLIENTE (POST)
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null || !(client.IsActive ?? false)) return NotFound();

            client.IsActive = false; // soft delete
            await _context.SaveChangesAsync();
            _auditService.Log("Delete", "Client", client.ClientId, $"Se dio de baja el Cliente {client.ClientId}");
            return RedirectToAction(nameof(Index));
        }

        // REACTIVAR CLIENTE
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null || (client.IsActive ?? false)) return NotFound();

            client.IsActive = true;
            await _context.SaveChangesAsync();
            _auditService.Log("Activate", "Client", client.ClientId, $"Se reactivó el Cliente {client.ClientId}");
            return RedirectToAction(nameof(Index));
        }
    }
}

