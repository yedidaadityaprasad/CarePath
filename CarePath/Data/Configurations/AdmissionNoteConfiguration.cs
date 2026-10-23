using CarePath.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarePath.Data.Configurations;

public class AdmissionNoteConfiguration : IEntityTypeConfiguration<AdmissionNote>
{
    public void Configure(EntityTypeBuilder<AdmissionNote> builder)
    {
        builder.ToTable("AdmissionNotes");
        builder.Property(n => n.Text).HasMaxLength(4000).IsRequired();
        builder.Property(n => n.Author).HasMaxLength(256).IsRequired();
    }
}
