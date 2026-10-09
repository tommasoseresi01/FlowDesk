using FlowDesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowDesk.Infrastructure.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        builder.HasKey(x => x.IdCustomer);

        builder.Property(x => x.LegalName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.VatNumber).HasMaxLength(11).IsUnicode(false).IsRequired();
        builder.Property(x => x.ContactName).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(254).IsRequired();
        builder.Property(x => x.Phone).HasMaxLength(30).IsRequired();

        // La partita IVA è unica anche tra i clienti archiviati.
        builder.HasIndex(x => x.VatNumber).IsUnique();
        builder.HasIndex(x => x.LegalName);
    }
}
