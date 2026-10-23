using CarePath.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarePath.Data.Configurations;

public class DrugCategoryConfiguration : IEntityTypeConfiguration<DrugCategory>
{
    public void Configure(EntityTypeBuilder<DrugCategory> builder)
    {
        builder.ToTable("DrugCategories");
        builder.Property(c => c.Name).HasMaxLength(100).IsRequired();
        builder.HasIndex(c => c.Name).IsUnique();

        builder.HasData(
            new DrugCategory { Id = 1, Name = "Analgesic" },
            new DrugCategory { Id = 2, Name = "Antibiotic" },
            new DrugCategory { Id = 3, Name = "Anticoagulant" },
            new DrugCategory { Id = 4, Name = "Antihypertensive" },
            new DrugCategory { Id = 5, Name = "Intravenous fluid" });
    }
}
