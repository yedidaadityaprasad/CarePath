using CarePath.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarePath.Data.Configurations;

public class LabTestDefinitionConfiguration : IEntityTypeConfiguration<LabTestDefinition>
{
    public void Configure(EntityTypeBuilder<LabTestDefinition> builder)
    {
        builder.ToTable("LabTestDefinitions");
        builder.Property(t => t.Code).HasMaxLength(20).IsRequired();
        builder.HasIndex(t => t.Code).IsUnique();
        builder.Property(t => t.Name).HasMaxLength(150).IsRequired();
        builder.Property(t => t.ResultType).HasConversion<string>().HasMaxLength(20);
        builder.Property(t => t.Unit).HasMaxLength(30);
        builder.Property(t => t.ReferenceLow).HasPrecision(18, 4);
        builder.Property(t => t.ReferenceHigh).HasPrecision(18, 4);
        builder.Property(t => t.ReferenceText).HasMaxLength(200);
        builder.Property(t => t.Price).HasPrecision(18, 2);
        builder.Ignore(t => t.ReferenceRangeDisplay);

        builder.HasOne(t => t.SpecimenType).WithMany().HasForeignKey(t => t.SpecimenTypeId);
    }
}
