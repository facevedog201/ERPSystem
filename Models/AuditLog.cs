using System;

namespace ERPSystem.Models
{
    public class AuditLog
    {
        public int AuditLogId { get; set; }
        public string UserName { get; set; }
        public string ActionType { get; set; }
        public string EntityName { get; set; }
        public int? EntityId { get; set; }
        public string Description { get; set; }
        public DateTime ActionDate { get; set; } = DateTime.Now;
    }
}
