namespace OrkinosaiCMS.Shared.DTOs;

/// <summary>
/// Data transfer object for Site
/// </summary>
public class SiteDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Url { get; set; } = string.Empty;
    public int? ThemeId { get; set; }
    public string? ThemeName { get; set; }
    public string? LogoUrl { get; set; }
    public string? FaviconUrl { get; set; }
    public string AdminEmail { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string DefaultLanguage { get; set; } = "en-US";
    public DateTime CreatedOn { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string Status { get; set; } = "active";
}

/// <summary>
/// DTO for creating a new site
/// </summary>
public class CreateSiteDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Purpose { get; set; }
    public int? ThemeId { get; set; }
    public string AdminEmail { get; set; } = string.Empty;
}

/// <summary>
/// DTO for updating a site
/// </summary>
public class UpdateSiteDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Url { get; set; } = string.Empty;
    public int? ThemeId { get; set; }
    public string? LogoUrl { get; set; }
    public string? FaviconUrl { get; set; }
    public string DefaultLanguage { get; set; } = "en-US";
}

/// <summary>
/// DTO for site provisioning response
/// </summary>
public class SiteProvisioningResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public SiteDto? Site { get; set; }
    public string? CmsDashboardUrl { get; set; }
    public string? ErrorDetails { get; set; }
    public string? StackTrace { get; set; }
}
