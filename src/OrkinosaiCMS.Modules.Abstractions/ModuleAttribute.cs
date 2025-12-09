namespace OrkinosaiCMS.Modules.Abstractions;

/// <summary>
/// Attribute to mark a class as a CMS module
/// Used for module discovery and registration
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ModuleAttribute : Attribute
{
    /// <summary>
    /// Unique module name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Display title
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Module description
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Module category
    /// </summary>
    public string Category { get; set; }

    /// <summary>
    /// Module version
    /// </summary>
    public string Version { get; set; }

    /// <summary>
    /// Icon CSS class
    /// </summary>
    public string? IconCssClass { get; set; }

    public ModuleAttribute(string name, string title, string description = "", string category = "General", string version = "1.0.0")
    {
        Name = name;
        Title = title;
        Description = description;
        Category = category;
        Version = version;
    }
}
