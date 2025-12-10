using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrkinosaiCMS.Core.Entities.Sites;

namespace OrkinosaiCMS.Infrastructure.Data.Configurations;

public class MasterPageConfiguration : IEntityTypeConfiguration<MasterPage>
{
    public void Configure(EntityTypeBuilder<MasterPage> builder)
    {
        builder.HasKey(mp => mp.Id);

        builder.Property(mp => mp.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(mp => mp.ComponentPath)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasOne(mp => mp.Site)
            .WithMany()
            .HasForeignKey(mp => mp.SiteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(mp => new { mp.SiteId, mp.Name })
            .IsUnique();
    }
}
