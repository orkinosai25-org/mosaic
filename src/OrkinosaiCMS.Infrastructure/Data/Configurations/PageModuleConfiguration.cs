using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrkinosaiCMS.Core.Entities.Sites;

namespace OrkinosaiCMS.Infrastructure.Data.Configurations;

public class PageModuleConfiguration : IEntityTypeConfiguration<PageModule>
{
    public void Configure(EntityTypeBuilder<PageModule> builder)
    {
        builder.HasKey(pm => pm.Id);

        builder.Property(pm => pm.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(pm => pm.Zone)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasOne(pm => pm.Module)
            .WithMany()
            .HasForeignKey(pm => pm.ModuleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(pm => new { pm.PageId, pm.Zone, pm.Order });
    }
}
