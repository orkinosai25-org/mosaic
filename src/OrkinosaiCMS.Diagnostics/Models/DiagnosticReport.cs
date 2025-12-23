namespace OrkinosaiCMS.Diagnostics.Models;

/// <summary>
/// Represents a comprehensive diagnostic report
/// </summary>
public class DiagnosticReport
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string MachineName { get; set; } = "";
    public ApplicationInfo Application { get; set; } = new();
    public Dictionary<string, string> EnvironmentVariables { get; set; } = new();
    public Dictionary<string, object> Configuration { get; set; } = new();
    public List<LogEntry> RecentLogs { get; set; } = new();
    public List<LogEntry> RecentErrors { get; set; } = new();
    public StartupStatus Startup { get; set; } = new();
    public SystemInfo System { get; set; } = new();
}

/// <summary>
/// Application information
/// </summary>
public class ApplicationInfo
{
    public string Name { get; set; } = "";
    public string Version { get; set; } = "";
    public string Environment { get; set; } = "";
    public string ContentRootPath { get; set; } = "";
    public string Status { get; set; } = "";
}

/// <summary>
/// System information
/// </summary>
public class SystemInfo
{
    public string DotNetVersion { get; set; } = "";
    public string OsVersion { get; set; } = "";
    public bool Is64BitProcess { get; set; }
    public int ProcessorCount { get; set; }
    public string WorkingSet { get; set; } = "";
    public string Uptime { get; set; } = "";
}

/// <summary>
/// Startup status information
/// </summary>
public class StartupStatus
{
    public bool IsHealthy { get; set; }
    public string LastKnownError { get; set; } = "";
    public DateTime? LastSuccessfulStart { get; set; }
    public DateTime? LastFailedStart { get; set; }
    public List<string> StartupErrors { get; set; } = new();
}

/// <summary>
/// Log entry
/// </summary>
public class LogEntry
{
    public DateTime Timestamp { get; set; }
    public string Level { get; set; } = "";
    public string Source { get; set; } = "";
    public string Message { get; set; } = "";
    public string RawLine { get; set; } = "";
}
