using Microsoft.Extensions.Logging;
using OrkinosaiCMS.Cnms.Interfaces;

namespace OrkinosaiCMS.Cnms.Services;

/// <summary>
/// Extension point manager implementation inspired by Umbraco's extension architecture.
/// Provides a flexible plugin system for CMS extensibility.
/// </summary>
public class ExtensionPointManager : IExtensionPointManager
{
    private readonly ILogger<ExtensionPointManager> _logger;
    private readonly Dictionary<string, List<object>> _extensions = new();
    private readonly Dictionary<string, ExtensionPointDescriptor> _extensionPoints = new();

    public ExtensionPointManager(ILogger<ExtensionPointManager> logger)
    {
        _logger = logger;
        InitializeDefaultExtensionPoints();
    }

    public void RegisterExtension<TExtension>(string extensionPointName, TExtension extension) 
        where TExtension : class
    {
        if (extension == null)
        {
            throw new ArgumentNullException(nameof(extension));
        }

        if (!_extensionPoints.ContainsKey(extensionPointName))
        {
            _logger.LogWarning("Extension point '{ExtensionPoint}' not found, creating dynamic extension point", 
                extensionPointName);
            
            _extensionPoints[extensionPointName] = new ExtensionPointDescriptor
            {
                Name = extensionPointName,
                Description = "Dynamically created extension point",
                ExtensionType = typeof(TExtension),
                AllowMultiple = true
            };
        }

        if (!_extensions.ContainsKey(extensionPointName))
        {
            _extensions[extensionPointName] = new List<object>();
        }

        _extensions[extensionPointName].Add(extension);
        _logger.LogInformation("Registered extension of type {Type} for extension point '{ExtensionPoint}'", 
            typeof(TExtension).Name, extensionPointName);
    }

    public IEnumerable<TExtension> GetExtensions<TExtension>(string extensionPointName) 
        where TExtension : class
    {
        if (!_extensions.TryGetValue(extensionPointName, out var extensions))
        {
            return Enumerable.Empty<TExtension>();
        }

        return extensions.OfType<TExtension>();
    }

    public async Task<ExtensionExecutionResult> ExecuteExtensionPointAsync(string extensionPointName, object context)
    {
        var result = new ExtensionExecutionResult { Success = true };

        try
        {
            if (!_extensions.TryGetValue(extensionPointName, out var extensions))
            {
                _logger.LogWarning("No extensions registered for extension point '{ExtensionPoint}'", 
                    extensionPointName);
                return result;
            }

            _logger.LogInformation("Executing {Count} extensions for extension point '{ExtensionPoint}'", 
                extensions.Count, extensionPointName);

            foreach (var extension in extensions)
            {
                try
                {
                    // If extension implements IAsyncExtension, execute it
                    if (extension is IAsyncExtension asyncExtension)
                    {
                        var extensionResult = await asyncExtension.ExecuteAsync(context);
                        result.Results.Add(extensionResult);
                    }
                    // If extension implements ISyncExtension, execute it synchronously
                    else if (extension is ISyncExtension syncExtension)
                    {
                        var extensionResult = syncExtension.Execute(context);
                        result.Results.Add(extensionResult);
                    }
                    else
                    {
                        _logger.LogWarning("Extension does not implement IAsyncExtension or ISyncExtension");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error executing extension of type {Type}", extension.GetType().Name);
                    result.Errors.Add($"Extension error: {ex.Message}");
                    result.Success = false;
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing extension point '{ExtensionPoint}'", extensionPointName);
            result.Success = false;
            result.Errors.Add($"Execution error: {ex.Message}");
            return result;
        }
    }

    public IEnumerable<ExtensionPointDescriptor> GetAvailableExtensionPoints()
    {
        return _extensionPoints.Values;
    }

    private void InitializeDefaultExtensionPoints()
    {
        // Content rendering extension point
        _extensionPoints["content-rendering"] = new ExtensionPointDescriptor
        {
            Name = "content-rendering",
            Description = "Allows extensions to modify content before rendering",
            ExtensionType = typeof(IContentRenderingExtension),
            AllowMultiple = true
        };

        // Theme loading extension point
        _extensionPoints["theme-loading"] = new ExtensionPointDescriptor
        {
            Name = "theme-loading",
            Description = "Allows extensions to customize theme loading behavior",
            ExtensionType = typeof(IThemeLoadingExtension),
            AllowMultiple = true
        };

        // Module initialization extension point
        _extensionPoints["module-init"] = new ExtensionPointDescriptor
        {
            Name = "module-init",
            Description = "Allows extensions to participate in module initialization",
            ExtensionType = typeof(IModuleInitExtension),
            AllowMultiple = true
        };

        // Authentication extension point (similar to Oqtane)
        _extensionPoints["authentication"] = new ExtensionPointDescriptor
        {
            Name = "authentication",
            Description = "Allows custom authentication providers",
            ExtensionType = typeof(IAuthenticationExtension),
            AllowMultiple = false
        };

        _logger.LogInformation("Initialized {Count} default extension points", _extensionPoints.Count);
    }
}

/// <summary>
/// Base interface for async extensions.
/// </summary>
public interface IAsyncExtension
{
    Task<object> ExecuteAsync(object context);
}

/// <summary>
/// Base interface for sync extensions.
/// </summary>
public interface ISyncExtension
{
    object Execute(object context);
}

/// <summary>
/// Content rendering extension interface.
/// </summary>
public interface IContentRenderingExtension : IAsyncExtension
{
    string Name { get; }
    int Priority { get; }
}

/// <summary>
/// Theme loading extension interface.
/// </summary>
public interface IThemeLoadingExtension : IAsyncExtension
{
    string Name { get; }
}

/// <summary>
/// Module initialization extension interface.
/// </summary>
public interface IModuleInitExtension : IAsyncExtension
{
    string Name { get; }
}

/// <summary>
/// Authentication extension interface.
/// </summary>
public interface IAuthenticationExtension : IAsyncExtension
{
    string ProviderName { get; }
}
