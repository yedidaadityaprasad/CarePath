using CarePath.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarePath.Data.Configurations;

public class OrderStatusChangeConfiguration : IEntityTypeConfiguration<OrderStatusChange>
{
    public void Configure(EntityTypeBuilder<OrderStatusChange> builder)
    {
        builder.ToTable("OrderStatusChanges");
        builder.Property(c => c.FromStatus).HasConversion<string>().HasMaxLength(20);
        builder.Property(c => c.ToStatus).HasConversion<string>().HasMaxLength(20);
        builder.Property(c => c.ChangedBy).HasMaxLength(256).IsRequired();
        builder.Property(c => c.Note).HasMaxLength(500);
    }
}
