using CarePath.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarePath.Data.Configurations;

public class SpecimenTypeConfiguration : IEntityTypeConfiguration<SpecimenType>
{
    public void Configure(EntityTypeBuilder<SpecimenType> builder)
    {
        builder.ToTable("SpecimenTypes");
        builder.Property(s => s.Code).HasMaxLength(20).IsRequired();
        builder.HasIndex(s => s.Code).IsUnique();
        builder.Property(s => s.Name).HasMaxLength(100).IsRequired();

        builder.HasData(
            new SpecimenType { Id = 1, Code = "BLD", Name = "Whole blood" },
            new SpecimenType { Id = 2, Code = "SER", Name = "Serum" },
            new SpecimenType { Id = 3, Code = "URN", Name = "Urine" },
            new SpecimenType { Id = 4, Code = "SWB", Name = "Swab" },
            new SpecimenType { Id = 5, Code = "CSF", Name = "Cerebrospinal fluid" });
    }
}
