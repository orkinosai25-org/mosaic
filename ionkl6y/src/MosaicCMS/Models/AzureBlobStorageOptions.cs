namespace MosaicCMS.Models;

/// <summary>
/// Configuration options for Azure Blob Storage integration
/// </summary>
public class AzureBlobStorageOptions
{
    /// <summary>
    /// Azure Storage Account Name
    /// </summary>
    public string AccountName { get; set; } = string.Empty;

    /// <summary>
    /// Primary Blob Storage endpoint
    /// </summary>
    public string PrimaryEndpoint { get; set; } = string.Empty;

    /// <summary>
    /// Azure region where storage account is deployed
    /// </summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Storage account SKU (e.g., Standard_RAGRS)
    /// </summary>
    public string SKU { get; set; } = string.Empty;

    /// <summary>
    /// Full Azure Resource ID for the storage account
    /// </summary>
    public string ResourceId { get; set; } = string.Empty;

    /// <summary>
    /// All available service endpoints
    /// </summary>
    public EndpointsOptions Endpoints { get; set; } = new();

    /// <summary>
    /// Security settings for the storage account
    /// </summary>
    public SecurityOptions Security { get; set; } = new();

    /// <summary>
    /// Container names for different types of content
    /// </summary>
    public ContainersOptions Containers { get; set; } = new();

    /// <summary>
    /// Key name for connection string in configuration or Key Vault
    /// </summary>
    public string ConnectionStringKey { get; set; } = string.Empty;

    /// <summary>
    /// Maximum file size in bytes (default 10 MB)
    /// </summary>
    public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024;
}

public class EndpointsOptions
{
    public string Blob { get; set; } = string.Empty;
    public string File { get; set; } = string.Empty;
    public string Queue { get; set; } = string.Empty;
    public string Table { get; set; } = string.Empty;
    public string Dfs { get; set; } = string.Empty;
}

public class SecurityOptions
{
    public bool PublicAccess { get; set; }
    public bool EncryptionEnabled { get; set; }
    public bool FileSharesEnabled { get; set; }
    public string MinimumTlsVersion { get; set; } = string.Empty;
    public bool AllowBlobPublicAccess { get; set; }
    public bool SupportsHttpsTrafficOnly { get; set; }
}

public class ContainersOptions
{
    public string MediaAssets { get; set; } = string.Empty;
    public string UserUploads { get; set; } = string.Empty;
    public string Documents { get; set; } = string.Empty;
    public string Backups { get; set; } = string.Empty;
    public string Images { get; set; } = string.Empty;
}
