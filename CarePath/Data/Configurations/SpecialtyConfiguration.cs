using CarePath.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarePath.Data.Configurations;

public class SpecialtyConfiguration : IEntityTypeConfiguration<Specialty>
{
    public void Configure(EntityTypeBuilder<Specialty> builder)
    {
        builder.ToTable("Specialties");
        builder.Property(s => s.Code).HasMaxLength(20).IsRequired();
        builder.HasIndex(s => s.Code).IsUnique();
        builder.Property(s => s.Name).HasMaxLength(100).IsRequired();

        builder.HasData(
            new Specialty { Id = 1, Code = "GEN", Name = "General Medicine" },
            new Specialty { Id = 2, Code = "CARD", Name = "Cardiology" },
            new Specialty { Id = 3, Code = "SURG", Name = "General Surgery" },
            new Specialty { Id = 4, Code = "PED", Name = "Paediatrics" },
            new Specialty { Id = 5, Code = "ICU", Name = "Critical Care" },
            new Specialty { Id = 6, Code = "ORTH", Name = "Orthopaedics" },
            new Specialty { Id = 7, Code = "NURS", Name = "General Nursing" });
    }
}
