using ERPSystem.Data;
using ERPSystem.Models;
using ERPSystem.Services;
using Microsoft.AspNetCore.Mvc;
using System.Linq;


namespace ERPSystem.Controllers
{
    public class ClientsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly AuditService _auditService;

        public ClientsController(AppDbContext context, AuditService auditService)
        {
            _context = context;
            _auditService = auditService;

        }

        // GET: /Clients
        public IActionResult Index()
        {
            var clients = _context.Clients.ToList();
            return View(clients);
        }

        // GET: /Clients/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Clients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Client client)
        {
            if (ModelState.IsValid)
            {
                _context.Clients.Add(client);
                _context.SaveChanges();
                _auditService.Log("Create", "Client", client.ClientId,$"Se creó el Cliente {client.ClientId}");
                return RedirectToAction(nameof(Index));
            }
            return View(client);
        }

        // GET: /Clients/Edit/5
        public IActionResult Edit(int id)
        {
            var client = _context.Clients.Find(id);
            if (client == null) return NotFound();
            return View(client);
        }

        // POST: /Clients/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Client client)
        {
            if (ModelState.IsValid)
            {
                _context.Clients.Update(client);
                _context.SaveChanges();
                _auditService.Log("Edit", "Client", client.ClientId, $"Se edito el Cliente {client.ClientId}");
                return RedirectToAction(nameof(Index));
            }
            return View(client);
        }

        // GET: /Clients/Delete/5
        public IActionResult Delete(int id)
        {
            var client = _context.Clients.Find(id);
            if (client == null) return NotFound();
            return View(client);
        }

        // POST: /Clients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var client = _context.Clients.Find(id);
            if (client == null) return NotFound();

            _context.Clients.Remove(client);
            _context.SaveChanges();
            _auditService.Log("Delete", "Client", client.ClientId, $"Se elimino el Cliente {client.ClientId}");
            return RedirectToAction(nameof(Index));
        }
    }
}