using CarePath.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarePath.Data.Configurations;

public class WardStockConfiguration : IEntityTypeConfiguration<WardStock>
{
    public void Configure(EntityTypeBuilder<WardStock> builder)
    {
        builder.ToTable("WardStocks");
        builder.HasIndex(s => new { s.WardId, s.DrugId }).IsUnique();
        builder.Property(s => s.QuantityOnHand).IsConcurrencyToken();
        builder.Ignore(s => s.BelowReorderLevel);

        builder.HasOne(s => s.Ward).WithMany(w => w.Stock).HasForeignKey(s => s.WardId);
        builder.HasOne(s => s.Drug).WithMany(d => d.Stock).HasForeignKey(s => s.DrugId);
        builder.HasMany(s => s.Movements).WithOne(m => m.WardStock).HasForeignKey(m => m.WardStockId);
    }
}
