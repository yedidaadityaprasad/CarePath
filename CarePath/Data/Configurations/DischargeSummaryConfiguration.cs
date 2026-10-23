using CarePath.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarePath.Data.Configurations;

public class DischargeSummaryConfiguration : IEntityTypeConfiguration<DischargeSummary>
{
    public void Configure(EntityTypeBuilder<DischargeSummary> builder)
    {
        builder.ToTable("DischargeSummaries");
        builder.Property(d => d.Disposition).HasMaxLength(100).IsRequired();
        builder.Property(d => d.FinalDiagnosis).HasMaxLength(300).IsRequired();
        builder.Property(d => d.Summary).HasMaxLength(4000).IsRequired();
        builder.Property(d => d.FollowUpInstructions).HasMaxLength(2000);
        builder.Property(d => d.WrittenBy).HasMaxLength(256).IsRequired();
    }
}
