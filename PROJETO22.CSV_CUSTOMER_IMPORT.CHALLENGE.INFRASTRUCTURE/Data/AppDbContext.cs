using Microsoft.EntityFrameworkCore;
using PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.DOMAIN.Entities;

namespace PROJETO22.CSV_CUSTOMER_IMPORT.CHALLENGE.INFRASTRUCTURE.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Client> Clients { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Client>(builder =>
            {
                builder.HasKey(c => c.Id);
                builder.Property(c => c.Name).IsRequired().HasMaxLength(200);
                builder.Property(c => c.Cpf).IsRequired().HasMaxLength(11);
                builder.Property(c => c.Email).IsRequired().HasMaxLength(320);
                builder.Property(c => c.CreatedAt).IsRequired();
            });
        }
    }
}