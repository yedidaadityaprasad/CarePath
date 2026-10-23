using CarePath.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarePath.Data.Configurations;

public class DispenseRecordConfiguration : IEntityTypeConfiguration<DispenseRecord>
{
    public void Configure(EntityTypeBuilder<DispenseRecord> builder)
    {
        builder.ToTable("DispenseRecords");
        builder.Property(d => d.UnitPrice).HasPrecision(18, 2);
        builder.Property(d => d.DispensedBy).HasMaxLength(256).IsRequired();
        builder.Property(d => d.Notes).HasMaxLength(500);

        builder.HasOne(d => d.WardStock).WithMany().HasForeignKey(d => d.WardStockId);
    }
}
