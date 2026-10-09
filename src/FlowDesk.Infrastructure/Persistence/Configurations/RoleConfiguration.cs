using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Entities.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowDesk.Infrastructure.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");
        builder.HasKey(x => x.IdRole);
        builder.Property(x => x.IdRole).ValueGeneratedNever();
        builder.Property(x => x.RoleName).HasMaxLength(50).IsRequired();

        // Gli id coincidono con RoleEnum e con il frontend.
        builder.HasData(
            new Role { IdRole = RoleEnum.ADMIN, RoleName = nameof(RoleEnum.ADMIN) },
            new Role { IdRole = RoleEnum.MANAGER, RoleName = nameof(RoleEnum.MANAGER) },
            new Role { IdRole = RoleEnum.OPERATOR, RoleName = nameof(RoleEnum.OPERATOR) });
    }
}
