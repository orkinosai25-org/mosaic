namespace OrkinosaiCMS.Modules.Abstractions;

/// <summary>
/// Base interface for all CMS modules
/// Inspired by Oqtane's modular architecture
/// </summary>
public interface IModule
{
    /// <summary>
    /// Unique module identifier
    /// </summary>
    string ModuleName { get; }

    /// <summary>
    /// Display title for the module
    /// </summary>
    string Title { get; }

    /// <summary>
    /// Module description
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Module version
    /// </summary>
    string Version { get; }

    /// <summary>
    /// Module category for organization
    /// </summary>
    string Category { get; }

    /// <summary>
    /// Initialize the module with settings
    /// </summary>
    Task InitializeAsync(IDictionary<string, object> settings);

    /// <summary>
    /// Validate module settings
    /// </summary>
    Task<bool> ValidateSettingsAsync(IDictionary<string, object> settings);
}
