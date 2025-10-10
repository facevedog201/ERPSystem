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
        public async Task<IActionResult> Index()
        {
            var clients = await _context.Clients
                                        .Where(c => c.IsActive)
                                        .ToListAsync();
            return View(clients);
        }

        // LISTAR CLIENTES INACTIVOS
        public async Task<IActionResult> InactiveClients()
        {
            var inactiveClients = await _context.Clients
                                                .Where(c => !c.IsActive)
                                                .ToListAsync();
            return View(inactiveClients);
        }

        // DETALLES DEL CLIENTE
        public async Task<IActionResult> Details(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null || !client.IsActive) return NotFound();
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
            if (client == null || !client.IsActive) return NotFound();
            return View(client);
        }

        // EDITAR CLIENTE (POST)
        [HttpPost]
        [Authorize(Roles = "Admin, Contador")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Client client)
        {
            var existingClient = await _context.Clients.FindAsync(client.ClientId);
            if (existingClient == null || !existingClient.IsActive) return NotFound();

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
            if (client == null || !client.IsActive) return NotFound();
            return View(client);
        }

        // DAR DE BAJA CLIENTE (POST)
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null || !client.IsActive) return NotFound();

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
            if (client == null || client.IsActive) return NotFound();

            client.IsActive = true;
            await _context.SaveChangesAsync();
            _auditService.Log("Activate", "Client", client.ClientId, $"Se reactivó el Cliente {client.ClientId}");
            return RedirectToAction(nameof(InactiveClients));
        }
    }
}





//using ERPSystem.Data;
//using ERPSystem.Helpers;
//using ERPSystem.Models;
//using ERPSystem.Services;
//using Microsoft.AspNetCore.Mvc;
//using System.Linq;


//namespace ERPSystem.Controllers
//{
//    [RoleAuthorize("Admin","Recepcion","Contabilidad")]
//    public class ClientsController : Controller
//    {
//        private readonly AppDbContext _context;
//        private readonly AuditService _auditService;

//        public ClientsController(AppDbContext context, AuditService auditService)
//        {
//            _context = context;
//            _auditService = auditService;

//        }

//        // GET: /Clients
//        public IActionResult Index()
//        {
//            var clients = _context.Clients.ToList();
//            return View(clients);
//        }

//        // GET: /Clients/Create
//        public IActionResult Create()
//        {
//            return View();
//        }

//        // POST: /Clients/Create
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public IActionResult Create(Client client)
//        {
//            if (ModelState.IsValid)
//            {
//                _context.Clients.Add(client);
//                _context.SaveChanges();
//                _auditService.Log("Create", "Client", client.ClientId,$"Se creó el Cliente {client.ClientId}");
//                return RedirectToAction(nameof(Index));
//            }
//            return View(client);
//        }

//        // GET: /Clients/Edit/5
//        public IActionResult Edit(int id)
//        {
//            var client = _context.Clients.Find(id);
//            if (client == null) return NotFound();
//            return View(client);
//        }

//        // POST: /Clients/Edit/5
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public IActionResult Edit(Client client)
//        {
//            if (ModelState.IsValid)
//            {
//                _context.Clients.Update(client);
//                _context.SaveChanges();
//                _auditService.Log("Edit", "Client", client.ClientId, $"Se edito el Cliente {client.ClientId}");
//                return RedirectToAction(nameof(Index));
//            }
//            return View(client);
//        }

//        // GET: /Clients/Delete/5
//        public IActionResult Delete(int id)
//        {
//            var client = _context.Clients.Find(id);
//            if (client == null) return NotFound();
//            return View(client);
//        }

//        // POST: /Clients/Delete/5
//        [HttpPost, ActionName("Delete")]
//        [ValidateAntiForgeryToken]
//        public IActionResult DeleteConfirmed(int id)
//        {
//            var client = _context.Clients.Find(id);
//            if (client == null) return NotFound();

//            _context.Clients.Remove(client);
//            _context.SaveChanges();
//            _auditService.Log("Delete", "Client", client.ClientId, $"Se elimino el Cliente {client.ClientId}");
//            return RedirectToAction(nameof(Index));
//        }
//    }
//}