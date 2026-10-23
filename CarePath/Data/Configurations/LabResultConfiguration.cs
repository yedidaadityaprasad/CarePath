using CarePath.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarePath.Data.Configurations;

public class LabResultConfiguration : IEntityTypeConfiguration<LabResult>
{
    public void Configure(EntityTypeBuilder<LabResult> builder)
    {
        builder.ToTable("LabResults");
        builder.Property(r => r.NumericValue).HasPrecision(18, 4);
        builder.Property(r => r.TextValue).HasMaxLength(500);
        builder.Property(r => r.Flag).HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.Comment).HasMaxLength(1000);
        builder.Property(r => r.ResultedBy).HasMaxLength(256).IsRequired();
        builder.Ignore(r => r.DisplayValue);
    }
}
