using Microsoft.EntityFrameworkCore;
using PurchaseApp.Models;

namespace PurchaseApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<PurchaseRequest> PurchaseRequests => Set<PurchaseRequest>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    }
}

