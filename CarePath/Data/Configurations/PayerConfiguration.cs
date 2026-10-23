using CarePath.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarePath.Data.Configurations;

public class PayerConfiguration : IEntityTypeConfiguration<Payer>
{
    public void Configure(EntityTypeBuilder<Payer> builder)
    {
        builder.ToTable("Payers");
        builder.Property(p => p.Code).HasMaxLength(20).IsRequired();
        builder.HasIndex(p => p.Code).IsUnique();
        builder.Property(p => p.Name).HasMaxLength(150).IsRequired();
        builder.HasIndex(p => p.Name).IsUnique();
        builder.Property(p => p.Phone).HasMaxLength(30);
        builder.Property(p => p.ClaimsEmail).HasMaxLength(200);
        builder.Property(p => p.DefaultCoveragePercent).HasPrecision(5, 2);
    }
}
