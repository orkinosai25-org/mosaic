using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Reflection;
using System.Text.RegularExpressions;

namespace OrkinosaiCMS.Web.Services;

/// <summary>
/// Service for gathering diagnostic information about the application
/// including configuration, environment, and system status.
/// </summary>
public interface IDiagnosticsService
{
    /// <summary>
    /// Get all configuration settings with sensitive data redacted
    /// </summary>
    Task<Dictionary<string, object>> GetConfigurationAsync();
    
    /// <summary>
    /// Get environment variables with sensitive data redacted
    /// </summary>
    Dictionary<string, string> GetEnvironmentVariables();
    
    /// <summary>
    /// Get comprehensive application status
    /// </summary>
    Task<Dictionary<string, object>> GetApplicationStatusAsync();
    
    /// <summary>
    /// Get recent log entries from configured log files
    /// </summary>
    Task<List<LogEntry>> GetRecentLogsAsync(int maxLines = 100);
}

public class DiagnosticsService : IDiagnosticsService
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;
    private readonly HealthCheckService _healthCheckService;
    private readonly ILogger<DiagnosticsService> _logger;
    
    // Sensitive keys that should be redacted
    private static readonly string[] SensitiveKeys = new[]
    {
        "password", "pwd", "secret", "key", "token", "connectionstring", 
        "apikey", "api_key", "clientsecret", "client_secret", "credential"
    };
    
    // Configuration constants for diagnostic behavior
    private const int MaxLogFilesToCheck = 3;  // Number of recent log files to search
    private const int ConnectionStringMinLength = 50;  // Minimum length to trigger connection string detection
    
    public DiagnosticsService(
        IConfiguration configuration,
        IWebHostEnvironment environment,
        HealthCheckService healthCheckService,
        ILogger<DiagnosticsService> logger)
    {
        _configuration = configuration;
        _environment = environment;
        _healthCheckService = healthCheckService;
        _logger = logger;
    }
    
    public async Task<Dictionary<string, object>> GetConfigurationAsync()
    {
        var config = new Dictionary<string, object>();
        
        try
        {
            // Get all configuration sections
            var root = _configuration as IConfigurationRoot;
            if (root != null)
            {
                foreach (var provider in root.Providers)
                {
                    var providerName = provider.GetType().Name;
                    _logger.LogDebug("Reading configuration from provider: {Provider}", providerName);
                }
            }
            
            // Build configuration tree with redaction
            config = BuildConfigurationTree(_configuration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading configuration");
            config["error"] = $"Error reading configuration: {ex.Message}";
        }
        
        return config;
    }
    
    private Dictionary<string, object> BuildConfigurationTree(IConfiguration configuration)
    {
        var result = new Dictionary<string, object>();
        
        foreach (var child in configuration.GetChildren())
        {
            var key = child.Key;
            var path = child.Path;
            
            // Check if this is a sensitive key
            if (IsSensitiveKey(key) || IsSensitiveKey(path))
            {
                result[key] = "***REDACTED***";
            }
            else if (child.GetChildren().Any())
            {
                // This is a section, recurse into it
                result[key] = BuildConfigurationTree(child);
            }
            else
            {
                // This is a value
                var value = child.Value;
                if (value != null && IsSensitiveValue(value))
                {
                    result[key] = "***REDACTED***";
                }
                else
                {
                    result[key] = value ?? "(null)";
                }
            }
        }
        
        return result;
    }
    
    private bool IsSensitiveKey(string key)
    {
        return SensitiveKeys.Any(sk => key.Contains(sk, StringComparison.OrdinalIgnoreCase));
    }
    
    private bool IsSensitiveValue(string value)
    {
        // Redact values that look like connection strings, keys, or tokens
        // Use ConnectionStringMinLength threshold to avoid false positives on short strings
        if (value.Length > ConnectionStringMinLength && (
            value.Contains("Server=", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("Password=", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("Pwd=", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("Secret", StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }
        
        return false;
    }
    
    public Dictionary<string, string> GetEnvironmentVariables()
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
    
    public async Task<Dictionary<string, object>> GetApplicationStatusAsync()
    {
        var status = new Dictionary<string, object>();
        
        try
        {
            // Application info
            status["applicationName"] = Assembly.GetEntryAssembly()?.GetName().Name ?? "Unknown";
            status["version"] = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "Unknown";
            status["environment"] = _environment.EnvironmentName;
            status["contentRootPath"] = _environment.ContentRootPath;
            status["webRootPath"] = _environment.WebRootPath;
            status["timestamp"] = DateTime.UtcNow;
            
            // Runtime info
            status["dotnetVersion"] = Environment.Version.ToString();
            status["osVersion"] = Environment.OSVersion.ToString();
            status["is64BitProcess"] = Environment.Is64BitProcess;
            status["processorCount"] = Environment.ProcessorCount;
            status["machineName"] = Environment.MachineName;
            status["uptime"] = TimeSpan.FromMilliseconds(Environment.TickCount64).ToString();
            
            // Memory info
            status["workingSet"] = $"{Environment.WorkingSet / (1024 * 1024)} MB";
            
            // Health checks
            var healthReport = await _healthCheckService.CheckHealthAsync();
            status["healthStatus"] = healthReport.Status.ToString();
            status["healthChecks"] = healthReport.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration,
                exception = e.Value.Exception?.Message
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error gathering application status");
            status["error"] = $"Error gathering status: {ex.Message}";
        }
        
        return status;
    }
    
    public async Task<List<LogEntry>> GetRecentLogsAsync(int maxLines = 100)
    {
        var logs = new List<LogEntry>();
        
        try
        {
            // Get log directory from configuration or use default
            var logDirectory = Path.Combine(_environment.ContentRootPath, "App_Data", "Logs");
            
            if (!Directory.Exists(logDirectory))
            {
                _logger.LogWarning("Log directory does not exist: {LogDirectory}", logDirectory);
                return logs;
            }
            
            // Find the most recent log file
            var logFiles = Directory.GetFiles(logDirectory, "*.log")
                .OrderByDescending(f => File.GetLastWriteTimeUtc(f))
                .Take(MaxLogFilesToCheck)  // Check last N log files for complete recent history
                .ToList();
            
            if (!logFiles.Any())
            {
                _logger.LogWarning("No log files found in {LogDirectory}", logDirectory);
                return logs;
            }
            
            var lineCount = 0;
            
            foreach (var logFile in logFiles)
            {
                if (lineCount >= maxLines)
                    break;
                    
                try
                {
                    // Read log file in reverse order to get most recent entries first
                    var lines = await File.ReadAllLinesAsync(logFile);
                    var recentLines = lines.Reverse().Take(maxLines - lineCount).Reverse();
                    
                    foreach (var line in recentLines)
                    {
                        if (string.IsNullOrWhiteSpace(line))
                            continue;
                            
                        // Parse log entry
                        var entry = ParseLogEntry(line);
                        if (entry != null)
                        {
                            logs.Add(entry);
                            lineCount++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error reading log file: {LogFile}", logFile);
                }
            }
            
            // Sort by timestamp descending (most recent first)
            logs = logs.OrderByDescending(l => l.Timestamp).Take(maxLines).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading logs");
        }
        
        return logs;
    }
    
    private LogEntry? ParseLogEntry(string line)
    {
        try
        {
            // Parse Serilog format: [timestamp] [level] [source] message
            // Example: [2025-12-23 16:19:41.621 +00:00] [INF] [OrkinosaiCMS.Web.Program] Application started
            var match = Regex.Match(line, @"\[([^\]]+)\]\s*\[([^\]]+)\]\s*(?:\[([^\]]+)\])?\s*(.*)");
            
            if (match.Success)
            {
                var timestampStr = match.Groups[1].Value;
                var level = match.Groups[2].Value;
                // Group 3 is optional source context (may be empty)
                var source = match.Groups.Count >= 4 && !string.IsNullOrEmpty(match.Groups[3].Value) 
                    ? match.Groups[3].Value 
                    : "";
                // Group 4 is the message (when source is present) or the content after level (when source is absent)
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
}

public class LogEntry
{
    public DateTime Timestamp { get; set; }
    public string Level { get; set; } = "";
    public string Source { get; set; } = "";
    public string Message { get; set; } = "";
    public string RawLine { get; set; } = "";
}
