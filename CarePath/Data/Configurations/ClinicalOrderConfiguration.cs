using CarePath.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarePath.Data.Configurations;

public class ClinicalOrderConfiguration : IEntityTypeConfiguration<ClinicalOrder>
{
    public void Configure(EntityTypeBuilder<ClinicalOrder> builder)
    {
        builder.ToTable("ClinicalOrders");
        builder.Property(o => o.OrderNumber).HasMaxLength(30).IsRequired();
        builder.HasIndex(o => o.OrderNumber).IsUnique();
        builder.Property(o => o.Type).HasConversion<string>().HasMaxLength(20);
        builder.Property(o => o.Priority).HasConversion<string>().HasMaxLength(20);
        builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(o => new { o.Type, o.Status });
        builder.Property(o => o.Notes).HasMaxLength(1000);
        builder.Property(o => o.PlacedBy).HasMaxLength(256).IsRequired();
        builder.Property(o => o.CancellationReason).HasMaxLength(500);
        builder.Ignore(o => o.Description);

        builder.HasOne(o => o.OrderingPractitioner).WithMany().HasForeignKey(o => o.OrderingPractitionerId);
        builder.HasOne(o => o.LabDetail).WithOne(d => d.Order).HasForeignKey<LabOrderDetail>(d => d.ClinicalOrderId);
        builder.HasOne(o => o.MedicationDetail).WithOne(d => d.Order).HasForeignKey<MedicationOrderDetail>(d => d.ClinicalOrderId);
        builder.HasOne(o => o.Dispense).WithOne(d => d.Order).HasForeignKey<DispenseRecord>(d => d.ClinicalOrderId);
        builder.HasMany(o => o.StatusChanges).WithOne(c => c.Order).HasForeignKey(c => c.ClinicalOrderId);
    }
}
