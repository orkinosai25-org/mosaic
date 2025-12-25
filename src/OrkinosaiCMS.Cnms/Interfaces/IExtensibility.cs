namespace OrkinosaiCMS.Cnms.Interfaces;

/// <summary>
/// Module lifecycle manager inspired by Oqtane's module lifecycle.
/// Provides robust module initialization, configuration, and cleanup.
/// </summary>
public interface IModuleLifecycleManager
{
    /// <summary>
    /// Initializes a module instance.
    /// </summary>
    Task<ModuleInitResult> InitializeAsync(int moduleInstanceId);

    /// <summary>
    /// Configures a module with settings.
    /// </summary>
    Task<bool> ConfigureAsync(int moduleInstanceId, Dictionary<string, object> settings);

    /// <summary>
    /// Validates module configuration.
    /// </summary>
    Task<ModuleValidationResult> ValidateConfigurationAsync(int moduleInstanceId);

    /// <summary>
    /// Disposes/cleans up a module instance.
    /// </summary>
    Task<bool> DisposeAsync(int moduleInstanceId);

    /// <summary>
    /// Gets module dependencies.
    /// </summary>
    Task<IEnumerable<ModuleDependency>> GetDependenciesAsync(int moduleInstanceId);
}

/// <summary>
/// Result of module initialization.
/// </summary>
public class ModuleInitResult
{
    public bool Success { get; set; }
    public List<string> Errors { get; set; } = new();
    public Dictionary<string, object> InitialState { get; set; } = new();
}

/// <summary>
/// Result of module validation.
/// </summary>
public class ModuleValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

/// <summary>
/// Represents a module dependency.
/// </summary>
public class ModuleDependency
{
    public string ModuleName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public bool IsOptional { get; set; }
}

/// <summary>
/// Extension point manager for CMS extensibility.
/// Inspired by Umbraco's extension architecture.
/// </summary>
public interface IExtensionPointManager
{
    /// <summary>
    /// Registers an extension provider.
    /// </summary>
    void RegisterExtension<TExtension>(string extensionPointName, TExtension extension) where TExtension : class;

    /// <summary>
    /// Gets all extensions for a specific extension point.
    /// </summary>
    IEnumerable<TExtension> GetExtensions<TExtension>(string extensionPointName) where TExtension : class;

    /// <summary>
    /// Executes an extension point with context.
    /// </summary>
    Task<ExtensionExecutionResult> ExecuteExtensionPointAsync(string extensionPointName, object context);

    /// <summary>
    /// Gets available extension points.
    /// </summary>
    IEnumerable<ExtensionPointDescriptor> GetAvailableExtensionPoints();
}

/// <summary>
/// Describes an extension point.
/// </summary>
public class ExtensionPointDescriptor
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Type ExtensionType { get; set; } = typeof(object);
    public bool AllowMultiple { get; set; }
}

/// <summary>
/// Result of extension point execution.
/// </summary>
public class ExtensionExecutionResult
{
    public bool Success { get; set; }
    public List<object> Results { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}
