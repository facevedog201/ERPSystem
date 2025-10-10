using BCrypt.Net;
using ERPSystem.Data;
using ERPSystem.Helpers;
using ERPSystem.Models;
using ERPSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        // LISTAR USUARIOS ACTIVOS
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users
                                      .Where(u => u.IsActive)
                                      .ToListAsync();
            return View(users);
        }

        // DETALLES DE UN USUARIO
        public async Task<IActionResult> Details(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null || !user.IsActive) return NotFound();
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
                return View(user);
            }

            // Hashear contraseña
            user.Password = BCrypt.Net.BCrypt.HashPassword(password);

            // Por defecto el usuario está activo
            user.IsActive = true;

            _context.Add(user);
            await _context.SaveChangesAsync();
            _auditService.Log("Create", "User", user.UserId, $"Se creó el usuario {user.Username}");

            return RedirectToAction(nameof(Index));
        }

        // EDITAR USUARIO (GET)
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null || !user.IsActive) return NotFound();
            return View(user);
        }

        // EDITAR USUARIO (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, User user, string password)
        {
            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null || !existingUser.IsActive) return NotFound();

            // Actualizar campos permitidos
            existingUser.Username = user.Username;
            existingUser.FullName = user.FullName;
            existingUser.Role = user.Role;

            // Solo actualizar password si se ingresó algo
            if (!string.IsNullOrEmpty(password))
            {
                existingUser.Password = BCrypt.Net.BCrypt.HashPassword(password);
            }

            try
            {
                await _context.SaveChangesAsync();
                _auditService.Log("Edit", "User", existingUser.UserId, $"Se editó el usuario {existingUser.Username}");
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Users.AnyAsync(e => e.UserId == id && e.IsActive))
                    return NotFound();
                throw;
            }
        }

        // ELIMINAR USUARIO (GET)
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null || !user.IsActive) return NotFound();
            return View(user);
        }

        // ELIMINAR USUARIO (POST) - SOFT DELETE
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null || !user.IsActive) return NotFound();

            // Soft delete
            user.IsActive = false;
            await _context.SaveChangesAsync();
            _auditService.Log("Delete", "User", user.UserId, $"Se dio de baja al usuario {user.Username}");

            return RedirectToAction(nameof(Index));
        }

        // LISTAR USUARIOS INACTIVOS
        public async Task<IActionResult> InactiveUsers()
        {
            var inactiveUsers = await _context.Users
                                              .Where(u => !u.IsActive)
                                              .ToListAsync();
            return View(inactiveUsers);
        }


        // REACTIVAR USUARIO (opcional)
        public async Task<IActionResult> Activate(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null || user.IsActive) return NotFound();

            user.IsActive = true;
            await _context.SaveChangesAsync();
            _auditService.Log("Activate", "User", user.UserId, $"Se reactivó al usuario {user.Username}");

            return RedirectToAction(nameof(Index));
        }
    }
}
