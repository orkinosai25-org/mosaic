using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrkinosaiCMS.Core.Entities.Sites;

namespace OrkinosaiCMS.Infrastructure.Data.Configurations;

public class PageConfiguration : IEntityTypeConfiguration<Page>
{
    public void Configure(EntityTypeBuilder<Page> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Path)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasIndex(p => new { p.SiteId, p.Path })
            .IsUnique();

        builder.HasOne(p => p.Parent)
            .WithMany(p => p.Children)
            .HasForeignKey(p => p.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.PageModules)
            .WithOne(pm => pm.Page)
            .HasForeignKey(pm => pm.PageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
