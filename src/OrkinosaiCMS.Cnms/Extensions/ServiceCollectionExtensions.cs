using Microsoft.Extensions.DependencyInjection;
using OrkinosaiCMS.Cnms.Interfaces;
using OrkinosaiCMS.Cnms.Services;

namespace OrkinosaiCMS.Cnms.Extensions;

/// <summary>
/// Dependency injection extensions for the Core CMS Module (cnms).
/// Provides easy registration of all cnms services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all Core CMS Module (cnms) services to the service collection.
    /// Includes theme provider, layout engine, master page renderer, and extensibility infrastructure.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddCoreCmsModule(this IServiceCollection services)
    {
        // Theme management
        services.AddScoped<IThemeProvider, ThemeProvider>();

        // Layout engine
        services.AddScoped<ILayoutEngine, LayoutEngine>();

        // Master page rendering
        services.AddScoped<IMasterPageRenderer, MasterPageRenderer>();

        // Module lifecycle management
        services.AddScoped<IModuleLifecycleManager, ModuleLifecycleManager>();

        // Extensibility infrastructure
        services.AddSingleton<IExtensionPointManager, ExtensionPointManager>();

        return services;
    }

    /// <summary>
    /// Adds Core CMS Module with custom configuration.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configure">Configuration action</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddCoreCmsModule(
        this IServiceCollection services,
        Action<CnmsOptions> configure)
    {
        services.AddCoreCmsModule();
        services.Configure(configure);
        return services;
    }
}

/// <summary>
/// Configuration options for the Core CMS Module.
/// </summary>
public class CnmsOptions
{
    /// <summary>
    /// Enable or disable theme caching.
    /// </summary>
    public bool EnableThemeCaching { get; set; } = true;

    /// <summary>
    /// Enable or disable layout validation.
    /// </summary>
    public bool EnableLayoutValidation { get; set; } = true;

    /// <summary>
    /// Default theme name.
    /// </summary>
    public string DefaultTheme { get; set; } = "default";

    /// <summary>
    /// Default master page name.
    /// </summary>
    public string DefaultMasterPage { get; set; } = "standard";

    /// <summary>
    /// Default layout template.
    /// </summary>
    public string DefaultLayout { get; set; } = "single-column";

    /// <summary>
    /// Enable or disable module lifecycle logging.
    /// </summary>
    public bool EnableModuleLifecycleLogging { get; set; } = true;

    /// <summary>
    /// Extension point execution timeout in seconds.
    /// </summary>
    public int ExtensionPointTimeoutSeconds { get; set; } = 30;
}
