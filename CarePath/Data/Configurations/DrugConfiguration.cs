using CarePath.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarePath.Data.Configurations;

public class DrugConfiguration : IEntityTypeConfiguration<Drug>
{
    public void Configure(EntityTypeBuilder<Drug> builder)
    {
        builder.ToTable("Drugs");
        builder.Property(d => d.Code).HasMaxLength(20).IsRequired();
        builder.HasIndex(d => d.Code).IsUnique();
        builder.Property(d => d.Name).HasMaxLength(150).IsRequired();
        builder.Property(d => d.Strength).HasMaxLength(50).IsRequired();
        builder.Property(d => d.Form).HasMaxLength(50).IsRequired();
        builder.Property(d => d.UnitPrice).HasPrecision(18, 2);
        builder.Ignore(d => d.DisplayName);

        builder.HasOne(d => d.Category).WithMany(c => c.Drugs).HasForeignKey(d => d.DrugCategoryId);
    }
}
