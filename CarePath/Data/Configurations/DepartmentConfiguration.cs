using CarePath.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarePath.Data.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("Departments");
        builder.Property(d => d.Code).HasMaxLength(20).IsRequired();
        builder.HasIndex(d => d.Code).IsUnique();
        builder.Property(d => d.Name).HasMaxLength(100).IsRequired();

        builder.HasData(
            new Department { Id = 1, Code = "MED", Name = "Medicine" },
            new Department { Id = 2, Code = "SURG", Name = "Surgery" },
            new Department { Id = 3, Code = "CRIT", Name = "Critical Care" },
            new Department { Id = 4, Code = "PAED", Name = "Paediatrics" });
    }
}
