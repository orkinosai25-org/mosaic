using OrkinosaiCMS.Core.Common;

namespace OrkinosaiCMS.Core.Entities.Sites;

/// <summary>
/// Represents a module definition (like SharePoint Web Parts)
/// This is the module template/type, not an instance
/// </summary>
public class Module : BaseEntity
{
    /// <summary>
    /// Unique module name (used for loading)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Display title for the module
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Module description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Category for organizing modules
    /// </summary>
    public string Category { get; set; } = "General";

    /// <summary>
    /// Version of the module
    /// </summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// Assembly name where the module is defined
    /// </summary>
    public string AssemblyName { get; set; } = string.Empty;

    /// <summary>
    /// Fully qualified type name of the module component
    /// </summary>
    public string ComponentType { get; set; } = string.Empty;

    /// <summary>
    /// Icon CSS class
    /// </summary>
    public string? IconCssClass { get; set; }

    /// <summary>
    /// Whether the module is enabled
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Whether the module is a system module
    /// </summary>
    public bool IsSystem { get; set; }

    /// <summary>
    /// Default settings as JSON
    /// </summary>
    public string? DefaultSettings { get; set; }

    /// <summary>
    /// Permissions required to use this module
    /// </summary>
    public string? RequiredPermissions { get; set; }
}
