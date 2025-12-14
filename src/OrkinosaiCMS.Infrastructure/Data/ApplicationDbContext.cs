using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OrkinosaiCMS.Core.Entities.Identity;
using OrkinosaiCMS.Core.Entities.Sites;
using OrkinosaiCMS.Core.Entities.Subscriptions;

namespace OrkinosaiCMS.Infrastructure.Data;

/// <summary>
/// Main database context for the OrkinosaiCMS
/// Now integrates with ASP.NET Core Identity following Oqtane's approach
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
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

    // Users and Permissions (legacy tables for backward compatibility)
    // Note: Identity uses AspNetUsers, AspNetRoles, AspNetUserRoles tables
    public DbSet<User> LegacyUsers => Set<User>();
    public DbSet<Role> LegacyRoles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> LegacyUserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    // Content
    public DbSet<Content> Contents => Set<Content>();

    // Subscriptions
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Configure Identity tables with custom names to avoid conflicts
        modelBuilder.Entity<ApplicationUser>().ToTable("AspNetUsers");
        modelBuilder.Entity<IdentityRole<int>>().ToTable("AspNetRoles");
        modelBuilder.Entity<IdentityUserRole<int>>().ToTable("AspNetUserRoles");
        modelBuilder.Entity<IdentityUserClaim<int>>().ToTable("AspNetUserClaims");
        modelBuilder.Entity<IdentityUserLogin<int>>().ToTable("AspNetUserLogins");
        modelBuilder.Entity<IdentityUserToken<int>>().ToTable("AspNetUserTokens");
        modelBuilder.Entity<IdentityRoleClaim<int>>().ToTable("AspNetRoleClaims");

        // Configure soft delete filter for ApplicationUser
        modelBuilder.Entity<ApplicationUser>().HasQueryFilter(e => !e.IsDeleted);

        // Configure soft delete filter for existing entities
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
        modelBuilder.Entity<Customer>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Subscription>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Invoice>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<PaymentMethod>().HasQueryFilter(e => !e.IsDeleted);
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

        // Handle ApplicationUser timestamps
        var userEntries = ChangeTracker.Entries<ApplicationUser>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in userEntries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedOn = DateTime.UtcNow;
            }
            
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.ModifiedOn = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
