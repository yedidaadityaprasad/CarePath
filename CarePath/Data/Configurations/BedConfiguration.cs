using CarePath.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarePath.Data.Configurations;

public class BedConfiguration : IEntityTypeConfiguration<Bed>
{
    public void Configure(EntityTypeBuilder<Bed> builder)
    {
        builder.ToTable("Beds");
        builder.Property(b => b.BedNumber).HasMaxLength(20).IsRequired();
        builder.HasIndex(b => new { b.WardId, b.BedNumber }).IsUnique();
        builder.Property(b => b.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(b => b.Notes).HasMaxLength(500);
        builder.Ignore(b => b.IsAssignable);

        builder.HasMany(b => b.StatusChanges).WithOne(c => c.Bed).HasForeignKey(c => c.BedId);
    }
}
