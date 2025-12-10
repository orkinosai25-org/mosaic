using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrkinosaiCMS.Core.Entities.Sites;

namespace OrkinosaiCMS.Infrastructure.Data.Configurations;

public class ThemeConfiguration : IEntityTypeConfiguration<Theme>
{
    public void Configure(EntityTypeBuilder<Theme> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Version)
            .HasMaxLength(50);

        builder.Property(t => t.AssetsPath)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasIndex(t => t.Name)
            .IsUnique();
    }
}
