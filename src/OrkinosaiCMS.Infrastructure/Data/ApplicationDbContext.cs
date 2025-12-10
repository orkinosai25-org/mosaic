using Microsoft.EntityFrameworkCore;
using OrkinosaiCMS.Core.Entities.Sites;

namespace OrkinosaiCMS.Infrastructure.Data;

/// <summary>
/// Main database context for the OrkinosaiCMS
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Sites
    public DbSet<Site> Sites => Set<Site>();
    public DbSet<Page> Pages => Set<Page>();
    public DbSet<MasterPage> MasterPages => Set<MasterPage>();

    // Modules
    public DbSet<Module> Modules => Set<Module>();
    public DbSet<PageModule> PageModules => Set<PageModule>();

    // Themes
    public DbSet<Theme> Themes => Set<Theme>();

    // Users and Permissions
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    // Content
    public DbSet<Content> Contents => Set<Content>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Configure soft delete filter
        modelBuilder.Entity<Site>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Page>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<MasterPage>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Module>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<PageModule>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Theme>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<User>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Role>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Permission>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<UserRole>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<RolePermission>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Content>().HasQueryFilter(e => !e.IsDeleted);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Automatically set CreatedOn and ModifiedOn
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is Core.Common.BaseEntity && 
                       (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var entity = (Core.Common.BaseEntity)entry.Entity;
            
            if (entry.State == EntityState.Added)
            {
                entity.CreatedOn = DateTime.UtcNow;
            }
            
            if (entry.State == EntityState.Modified)
            {
                entity.ModifiedOn = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
