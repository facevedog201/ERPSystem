using ERPSystem.Models;
using ERPSystem.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ERPSystem.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Provider> Providers { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<InvoiceDetail> InvoiceDetails { get; set; }
        public DbSet<ServiceProvider> ServiceProviders { get; set; } // Opcional
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<FacturaViewModel> vw_FacturaCompleta { get; set; }
        public DbSet<ProviderInvoices> ProviderInvoices { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de la vista keyless
            modelBuilder.Entity<FacturaViewModel>()
                .HasNoKey()
                .ToView("vw_FacturaCompleta"); // nombre exacto de la vista en SQL Server
        }

    }
}
