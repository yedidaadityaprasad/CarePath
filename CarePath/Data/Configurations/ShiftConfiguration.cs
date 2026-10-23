using CarePath.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarePath.Data.Configurations;

public class ShiftConfiguration : IEntityTypeConfiguration<Shift>
{
    public void Configure(EntityTypeBuilder<Shift> builder)
    {
        builder.ToTable("Shifts");
        builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(s => s.Notes).HasMaxLength(500);
        builder.Property(s => s.CreatedBy).HasMaxLength(256).IsRequired();
        builder.Property(s => s.CancellationReason).HasMaxLength(500);
        builder.HasIndex(s => new { s.PractitionerId, s.StartsAt });

        builder.HasOne(s => s.Practitioner).WithMany(p => p.Shifts).HasForeignKey(s => s.PractitionerId);
        builder.HasOne(s => s.Ward).WithMany(w => w.Shifts).HasForeignKey(s => s.WardId);
    }
}
