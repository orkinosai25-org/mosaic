namespace OrkinosaiCMS.Cnms.Interfaces;

/// <summary>
/// Master page renderer inspired by SharePoint and Umbraco master page concepts.
/// Provides robust master page rendering with content slot management.
/// </summary>
public interface IMasterPageRenderer
{
    /// <summary>
    /// Renders a master page with content in specified slots.
    /// </summary>
    Task<RenderResult> RenderMasterPageAsync(MasterPageRenderContext context);

    /// <summary>
    /// Gets available slots for a master page.
    /// </summary>
    Task<IEnumerable<ContentSlot>> GetAvailableSlotsAsync(string masterPageName);

    /// <summary>
    /// Registers content for a specific slot.
    /// </summary>
    void RegisterSlotContent(string slotName, Func<Task<string>> contentProvider);

    /// <summary>
    /// Validates master page structure and slots.
    /// </summary>
    Task<MasterPageValidationResult> ValidateMasterPageAsync(string masterPageName);
}

/// <summary>
/// Context for rendering a master page.
/// </summary>
public class MasterPageRenderContext
{
    public string MasterPageName { get; set; } = string.Empty;
    public int PageId { get; set; }
    public int SiteId { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new();
    public Dictionary<string, Func<Task<string>>> SlotContent { get; set; } = new();
}

/// <summary>
/// Represents a content slot in a master page.
/// </summary>
public class ContentSlot
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public string DefaultContent { get; set; } = string.Empty;
}

/// <summary>
/// Result of master page rendering.
/// </summary>
public class RenderResult
{
    public bool Success { get; set; }
    public string Html { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// Result of master page validation.
/// </summary>
public class MasterPageValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public List<ContentSlot> Slots { get; set; } = new();
}
