using FlowDesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowDesk.Infrastructure.Persistence.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(x => x.IdUser);

        builder.Property(x => x.Email).HasMaxLength(254).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Surname).HasMaxLength(120).IsRequired();

        // Un utente di Entra ID corrisponde a una sola riga: è anche ciò che rende sicura
        // la creazione al primo accesso quando arrivano due richieste insieme.
        builder.HasIndex(x => x.EntraObjectId).IsUnique();
        builder.HasIndex(x => x.Email);

        builder.HasOne(x => x.Role)
            .WithMany()
            .HasForeignKey(x => x.IdRole)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
