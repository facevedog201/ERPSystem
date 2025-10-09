using ERPSystem.Data;
using ERPSystem.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BCrypt.Net;

namespace ERPSystem.Controllers
{
    public class LoginController : Controller
    {
        private readonly AppDbContext _context;

        public LoginController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Login
        public IActionResult Index()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                // Redirige según rol si ya está logueado
                return User.IsInRole("Admin") ? RedirectToAction("Index", "Dashboard") :
                       User.IsInRole("Recepcion") ? RedirectToAction("Index", "Recepcion") :
                       User.IsInRole("Contador") ? RedirectToAction("Index", "Contabilidad") :
                       RedirectToAction("Index", "Home");
            }

            return View();
        }


        // POST: Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Usuario o contraseña inválidos");
                return View();
            }

            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                ModelState.AddModelError("", "Usuario o contraseña inválidos");
                return View();
            }

            // Crear claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("UserId", user.UserId.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true, // Mantener sesión
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // Redirigir según rol
            return user.Role switch
            {
                UserRole.Admin => RedirectToAction("Index", "Dashboard"),
                UserRole.Recepcion => RedirectToAction("Index", "Recepcion"),
                UserRole.Contador => RedirectToAction("Index", "Contabilidad"),
                _ => RedirectToAction("Index", "Home"),
            };
        }

        // Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index");
        }

        // Acceso denegado
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
