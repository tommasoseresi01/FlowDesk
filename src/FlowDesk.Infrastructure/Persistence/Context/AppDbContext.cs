using FlowDesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlowDesk.Infrastructure.Persistence.Context;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Customer> Customers => Set<Customer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Le tre chiavi esterne di audit verso Users, per ogni entità auditabile.
        // Nessuna cascata: l'applicazione non cancella mai fisicamente.
        var auditableTypes = typeof(AuditableEntity).Assembly
            .GetTypes()
            .Where(t => typeof(AuditableEntity).IsAssignableFrom(t) && !t.IsAbstract);

        foreach (var entityType in auditableTypes)
        {
            modelBuilder.Entity(entityType)
                .HasOne(nameof(AuditableEntity.UserCreation)).WithMany()
                .HasForeignKey(nameof(AuditableEntity.IdUserCreation))
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity(entityType)
                .HasOne(nameof(AuditableEntity.UserModification)).WithMany()
                .HasForeignKey(nameof(AuditableEntity.IdUserModification))
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity(entityType)
                .HasOne(nameof(AuditableEntity.UserDeleted)).WithMany()
                .HasForeignKey(nameof(AuditableEntity.IdUserDeleted))
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        }

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
