using CarePath.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarePath.Data.Configurations;

public class EmergencyContactConfiguration : IEntityTypeConfiguration<EmergencyContact>
{
    public void Configure(EntityTypeBuilder<EmergencyContact> builder)
    {
        builder.ToTable("EmergencyContacts");
        builder.Property(c => c.Name).HasMaxLength(150).IsRequired();
        builder.Property(c => c.Relationship).HasMaxLength(50).IsRequired();
        builder.Property(c => c.Phone).HasMaxLength(30).IsRequired();
    }
}
