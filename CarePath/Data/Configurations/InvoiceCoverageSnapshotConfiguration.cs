using CarePath.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarePath.Data.Configurations;

public class InvoiceCoverageSnapshotConfiguration : IEntityTypeConfiguration<InvoiceCoverageSnapshot>
{
    public void Configure(EntityTypeBuilder<InvoiceCoverageSnapshot> builder)
    {
        builder.ToTable("InvoiceCoverageSnapshots");
        builder.Property(s => s.PayerCode).HasMaxLength(20).IsRequired();
        builder.Property(s => s.PayerName).HasMaxLength(150).IsRequired();
        builder.Property(s => s.MemberId).HasMaxLength(50).IsRequired();
        builder.Property(s => s.GroupNumber).HasMaxLength(50);
        builder.Property(s => s.CoveragePercent).HasPrecision(5, 2);
        builder.Property(s => s.CoveredAmount).HasPrecision(18, 2);
    }
}
