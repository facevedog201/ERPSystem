using BCrypt.Net;
using ERPSystem.Data;
using ERPSystem.Models;
using ERPSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using ERPSystem.Helpers;

namespace ERPSystem.Controllers
{
    [RoleAuthorize("Admin")]
    public class UsersController : Controller
    {
        private readonly AppDbContext _context;
        private readonly AuditService _auditService;


        public UsersController(AppDbContext context, AuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        // LISTAR USUARIOS
        public IActionResult Index()
        {
            var users = _context.Users.ToList();
            return View(users);
        }

        // DETALLES DE UN USUARIO
        public IActionResult Details(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null) return NotFound();
            return View(user);
        }

        // CREAR USUARIO (GET)
        public IActionResult Create()
        {
            return View();
        }

        // CREAR USUARIO (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user, string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("Password", "Password is required");
            }

            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Model Error: {error.ErrorMessage}");
                }
                return View(user);
            }

            // Usar BCrypt para hashing
            user.Password = BCrypt.Net.BCrypt.HashPassword(password);

            _context.Add(user);
            await _context.SaveChangesAsync();
            _auditService.Log("Create", "User", user.UserId, $"Se creó el usuario {user.Username}");
            return RedirectToAction(nameof(Index));
        }




        // EDITAR USUARIO (GET)
        public IActionResult Edit(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null) return NotFound();
            return View(user);
        }

        // EDITAR USUARIO (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, User user, string password)
        {
            if (id != user.UserId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (!string.IsNullOrEmpty(password))
                    {
                        user.Password = BCrypt.Net.BCrypt.HashPassword(password);
                    }
                    _context.Update(user);
                    _context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Users.Any(e => e.UserId == id))
                        return NotFound();
                    else
                        throw;
                }
                _auditService.Log("Edit", "User", user.UserId, $"Se editó el usuario {user.Username}");
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // ELIMINAR USUARIO (GET)
        public IActionResult Delete(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null) return NotFound();
            return View(user);
        }

        // ELIMINAR USUARIO (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var user = _context.Users.Find(id);
            _context.Users.Remove(user);
            _context.SaveChanges();
            _auditService.Log("Delete", "User", user.UserId, $"Se eliminó el usuario {user.Username}");
            return RedirectToAction(nameof(Index));
        }
    }
}
