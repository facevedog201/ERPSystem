using ERPSystem.Models;
using System.ComponentModel.DataAnnotations;

public class ServiceProvider
{
    [Key]
    public int ServiceProviderId { get; set; } 

    public int ServiceId { get; set; } = 0;
    public Service Service { get; set; } 

    public int ProviderId { get; set; } = 0;
    public Provider Provider { get; set; } 

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
