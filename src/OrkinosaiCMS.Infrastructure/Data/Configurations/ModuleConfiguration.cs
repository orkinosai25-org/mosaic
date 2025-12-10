using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrkinosaiCMS.Core.Entities.Sites;

namespace OrkinosaiCMS.Infrastructure.Data.Configurations;

public class ModuleConfiguration : IEntityTypeConfiguration<Module>
{
    public void Configure(EntityTypeBuilder<Module> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.Category)
            .HasMaxLength(100);

        builder.Property(m => m.Version)
            .HasMaxLength(50);

        builder.Property(m => m.AssemblyName)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(m => m.ComponentType)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasIndex(m => m.Name)
            .IsUnique();
    }
}
