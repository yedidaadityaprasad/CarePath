using CarePath.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarePath.Data.Configurations;

public class VitalSignConfiguration : IEntityTypeConfiguration<VitalSign>
{
    public void Configure(EntityTypeBuilder<VitalSign> builder)
    {
        builder.ToTable("VitalSigns");
        builder.Property(v => v.TemperatureCelsius).HasPrecision(4, 1);
        builder.Property(v => v.RecordedBy).HasMaxLength(256).IsRequired();
    }
}
