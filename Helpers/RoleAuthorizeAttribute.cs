using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ERPSystem.Helpers
{
    public class RoleAuthorizeAttribute : ActionFilterAttribute
    {
        private readonly string[] _roles;

        public RoleAuthorizeAttribute(params string[] roles)
        {
            _roles = roles;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Obtenemos el rol del usuario desde Claims
            var roleClaim = context.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Role);
            var userRole = roleClaim?.Value;

            // Si no tiene rol o no está en la lista de permitidos
            if (string.IsNullOrEmpty(userRole) || !_roles.Contains(userRole))
            {
                // Redirigir a página de acceso denegado
                context.Result = new RedirectToActionResult("AccessDenied", "Login", null);
            }

            base.OnActionExecuting(context);
        }
    }
}
