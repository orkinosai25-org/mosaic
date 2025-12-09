namespace OrkinosaiCMS.Shared.DTOs;

/// <summary>
/// Data transfer object for Theme
/// </summary>
public class ThemeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Version { get; set; } = "1.0.0";
    public string? Author { get; set; }
    public string AssetsPath { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public bool IsEnabled { get; set; }
    public bool IsSystem { get; set; }
    public string Category { get; set; } = "General";
    public string LayoutType { get; set; } = "TopNavigation";
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
    public string? AccentColor { get; set; }
    public string? LogoUrl { get; set; }
    public bool IsMobileResponsive { get; set; }
}

/// <summary>
/// DTO for creating a new theme
/// </summary>
public class CreateThemeDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = "General";
    public string LayoutType { get; set; } = "TopNavigation";
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
    public string? AccentColor { get; set; }
    public string? LogoUrl { get; set; }
}

/// <summary>
/// DTO for updating theme branding
/// </summary>
public class UpdateThemeBrandingDto
{
    public int ThemeId { get; set; }
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
    public string? AccentColor { get; set; }
    public string? LogoUrl { get; set; }
}

/// <summary>
/// DTO for cloning a theme
/// </summary>
public class CloneThemeDto
{
    public int SourceThemeId { get; set; }
    public string NewName { get; set; } = string.Empty;
    public string? NewDescription { get; set; }
}

/// <summary>
/// DTO for applying theme to a site
/// </summary>
public class ApplyThemeDto
{
    public int SiteId { get; set; }
    public int ThemeId { get; set; }
}
