using CarePath.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarePath.Data.Configurations;

public class BedStatusChangeConfiguration : IEntityTypeConfiguration<BedStatusChange>
{
    public void Configure(EntityTypeBuilder<BedStatusChange> builder)
    {
        builder.ToTable("BedStatusChanges");
        builder.Property(c => c.FromStatus).HasConversion<string>().HasMaxLength(20);
        builder.Property(c => c.ToStatus).HasConversion<string>().HasMaxLength(20);
        builder.Property(c => c.ChangedBy).HasMaxLength(256).IsRequired();
        builder.Property(c => c.Reason).HasMaxLength(500).IsRequired();
    }
}
