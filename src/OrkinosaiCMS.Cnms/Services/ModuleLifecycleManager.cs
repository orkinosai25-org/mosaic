using Microsoft.Extensions.Logging;
using OrkinosaiCMS.Cnms.Interfaces;
using OrkinosaiCMS.Core.Interfaces.Services;

namespace OrkinosaiCMS.Cnms.Services;

/// <summary>
/// Module lifecycle manager implementation inspired by Oqtane's module lifecycle.
/// Manages module initialization, configuration, and disposal.
/// </summary>
public class ModuleLifecycleManager : IModuleLifecycleManager
{
    private readonly IModuleService _moduleService;
    private readonly ILogger<ModuleLifecycleManager> _logger;
    private readonly Dictionary<int, ModuleState> _moduleStates = new();

    public ModuleLifecycleManager(
        IModuleService moduleService,
        ILogger<ModuleLifecycleManager> logger)
    {
        _moduleService = moduleService;
        _logger = logger;
    }

    public async Task<ModuleInitResult> InitializeAsync(int moduleInstanceId)
    {
        var result = new ModuleInitResult();

        try
        {
            _logger.LogInformation("Initializing module instance {ModuleInstanceId}", moduleInstanceId);

            // Get module definition
            var module = await _moduleService.GetModuleByIdAsync(moduleInstanceId);
            if (module == null)
            {
                result.Success = false;
                result.Errors.Add($"Module instance {moduleInstanceId} not found");
                return result;
            }

            // Check if already initialized
            if (_moduleStates.ContainsKey(moduleInstanceId))
            {
                _logger.LogWarning("Module instance {ModuleInstanceId} already initialized", moduleInstanceId);
                result.Success = true;
                result.InitialState = _moduleStates[moduleInstanceId].State;
                return result;
            }

            // Initialize module state
            var state = new ModuleState
            {
                ModuleInstanceId = moduleInstanceId,
                ModuleName = module.Name,
                InitializedAt = DateTime.UtcNow,
                State = new Dictionary<string, object>()
            };

            _moduleStates[moduleInstanceId] = state;

            result.Success = true;
            result.InitialState = state.State;
            
            _logger.LogInformation("Module instance {ModuleInstanceId} initialized successfully", moduleInstanceId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing module instance {ModuleInstanceId}", moduleInstanceId);
            result.Success = false;
            result.Errors.Add($"Initialization error: {ex.Message}");
            return result;
        }
    }

    public async Task<bool> ConfigureAsync(int moduleInstanceId, Dictionary<string, object> settings)
    {
        try
        {
            _logger.LogInformation("Configuring module instance {ModuleInstanceId} with {Count} settings", 
                moduleInstanceId, settings.Count);

            // Ensure module is initialized
            if (!_moduleStates.ContainsKey(moduleInstanceId))
            {
                var initResult = await InitializeAsync(moduleInstanceId);
                if (!initResult.Success)
                {
                    return false;
                }
            }

            // Update module state with settings
            var state = _moduleStates[moduleInstanceId];
            foreach (var (key, value) in settings)
            {
                state.State[key] = value;
            }

            state.LastConfiguredAt = DateTime.UtcNow;

            _logger.LogInformation("Module instance {ModuleInstanceId} configured successfully", moduleInstanceId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error configuring module instance {ModuleInstanceId}", moduleInstanceId);
            return false;
        }
    }

    public Task<ModuleValidationResult> ValidateConfigurationAsync(int moduleInstanceId)
    {
        var result = new ModuleValidationResult { IsValid = true };

        try
        {
            if (!_moduleStates.ContainsKey(moduleInstanceId))
            {
                result.IsValid = false;
                result.Errors.Add("Module not initialized");
                return Task.FromResult(result);
            }

            var state = _moduleStates[moduleInstanceId];

            // Validate module state
            if (state.State == null || state.State.Count == 0)
            {
                result.Warnings.Add("Module has no configuration settings");
            }

            // Add more validation rules as needed

            result.IsValid = result.Errors.Count == 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating module configuration {ModuleInstanceId}", moduleInstanceId);
            result.IsValid = false;
            result.Errors.Add($"Validation error: {ex.Message}");
        }

        return Task.FromResult(result);
    }

    public Task<bool> DisposeAsync(int moduleInstanceId)
    {
        try
        {
            _logger.LogInformation("Disposing module instance {ModuleInstanceId}", moduleInstanceId);

            if (_moduleStates.Remove(moduleInstanceId))
            {
                _logger.LogInformation("Module instance {ModuleInstanceId} disposed successfully", moduleInstanceId);
                return Task.FromResult(true);
            }

            _logger.LogWarning("Module instance {ModuleInstanceId} not found in state", moduleInstanceId);
            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disposing module instance {ModuleInstanceId}", moduleInstanceId);
            return Task.FromResult(false);
        }
    }

    public Task<IEnumerable<ModuleDependency>> GetDependenciesAsync(int moduleInstanceId)
    {
        // In a real implementation, this would load dependencies from module metadata
        // For now, return empty list
        var dependencies = new List<ModuleDependency>();
        return Task.FromResult<IEnumerable<ModuleDependency>>(dependencies);
    }

    private class ModuleState
    {
        public int ModuleInstanceId { get; set; }
        public string ModuleName { get; set; } = string.Empty;
        public DateTime InitializedAt { get; set; }
        public DateTime? LastConfiguredAt { get; set; }
        public Dictionary<string, object> State { get; set; } = new();
    }
}
