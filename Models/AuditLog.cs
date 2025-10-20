using System;

namespace ERPSystem.Models
{
    public class AuditLog
    {
        public int AuditLogId { get; set; } 
        public string UserName { get; set; } = string.Empty;
        public string ActionType { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty;
        public int? EntityId { get; set; }  
        public string Description { get; set; } = string.Empty;
        public DateTime ActionDate { get; set; } = DateTime.Now;
    }
}
