using CarePath.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarePath.Data.Configurations;

public class WardConfiguration : IEntityTypeConfiguration<Ward>
{
    public void Configure(EntityTypeBuilder<Ward> builder)
    {
        builder.ToTable("Wards");
        builder.Property(w => w.Code).HasMaxLength(20).IsRequired();
        builder.HasIndex(w => w.Code).IsUnique();
        builder.Property(w => w.Name).HasMaxLength(100).IsRequired();
        builder.Property(w => w.DailyRate).HasPrecision(18, 2);
        builder.Property(w => w.ClosureReason).HasMaxLength(500);

        builder.HasOne(w => w.Department).WithMany(d => d.Wards).HasForeignKey(w => w.DepartmentId);
        builder.HasMany(w => w.Beds).WithOne(b => b.Ward).HasForeignKey(b => b.WardId);
    }
}
