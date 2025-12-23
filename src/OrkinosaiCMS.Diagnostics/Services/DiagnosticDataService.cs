using OrkinosaiCMS.Diagnostics.Models;
using System.Text.RegularExpressions;
using System.Reflection;

namespace OrkinosaiCMS.Diagnostics.Services;

/// <summary>
/// Service for gathering diagnostic information about the main application
/// even when it's in a failed state
/// </summary>
public interface IDiagnosticDataService
{
    /// <summary>
    /// Generate a comprehensive diagnostic report
    /// </summary>
    Task<DiagnosticReport> GenerateReportAsync(string mainAppPath);
}

public class DiagnosticDataService : IDiagnosticDataService
{
    private readonly ILogger<DiagnosticDataService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;
    
    // Sensitive keys that should be redacted
    private static readonly string[] SensitiveKeys = new[]
    {
        "password", "pwd", "secret", "key", "token", "connectionstring", 
        "apikey", "api_key", "clientsecret", "client_secret", "credential"
    };
    
    public DiagnosticDataService(
        ILogger<DiagnosticDataService> logger,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        _logger = logger;
        _configuration = configuration;
        _environment = environment;
    }
    
    public async Task<DiagnosticReport> GenerateReportAsync(string mainAppPath)
    {
        var report = new DiagnosticReport
        {
            Timestamp = DateTime.UtcNow,
            MachineName = Environment.MachineName
        };
        
        try
        {
            // Gather application info
            report.Application = GetApplicationInfo(mainAppPath);
            
            // Gather system info
            report.System = GetSystemInfo();
            
            // Gather environment variables
            report.EnvironmentVariables = GetEnvironmentVariables();
            
            // Gather configuration from main app
            report.Configuration = await GetMainAppConfigurationAsync(mainAppPath);
            
            // Gather logs from main app
            var logs = await GetRecentLogsAsync(mainAppPath);
            report.RecentLogs = logs.Take(100).ToList();
            report.RecentErrors = logs
                .Where(l => l.Level.Contains("ERR", StringComparison.OrdinalIgnoreCase) || 
                           l.Level.Contains("FTL", StringComparison.OrdinalIgnoreCase) ||
                           l.Level.Contains("FATAL", StringComparison.OrdinalIgnoreCase))
                .Take(50)
                .ToList();
            
            // Gather startup status
            report.Startup = await GetStartupStatusAsync(mainAppPath, logs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating diagnostic report");
        }
        
        return report;
    }
    
    private ApplicationInfo GetApplicationInfo(string mainAppPath)
    {
        var info = new ApplicationInfo
        {
            Name = "OrkinosaiCMS",
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
            ContentRootPath = mainAppPath,
            Status = "Unknown"
        };
        
        try
        {
            // Try to read version from main app assembly if available
            var dllPath = Path.Combine(mainAppPath, "OrkinosaiCMS.Web.dll");
            if (File.Exists(dllPath))
            {
                var assembly = Assembly.LoadFrom(dllPath);
                info.Version = assembly.GetName().Version?.ToString() ?? "Unknown";
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not load main app assembly");
        }
        
        return info;
    }
    
    private SystemInfo GetSystemInfo()
    {
        return new SystemInfo
        {
            DotNetVersion = Environment.Version.ToString(),
            OsVersion = Environment.OSVersion.ToString(),
            Is64BitProcess = Environment.Is64BitProcess,
            ProcessorCount = Environment.ProcessorCount,
            WorkingSet = $"{Environment.WorkingSet / (1024 * 1024)} MB",
            Uptime = TimeSpan.FromMilliseconds(Environment.TickCount64).ToString()
        };
    }
    
    private Dictionary<string, string> GetEnvironmentVariables()
    {
        var envVars = new Dictionary<string, string>();
        
        try
        {
            foreach (var envVar in Environment.GetEnvironmentVariables().Cast<System.Collections.DictionaryEntry>())
            {
                var key = envVar.Key.ToString() ?? "";
                var value = envVar.Value?.ToString() ?? "";
                
                // Redact sensitive environment variables
                if (IsSensitiveKey(key) || IsSensitiveValue(value))
                {
                    envVars[key] = "***REDACTED***";
                }
                else
                {
                    envVars[key] = value;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading environment variables");
            envVars["error"] = $"Error reading environment variables: {ex.Message}";
        }
        
        return envVars;
    }
    
    private async Task<Dictionary<string, object>> GetMainAppConfigurationAsync(string mainAppPath)
    {
        var config = new Dictionary<string, object>();
        
        try
        {
            // Try to read appsettings.json from main app
            var appsettingsPath = Path.Combine(mainAppPath, "appsettings.json");
            if (File.Exists(appsettingsPath))
            {
                var json = await File.ReadAllTextAsync(appsettingsPath);
                var settings = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                if (settings != null)
                {
                    config = RedactSensitiveData(settings);
                }
            }
            
            // Also try Production settings
            var productionSettingsPath = Path.Combine(mainAppPath, "appsettings.Production.json");
            if (File.Exists(productionSettingsPath))
            {
                var json = await File.ReadAllTextAsync(productionSettingsPath);
                var settings = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                if (settings != null)
                {
                    config["ProductionSettings"] = RedactSensitiveData(settings);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading main app configuration");
            config["error"] = $"Error reading configuration: {ex.Message}";
        }
        
        return config;
    }
    
    private Dictionary<string, object> RedactSensitiveData(Dictionary<string, object> data)
    {
        var result = new Dictionary<string, object>();
        
        foreach (var kvp in data)
        {
            if (IsSensitiveKey(kvp.Key))
            {
                result[kvp.Key] = "***REDACTED***";
            }
            else if (kvp.Value is Dictionary<string, object> nested)
            {
                result[kvp.Key] = RedactSensitiveData(nested);
            }
            else if (kvp.Value is string strValue && IsSensitiveValue(strValue))
            {
                result[kvp.Key] = "***REDACTED***";
            }
            else
            {
                result[kvp.Key] = kvp.Value;
            }
        }
        
        return result;
    }
    
    private async Task<List<LogEntry>> GetRecentLogsAsync(string mainAppPath)
    {
        var logs = new List<LogEntry>();
        
        try
        {
            var logDirectory = Path.Combine(mainAppPath, "App_Data", "Logs");
            
            if (!Directory.Exists(logDirectory))
            {
                _logger.LogWarning("Log directory does not exist: {LogDirectory}", logDirectory);
                return logs;
            }
            
            // Find the most recent log files
            var logFiles = Directory.GetFiles(logDirectory, "*.log")
                .OrderByDescending(f => File.GetLastWriteTimeUtc(f))
                .Take(3)
                .ToList();
            
            if (!logFiles.Any())
            {
                _logger.LogWarning("No log files found in {LogDirectory}", logDirectory);
                return logs;
            }
            
            foreach (var logFile in logFiles)
            {
                try
                {
                    var lines = await File.ReadAllLinesAsync(logFile);
                    var recentLines = lines.Reverse().Take(200).Reverse();
                    
                    foreach (var line in recentLines)
                    {
                        if (string.IsNullOrWhiteSpace(line))
                            continue;
                            
                        var entry = ParseLogEntry(line);
                        if (entry != null)
                        {
                            logs.Add(entry);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error reading log file: {LogFile}", logFile);
                }
            }
            
            // Sort by timestamp descending (most recent first)
            logs = logs.OrderByDescending(l => l.Timestamp).Take(200).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading logs");
        }
        
        return logs;
    }
    
    private async Task<StartupStatus> GetStartupStatusAsync(string mainAppPath, List<LogEntry> logs)
    {
        var status = new StartupStatus();
        
        try
        {
            // Look for startup errors in recent logs
            var startupErrors = logs
                .Where(l => 
                    (l.Level.Contains("ERR", StringComparison.OrdinalIgnoreCase) || 
                     l.Level.Contains("FTL", StringComparison.OrdinalIgnoreCase)) &&
                    (l.Message.Contains("startup", StringComparison.OrdinalIgnoreCase) ||
                     l.Message.Contains("failed to start", StringComparison.OrdinalIgnoreCase) ||
                     l.Message.Contains("500.30", StringComparison.OrdinalIgnoreCase) ||
                     l.Message.Contains("database", StringComparison.OrdinalIgnoreCase)))
                .Select(l => l.Message)
                .Take(10)
                .ToList();
            
            status.StartupErrors = startupErrors;
            status.IsHealthy = startupErrors.Count == 0;
            
            if (startupErrors.Any())
            {
                status.LastKnownError = startupErrors.First();
                status.LastFailedStart = logs
                    .Where(l => l.Level.Contains("ERR", StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(l => l.Timestamp)
                    .FirstOrDefault()?.Timestamp;
            }
            else
            {
                status.LastSuccessfulStart = logs
                    .Where(l => l.Message.Contains("started successfully", StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(l => l.Timestamp)
                    .FirstOrDefault()?.Timestamp;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error determining startup status");
        }
        
        return status;
    }
    
    private LogEntry? ParseLogEntry(string line)
    {
        try
        {
            // Parse Serilog format: [timestamp] [level] [source] message
            var match = Regex.Match(line, @"\[([^\]]+)\]\s*\[([^\]]+)\]\s*(?:\[([^\]]+)\])?\s*(.*)");
            
            if (match.Success)
            {
                var timestampStr = match.Groups[1].Value;
                var level = match.Groups[2].Value;
                var source = match.Groups.Count >= 4 && !string.IsNullOrEmpty(match.Groups[3].Value) 
                    ? match.Groups[3].Value 
                    : "";
                var message = match.Groups.Count >= 5 && !string.IsNullOrEmpty(match.Groups[4].Value)
                    ? match.Groups[4].Value 
                    : line;
                
                if (DateTime.TryParse(timestampStr, out var timestamp))
                {
                    return new LogEntry
                    {
                        Timestamp = timestamp,
                        Level = level,
                        Source = source,
                        Message = message,
                        RawLine = line
                    };
                }
            }
            
            // If parsing fails, return raw line as message
            return new LogEntry
            {
                Timestamp = DateTime.UtcNow,
                Level = "UNKNOWN",
                Source = "",
                Message = line,
                RawLine = line
            };
        }
        catch
        {
            return null;
        }
    }
    
    private bool IsSensitiveKey(string key)
    {
        return SensitiveKeys.Any(sk => key.Contains(sk, StringComparison.OrdinalIgnoreCase));
    }
    
    private bool IsSensitiveValue(string value)
    {
        if (value.Length > 50 && (
            value.Contains("Server=", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("Password=", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("Pwd=", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("Secret", StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }
        
        return false;
    }
}
