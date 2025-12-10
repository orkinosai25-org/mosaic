using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrkinosaiCMS.Core.Entities.Sites;

namespace OrkinosaiCMS.Infrastructure.Data.Configurations;

public class SiteConfiguration : IEntityTypeConfiguration<Site>
{
    public void Configure(EntityTypeBuilder<Site> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Url)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(s => s.AdminEmail)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(s => s.DefaultLanguage)
            .HasMaxLength(10);

        builder.HasIndex(s => s.Url)
            .IsUnique();

        builder.HasMany(s => s.Pages)
            .WithOne(p => p.Site)
            .HasForeignKey(p => p.SiteId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
