namespace ERPSystem.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        // ✅ Nueva propiedad para mensajes de error personalizados
        public string? Message { get; set; }
    }
}
