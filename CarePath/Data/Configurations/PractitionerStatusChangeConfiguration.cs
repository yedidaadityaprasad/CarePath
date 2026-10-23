using CarePath.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarePath.Data.Configurations;

public class PractitionerStatusChangeConfiguration : IEntityTypeConfiguration<PractitionerStatusChange>
{
    public void Configure(EntityTypeBuilder<PractitionerStatusChange> builder)
    {
        builder.ToTable("PractitionerStatusChanges");
        builder.Property(c => c.Reason).HasMaxLength(500).IsRequired();
        builder.Property(c => c.ChangedBy).HasMaxLength(256).IsRequired();
    }
}
