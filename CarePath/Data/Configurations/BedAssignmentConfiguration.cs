using CarePath.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarePath.Data.Configurations;

public class BedAssignmentConfiguration : IEntityTypeConfiguration<BedAssignment>
{
    public void Configure(EntityTypeBuilder<BedAssignment> builder)
    {
        builder.ToTable("BedAssignments");
        builder.Property(b => b.Reason).HasMaxLength(500).IsRequired();
        builder.Property(b => b.AssignedBy).HasMaxLength(256).IsRequired();
        builder.HasOne(b => b.Bed).WithMany(bed => bed.Assignments).HasForeignKey(b => b.BedId);
    }
}
