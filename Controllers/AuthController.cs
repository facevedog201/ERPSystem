using Microsoft.AspNetCore.Mvc;
using ERPSystem.Data;
using ERPSystem.Models;
using System.Linq;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using System.Text;

namespace ERPSystem.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Auth/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Auth/Login
        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Debe ingresar usuario y contraseña.";
                return View();
            }

            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user == null)
            {
                ViewBag.Error = "Usuario no encontrado.";
                return View();
            }

            if (!VerifyPassword(password, user.Password))
            {
                ViewBag.Error = "Contraseña incorrecta.";
                return View();
            }

            // Guardamos información básica del usuario en sesión
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("Role", user.Role.ToString());

            // Redirigir según el rol del usuario
            switch (user.Role)
            {
                case UserRole.Admin:
                    return RedirectToAction("Index", "AdminDashboard");

                case UserRole.Recepcion:
                    return RedirectToAction("Index", "Facturacion");

                case UserRole.Contador:
                    return RedirectToAction("Index", "Reportes");

                default:
                    return RedirectToAction("Index", "Home");
            }
        }

        // GET: /Auth/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // Método para verificar hash
        private bool VerifyPassword(string password, string storedHash)
        {
            // Comparación simple por ahora (más adelante se implementará un hash seguro)
            return storedHash == password;
        }
    }
}