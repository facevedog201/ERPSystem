using ERPSystem.Data;
using ERPSystem.Models;
using Microsoft.AspNetCore.Http;
using System;

namespace ERPSystem.Services
{
    public class AuditService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public void Log(string actionType, string entityName, int? entityId = null, string description = null)
        {
            var userName = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Anonymous";

            var log = new AuditLog
            {
                UserName = userName,
                ActionType = actionType,
                EntityName = entityName,
                EntityId = entityId,
                Description = description,
                ActionDate = DateTime.Now
            };

            _context.AuditLogs.Add(log);
            _context.SaveChanges();
        }
    }
}