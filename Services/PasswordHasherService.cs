using BCrypt.Net;

namespace ERPSystem.Services
{
    public static class PasswordHasherService
    {
        // Hashea una contraseña nueva
        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        // Verifica contraseña contra hash almacenado
        public static bool VerifyPassword(string inputPassword, string storedHash)
        {
            if (string.IsNullOrEmpty(inputPassword) || string.IsNullOrEmpty(storedHash))
                return false;

            return BCrypt.Net.BCrypt.Verify(inputPassword, storedHash);
        }
    }
}
